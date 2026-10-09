using System;
using Medallion.Threading;
using Medallion.Threading.Redis;
using Ieh.OwneyBeauty.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using StackExchange.Redis;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.BackgroundWorkers;
using Volo.Abp.Caching;
using Volo.Abp.Caching.StackExchangeRedis;
using Volo.Abp.DistributedLocking;
using Volo.Abp.Modularity;

namespace Ieh.OwneyBeauty;

/// <summary>
/// Infraestructura compartida por la API y el Worker (PLT-F10, PLT-F11, PLT-F14).
/// Todo es configurable por variables de entorno para Railway:
///   Redis__IsEnabled, Redis__Configuration
///   BackgroundProcessing__IsEnabled (false en la API cuando existe Worker; true en el Worker)
///   OpenTelemetry__Otlp__Endpoint (opcional)
/// </summary>
[DependsOn(
    typeof(OwneyBeautyApplicationModule),
    typeof(OwneyBeautyEntityFrameworkCoreModule),
    typeof(AbpCachingStackExchangeRedisModule)
)]
public class OwneyBeautyInfrastructureModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var configuration = context.Services.GetConfiguration();

        ConfigureCache();
        ConfigureRedisLocks(context, configuration);
        ConfigureBackgroundProcessing(configuration);
        ConfigureOpenTelemetry(context, configuration);
    }

    private void ConfigureCache()
    {
        Configure<AbpDistributedCacheOptions>(options =>
        {
            options.KeyPrefix = "Owney:";
        });
    }

    private static void ConfigureRedisLocks(ServiceConfigurationContext context, IConfiguration configuration)
    {
        if (!IsRedisEnabled(configuration))
        {
            // Sin Redis: lock local (válido con una sola réplica, p. ej. en desarrollo).
            return;
        }

        context.Services.AddSingleton<IDistributedLockProvider>(_ =>
        {
            var connection = ConnectionMultiplexer.Connect(configuration["Redis:Configuration"]!);
            return new RedisDistributedSynchronizationProvider(connection.GetDatabase());
        });

        // Solo con Redis se reemplaza el lock local de ABP por el distribuido (Medallion).
        context.Services.Replace(ServiceDescriptor.Singleton<IAbpDistributedLock, MedallionAbpDistributedLock>());
    }

    private void ConfigureBackgroundProcessing(IConfiguration configuration)
    {
        var enabled = configuration.GetValue("BackgroundProcessing:IsEnabled", true);

        Configure<AbpBackgroundJobOptions>(options => options.IsJobExecutionEnabled = enabled);
        Configure<AbpBackgroundWorkerOptions>(options => options.IsEnabled = enabled);
    }

    private static void ConfigureOpenTelemetry(ServiceConfigurationContext context, IConfiguration configuration)
    {
        var serviceName = configuration["OpenTelemetry:ServiceName"] ?? "owney-beauty";
        var otlpEndpoint = configuration["OpenTelemetry:Otlp:Endpoint"];

        context.Services.AddOpenTelemetry()
            .ConfigureResource(resource => resource.AddService(serviceName))
            .WithTracing(tracing =>
            {
                tracing.AddAspNetCoreInstrumentation(o => o.Filter = http => !http.Request.Path.StartsWithSegments("/health"))
                    .AddHttpClientInstrumentation();
                if (!string.IsNullOrWhiteSpace(otlpEndpoint))
                {
                    tracing.AddOtlpExporter(o => o.Endpoint = new Uri(otlpEndpoint));
                }
            })
            .WithMetrics(metrics =>
            {
                metrics.AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation();
                if (!string.IsNullOrWhiteSpace(otlpEndpoint))
                {
                    metrics.AddOtlpExporter(o => o.Endpoint = new Uri(otlpEndpoint));
                }
            });
    }

    public static bool IsRedisEnabled(IConfiguration configuration) =>
        configuration.GetValue("Redis:IsEnabled", false) && !string.IsNullOrWhiteSpace(configuration["Redis:Configuration"]);
}
