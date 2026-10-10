using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;

namespace Ieh.OwneyBeauty;

public class Program
{
    public async static Task<int> Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Async(c => c.File("Logs/logs.txt"))
            .WriteTo.Async(c => c.Console())
            .CreateBootstrapLogger();

        try
        {
            Log.Information("Starting Ieh.OwneyBeauty.HttpApi.Host.");
            var builder = WebApplication.CreateBuilder(new WebApplicationOptions
            {
                Args = args,
                EnvironmentName = DebugEnvironment(),
            });

            // appsettings.secrets.json guarda credenciales de entornos compartidos (Railway): nunca se carga
            // en Development, para que una sesión de depuración no toque esas bases.
            if (!builder.Environment.IsDevelopment())
            {
                builder.Configuration.AddJsonFile("appsettings.secrets.json", optional: true, reloadOnChange: true);
            }

            builder.Host
                .UseAutofac()
                .UseSerilog((context, services, loggerConfiguration) =>
                {
                    loggerConfiguration
                        .ReadFrom.Configuration(context.Configuration)
                        .ReadFrom.Services(services)
                        .WriteTo.Async(c => c.AbpStudio(services));
                });
            await builder.AddApplicationAsync<OwneyBeautyHttpApiHostModule>();
            var app = builder.Build();
            await app.InitializeApplicationAsync();
            await app.RunAsync();
            return 0;
        }
        catch (Exception ex)
        {
            if (ex is HostAbortedException)
            {
                throw;
            }

            Log.Fatal(ex, "Host terminated unexpectedly!");
            return 1;
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }

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
}
