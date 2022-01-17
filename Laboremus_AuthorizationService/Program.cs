using System;
using System.IO;
using Laboremus_AuthorizationService.Data;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using Serilog.Exceptions;
using Serilog.Exceptions.Core;

namespace Laboremus_AuthorizationService
{
    public class Program
    {
        private static string _environmentName;
        public static int Main(string[] args)
        {
            Console.Title = "AuthServer";
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{environment}.json", optional: true)
                .AddEnvironmentVariables()
                .AddCommandLine(args)
                .Build();

            const string serviceName = "Auth Service";

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.AspNetCore.Hosting.Internal.WebHost", LogEventLevel.Information)
                .Enrich.WithProperty("ServiceName", serviceName)
                .Enrich.FromLogContext()
                .Enrich.WithEnvironmentUserName()
                .Enrich.WithMachineName()
                .Enrich.WithExceptionDetails(new DestructuringOptionsBuilder()
                    .WithDefaultDestructurers()
                    .WithRootName("Exception"))
                .WriteTo.Console()
                .WriteTo.File(@"Logs\application.log", rollingInterval: RollingInterval.Hour)
                .CreateLogger();

            try
            {
                Log.Information($"Starting web service: {serviceName} environment: {environment}");
                var host = CreateWebHostBuilder(args).Build();
                
                using (var scope = host.Services.CreateScope())
                {
                    SeedData.Init(scope, configuration);
                }

                host.Run();
                return 0;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, $"Host terminated unexpectedly {serviceName}");
                return 1;
            }
            finally
            {
                Log.CloseAndFlush();
            }

        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .ConfigureLogging((hostingContext, config) =>
                {
                    config.ClearProviders();
                    _environmentName = hostingContext.HostingEnvironment.EnvironmentName;
                })
                .UseSerilog()
                .UseKestrel()
                .UseStartup<Startup>();
    }
}
