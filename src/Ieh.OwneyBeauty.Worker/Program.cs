using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using Volo.Abp;
using Volo.Abp.Autofac;
using Volo.Abp.Modularity;

namespace Ieh.OwneyBeauty.Worker;

/// <summary>
/// Proceso de trabajo en segundo plano (PLT-F11): publica el outbox, procesa el inbox,
/// ejecuta background jobs (correos, recordatorios) y workers periódicos. No expone HTTP.
/// </summary>
public static class Program
{
    public static async Task<int> Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .WriteTo.Async(c => c.Console())
            .CreateLogger();

        try
        {
            Log.Information("Iniciando Ieh.OwneyBeauty.Worker");
            await Host.CreateDefaultBuilder(args)
                .AddAppSettingsSecretsJson()
                .UseSerilog()
                .ConfigureServices(services => services.AddHostedService<WorkerHostedService>())
                .RunConsoleAsync();
            return 0;
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "El Worker terminó de forma inesperada");
            return 1;
        }
        finally
        {
            await Log.CloseAndFlushAsync();
        }
    }
}

public class WorkerHostedService : IHostedService
{
    private readonly IConfiguration _configuration;
    private readonly IHostEnvironment _environment;
    private IAbpApplicationWithInternalServiceProvider? _application;

    public WorkerHostedService(IConfiguration configuration, IHostEnvironment environment)
    {
        _configuration = configuration;
        _environment = environment;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _application = await AbpApplicationFactory.CreateAsync<OwneyBeautyWorkerModule>(options =>
        {
            options.Services.ReplaceConfiguration(_configuration);
            options.Services.AddSingleton(_environment);
            options.UseAutofac();
            options.Services.AddLogging(logging => logging.ClearProviders().AddSerilog());
        });

        await _application.InitializeAsync();
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_application is not null)
        {
            await _application.ShutdownAsync();
            _application.Dispose();
        }
    }
}

[DependsOn(
    typeof(AbpAutofacModule),
    typeof(OwneyBeautyInfrastructureModule)
)]
public class OwneyBeautyWorkerModule : AbpModule
{
}
