using System;
using GranDen.Orleans.NetCoreGenericHost.CommonLib;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using Serilog.Exceptions;
using Serilog.Sinks.SystemConsole.Themes;

namespace HostingDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            var logConfig = new LoggerConfiguration()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("Orleans.Runtime.Management.ManagementGrain", LogEventLevel.Warning)
                .MinimumLevel.Override("Orleans.Runtime.SiloControl", LogEventLevel.Warning)
                .Enrich.FromLogContext()
                .Enrich.WithProcessId()
                .Enrich.WithProcessName()
                .Enrich.WithThreadId()
                .Enrich.WithExceptionDetails()
                .WriteTo.Console(theme: AnsiConsoleTheme.Code)
                .WriteTo.Trace()
                .WriteTo.Debug();

            Log.Logger = logConfig.CreateLogger();

            var genericHostBuilder = OrleansSiloBuilderExtension.CreateHostBuilder(args);

            try
            {
                var genericHost = genericHostBuilder.Build();
                genericHost.Run();
            }
            catch (OperationCanceledException)
            {
                //do nothing
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Orleans Silo Host error");
                throw;
            }
        }
    }
}
