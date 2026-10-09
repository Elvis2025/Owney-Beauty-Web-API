using System;
using Volo.Abp;
using Volo.Abp.Auditing;
using Volo.Abp.Domain.Entities;

namespace Ieh.OwneyBeauty.Platform.Idempotency;

/// <summary>
/// Respuesta registrada para una solicitud con encabezado <c>Idempotency-Key</c> (PLT-F08).
/// Repetir la misma solicitud devuelve la misma respuesta sin volver a ejecutar la operación.
/// Unicidad: (Scope, Key). Scope = id del usuario o "anonymous:{ip}".
/// </summary>
public class IdempotencyRecord : Entity<Guid>, IHasCreationTime
{
    public string Scope { get; private set; } = null!;

    public string Key { get; private set; } = null!;

    public string Method { get; private set; } = null!;

    public string Path { get; private set; } = null!;

    /// <summary>SHA-256 (hex) del método, ruta y cuerpo de la solicitud original.</summary>
    public string RequestHash { get; private set; } = null!;

    public int? ResponseStatusCode { get; private set; }

    public string? ResponseBody { get; private set; }

    public string? ResponseContentType { get; private set; }

    public DateTime CreationTime { get; private set; }

    public DateTime? CompletionTime { get; private set; }

    public DateTime ExpirationTime { get; private set; }

    public bool IsCompleted => CompletionTime.HasValue;

    protected IdempotencyRecord()
    {
    }

    public IdempotencyRecord(Guid id, string scope, string key, string method, string path, string requestHash, DateTime now, TimeSpan retention)
        : base(id)
    {
        Scope = Check.NotNullOrWhiteSpace(scope, nameof(scope), PlatformConsts.MaxIdempotencyScopeLength);
        Key = Check.NotNullOrWhiteSpace(key, nameof(key), PlatformConsts.MaxIdempotencyKeyLength);
        Method = Check.NotNullOrWhiteSpace(method, nameof(method), 16);
        Path = Check.NotNullOrWhiteSpace(path, nameof(path), PlatformConsts.MaxRequestPathLength);
        RequestHash = Check.NotNullOrWhiteSpace(requestHash, nameof(requestHash), PlatformConsts.RequestHashLength);
        CreationTime = now;
        ExpirationTime = now.Add(retention);
    }

    public bool Matches(string requestHash) => string.Equals(RequestHash, requestHash, StringComparison.Ordinal);

    public void Complete(int statusCode, string? body, string? contentType, DateTime now)
    {
        ResponseStatusCode = statusCode;
        ResponseBody = body;
        ResponseContentType = contentType?.Length > PlatformConsts.MaxResponseContentTypeLength
            ? contentType[..PlatformConsts.MaxResponseContentTypeLength]
            : contentType;
        CompletionTime = now;
    }
}
