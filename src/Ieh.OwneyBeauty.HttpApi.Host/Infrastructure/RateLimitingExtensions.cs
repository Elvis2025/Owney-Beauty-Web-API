using System;
using System.Globalization;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Ieh.OwneyBeauty.Infrastructure;

/// <summary>
/// PLT-F12 / OB-47-006. Límites por política, particionados por usuario autenticado o IP:
///  - auth:   login, registro y recuperación de contraseña (por IP).
///  - otp:    envío y validación de códigos de verificación.
///  - global: resto de la API.
/// Valores configurables en la sección "RateLimiting" (o variables RateLimiting__Auth__PermitLimit, etc.).
/// </summary>
public static class RateLimitingExtensions
{
    public static IServiceCollection AddOwneyRateLimiting(this IServiceCollection services, IConfiguration configuration)
    {
        var auth = Read(configuration, "Auth", 10, 60);
        var otp = Read(configuration, "Otp", 10, 60);
        var global = Read(configuration, "Global", 300, 60);

        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.OnRejected = async (context, token) =>
            {
                if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
                {
                    context.HttpContext.Response.Headers.RetryAfter = ((int)retryAfter.TotalSeconds).ToString(CultureInfo.InvariantCulture);
                }

                context.HttpContext.Response.ContentType = "application/json";
                await context.HttpContext.Response.WriteAsync(
                    "{\"error\":{\"code\":\"Owney.Platform:TooManyRequests\",\"message\":\"Demasiadas solicitudes. Intenta de nuevo en unos momentos.\"}}",
                    token);
            };

            options.GlobalLimiter = PartitionedRateLimiter.CreateChained(
                PartitionedRateLimiter.Create<HttpContext, string>(http =>
                {
                    var path = http.Request.Path;
                    if (IsAuthPath(path))
                    {
                        return RateLimitPartition.GetFixedWindowLimiter("auth:" + ClientIp(http), _ => Window(auth));
                    }

                    if (path.StartsWithSegments("/api/accounts/verification"))
                    {
                        return RateLimitPartition.GetFixedWindowLimiter("otp:" + Identity(http), _ => Window(otp));
                    }

                    return RateLimitPartition.GetNoLimiter("none");
                }),
                PartitionedRateLimiter.Create<HttpContext, string>(http =>
                    http.Request.Path.StartsWithSegments("/health")
                        ? RateLimitPartition.GetNoLimiter("health")
                        : RateLimitPartition.GetFixedWindowLimiter("global:" + Identity(http), _ => Window(global))));
        });

        return services;
    }

    private static bool IsAuthPath(PathString path) =>
        path.StartsWithSegments("/connect/token") ||
        path.StartsWithSegments("/api/accounts/registration") ||
        path.StartsWithSegments("/api/account/send-password-reset-code") ||
        path.StartsWithSegments("/api/account/reset-password") ||
        path.StartsWithSegments("/Account/Login") ||
        path.StartsWithSegments("/Account/ForgotPassword");

    private static string Identity(HttpContext http) =>
        http.User.FindFirst("sub")?.Value ?? "ip:" + ClientIp(http);

    private static string ClientIp(HttpContext http) =>
        http.Connection.RemoteIpAddress?.ToString() ?? "unknown";

    private static FixedWindowRateLimiterOptions Window((int Limit, int Seconds) value) => new()
    {
        PermitLimit = value.Limit,
        Window = TimeSpan.FromSeconds(value.Seconds),
        QueueLimit = 0,
        AutoReplenishment = true,
    };

    private static (int Limit, int Seconds) Read(IConfiguration configuration, string name, int defaultLimit, int defaultSeconds) =>
        (configuration.GetValue($"RateLimiting:{name}:PermitLimit", defaultLimit),
         configuration.GetValue($"RateLimiting:{name}:WindowSeconds", defaultSeconds));
}
