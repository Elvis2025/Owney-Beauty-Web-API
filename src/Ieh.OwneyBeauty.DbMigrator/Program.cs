using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;

namespace Ieh.OwneyBeauty.DbMigrator;

class Program
{
    static async Task Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("Volo.Abp", LogEventLevel.Warning)
#if DEBUG
                .MinimumLevel.Override("Ieh.OwneyBeauty", LogEventLevel.Debug)
#else
                .MinimumLevel.Override("Ieh.OwneyBeauty", LogEventLevel.Information)
#endif
                .Enrich.FromLogContext()
            .WriteTo.Async(c => c.File("Logs/logs.txt"))
            .WriteTo.Async(c => c.Console())
            .CreateLogger();

        await CreateHostBuilder(args).RunConsoleAsync();
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            // Los appsettings se copian junto al ejecutable: así funciona igual desde VS, `dotnet run` o Docker.
            .UseContentRoot(AppContext.BaseDirectory)
            .UseEnvironment(DebugEnvironment() ?? EnvironmentFromVariables())
            // appsettings.secrets.json guarda credenciales de entornos compartidos (Railway): nunca se carga
            // en Development, para que una sesión de depuración no toque esas bases.
            .ConfigureAppConfiguration((context, config) =>
            {
                if (!context.HostingEnvironment.IsDevelopment())
                {
                    config.AddJsonFile("appsettings.secrets.json", optional: true, reloadOnChange: true);
                }
            })
            .ConfigureLogging((context, logging) => logging.ClearProviders())
            .ConfigureServices((hostContext, services) =>
            {
                services.AddHostedService<DbMigratorHostedService>();
            });

    /// <summary>
    /// Compilado en Debug y sin ambiente explícito (DOTNET_ENVIRONMENT/ASPNETCORE_ENVIRONMENT), se usa
    /// Development: lee appsettings.Development.json y se conecta a la base local.
    /// </summary>
    private static string? DebugEnvironment()
    {
#if DEBUG
        if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT")) &&
            string.IsNullOrEmpty(Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")))
        {
            return Environments.Development;
        }
#endif
        return null;
    }

    private static string EnvironmentFromVariables() =>
        Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT")
        ?? Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
        ?? Environments.Production;
}
