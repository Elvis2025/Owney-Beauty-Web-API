using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;
using Volo.Abp.Settings;
using Volo.Abp.Timing;
using Volo.Abp.Uow;
using Volo.Abp.Users;
using Ieh.OwneyBeauty.Localization;
using Ieh.OwneyBeauty.Modules.Platform;
using Ieh.OwneyBeauty.Modules.Platform.Models;

namespace Ieh.OwneyBeauty.Infrastructure;

/// <summary>
/// PLT-F08. Si una solicitud de escritura a /api trae el encabezado <c>Idempotency-Key</c>:
///  - la primera vez se ejecuta y se guarda la respuesta;
///  - los reintentos con la misma clave y el mismo contenido reciben la misma respuesta
///    (encabezado <c>Idempotent-Replayed: true</c>) sin volver a ejecutar la operación;
///  - la misma clave con contenido distinto devuelve 422; mientras la primera sigue en curso, 409.
/// Las respuestas 5xx no se guardan, para permitir reintentar.
/// El cliente debe enviar un UUID nuevo por cada operación lógica (p. ej. cada intento de pago).
/// </summary>
public class IdempotencyMiddleware : IMiddleware, ITransientDependency
{
    public const string HeaderName = "Idempotency-Key";
    public const string ReplayedHeaderName = "Idempotent-Replayed";
    private const int MaxStoredBodyBytes = 256 * 1024;
    private static readonly TimeSpan InProgressTimeout = TimeSpan.FromMinutes(2);

    private readonly IRepository<IdempotencyRecord, Guid> _repository;
    private readonly IUnitOfWorkManager _unitOfWorkManager;
    private readonly ICurrentUser _currentUser;
    private readonly IClock _clock;
    private readonly IGuidGenerator _guidGenerator;
    private readonly ISettingProvider _settingProvider;
    private readonly IStringLocalizer<OwneyBeautyResource> _l;
    private readonly ILogger<IdempotencyMiddleware> _logger;

    public IdempotencyMiddleware(
        IRepository<IdempotencyRecord, Guid> repository,
        IUnitOfWorkManager unitOfWorkManager,
        ICurrentUser currentUser,
        IClock clock,
        IGuidGenerator guidGenerator,
        ISettingProvider settingProvider,
        IStringLocalizer<OwneyBeautyResource> localizer,
        ILogger<IdempotencyMiddleware> logger)
    {
        _repository = repository;
        _unitOfWorkManager = unitOfWorkManager;
        _currentUser = currentUser;
        _clock = clock;
        _guidGenerator = guidGenerator;
        _settingProvider = settingProvider;
        _l = localizer;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        if (!ShouldHandle(context, out var key))
        {
            await next(context);
            return;
        }

        var scope = _currentUser.Id?.ToString("N") ?? "anon:" + (context.Connection.RemoteIpAddress?.ToString() ?? "unknown");
        var hash = await ComputeRequestHashAsync(context.Request);

        var existing = await FindAsync(scope, key);
        if (existing is not null)
        {
            await HandleExistingAsync(context, existing, hash);
            return;
        }

        IdempotencyRecord record;
        try
        {
            record = await CreateAsync(scope, key, context.Request, hash);
        }
        catch (Exception ex) when (IsUniqueViolation(ex))
        {
            await WriteErrorAsync(context, StatusCodes.Status409Conflict, PlatformErrorCodes.IdempotentRequestInProgress);
            return;
        }

        var originalBody = context.Response.Body;
        await using var buffer = new MemoryStream();
        context.Response.Body = buffer;
        try
        {
            await next(context);

            buffer.Position = 0;
            if (context.Response.StatusCode >= 500)
            {
                await DeleteAsync(record.Id);
            }
            else
            {
                string? body = null;
                if (buffer.Length <= MaxStoredBodyBytes)
                {
                    body = Encoding.UTF8.GetString(buffer.ToArray());
                }

                await CompleteAsync(record.Id, context.Response.StatusCode, body, context.Response.ContentType);
            }

            buffer.Position = 0;
            await buffer.CopyToAsync(originalBody);
        }
        catch
        {
            await DeleteAsync(record.Id);
            throw;
        }
        finally
        {
            context.Response.Body = originalBody;
        }
    }

    private static bool ShouldHandle(HttpContext context, out string key)
    {
        key = string.Empty;
        var method = context.Request.Method;
        if (!(HttpMethods.IsPost(method) || HttpMethods.IsPut(method) || HttpMethods.IsPatch(method) || HttpMethods.IsDelete(method)))
        {
            return false;
        }

        if (!context.Request.Path.StartsWithSegments("/api"))
        {
            return false;
        }

        if (!context.Request.Headers.TryGetValue(HeaderName, out var values))
        {
            return false;
        }

        key = values.ToString().Trim();
        return key.Length is > 0 and <= PlatformConsts.MaxIdempotencyKeyLength;
    }

    private async Task HandleExistingAsync(HttpContext context, IdempotencyRecord existing, string hash)
    {
        if (!existing.Matches(hash))
        {
            await WriteErrorAsync(context, StatusCodes.Status422UnprocessableEntity, PlatformErrorCodes.IdempotencyKeyReused);
            return;
        }

        if (!existing.IsCompleted)
        {
            if (_clock.Now - existing.CreationTime > InProgressTimeout)
            {
                // El proceso original murió sin completar: liberar la clave para permitir reintentar.
                await DeleteAsync(existing.Id);
            }

            await WriteErrorAsync(context, StatusCodes.Status409Conflict, PlatformErrorCodes.IdempotentRequestInProgress);
            return;
        }

        context.Response.StatusCode = existing.ResponseStatusCode ?? StatusCodes.Status200OK;
        context.Response.Headers[ReplayedHeaderName] = "true";
        if (!string.IsNullOrEmpty(existing.ResponseContentType))
        {
            context.Response.ContentType = existing.ResponseContentType;
        }

        if (!string.IsNullOrEmpty(existing.ResponseBody))
        {
            await context.Response.WriteAsync(existing.ResponseBody);
        }
    }

    private static async Task<string> ComputeRequestHashAsync(HttpRequest request)
    {
        request.EnableBuffering();
        using var sha = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        sha.AppendData(Encoding.UTF8.GetBytes($"{request.Method}\n{request.Path}{request.QueryString}\n"));
        var buffer = new byte[8192];
        int read;
        while ((read = await request.Body.ReadAsync(buffer)) > 0)
        {
            sha.AppendData(buffer, 0, read);
        }

        request.Body.Position = 0;
        return Convert.ToHexString(sha.GetHashAndReset());
    }

    private async Task<IdempotencyRecord?> FindAsync(string scope, string key)
    {
        using var uow = _unitOfWorkManager.Begin(requiresNew: true);
        var record = await _repository.FirstOrDefaultAsync(x => x.Scope == scope && x.Key == key);
        await uow.CompleteAsync();
        return record;
    }

    private async Task<IdempotencyRecord> CreateAsync(string scope, string key, HttpRequest request, string hash)
    {
        var hours = await _settingProvider.GetAsync(PlatformSettingNames.IdempotencyRetentionHours, 24);
        var path = request.Path.Value ?? "/";
        if (path.Length > PlatformConsts.MaxRequestPathLength)
        {
            path = path[..PlatformConsts.MaxRequestPathLength];
        }

        using var uow = _unitOfWorkManager.Begin(requiresNew: true, isTransactional: true);
        var record = new IdempotencyRecord(_guidGenerator.Create(), scope, key, request.Method, path, hash, _clock.Now, TimeSpan.FromHours(hours));
        await _repository.InsertAsync(record, autoSave: true);
        await uow.CompleteAsync();
        return record;
    }

    private async Task CompleteAsync(Guid id, int status, string? body, string? contentType)
    {
        using var uow = _unitOfWorkManager.Begin(requiresNew: true);
        var record = await _repository.GetAsync(id);
        record.Complete(status, body, contentType, _clock.Now);
        await _repository.UpdateAsync(record);
        await uow.CompleteAsync();
    }

    private async Task DeleteAsync(Guid id)
    {
        try
        {
            using var uow = _unitOfWorkManager.Begin(requiresNew: true);
            await _repository.DeleteAsync(id);
            await uow.CompleteAsync();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "No se pudo liberar la clave de idempotencia {Id}", id);
        }
    }

    private async Task WriteErrorAsync(HttpContext context, int status, string code)
    {
        context.Response.StatusCode = status;
        context.Response.ContentType = "application/json";
        var payload = JsonSerializer.Serialize(new { error = new { code, message = _l[code].Value } });
        await context.Response.WriteAsync(payload);
    }

    private static bool IsUniqueViolation(Exception ex)
    {
        for (var e = ex; e is not null; e = e.InnerException!)
        {
            var message = e.Message;
            if (message.Contains("23505") || message.Contains("duplicate key", StringComparison.OrdinalIgnoreCase) || message.Contains("UNIQUE constraint failed", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }
}
