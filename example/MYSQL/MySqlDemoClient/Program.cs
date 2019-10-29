using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using Serilog.Exceptions;
using Serilog.Sinks.SystemConsole.Themes;
using System;
using System.Threading.Tasks;

namespace MySqlDemoClient
{
    class Program
    {
        public static async Task Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Override("Orleans.RuntimeClientLogStatistics", LogEventLevel.Warning)
                .Enrich.FromLogContext()
                .Enrich.WithProcessId()
                .Enrich.WithProcessName()
                .Enrich.WithThreadId()
                .Enrich.WithExceptionDetails()
                .WriteTo.Console(theme: AnsiConsoleTheme.Code)
                .WriteTo.Debug()
                .CreateLogger();

            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);

            var serviceProvider = serviceCollection.BuildServiceProvider();

            var demo = serviceProvider.GetService<AccessCounterDemo>();
            var demo2 = serviceProvider.GetService<CallGrainWith3rdPartyLibDemo>();

            var logger = GetLogger<Program>(serviceProvider);

            logger.LogInformation("Press space key to start demo");
            WaitForKeyAsync(ConsoleKey.Spacebar);

            try
            {
                await demo.RunCounter();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "error occured!");
                throw;
            }

            logger.LogInformation("\r\n===\r\nPress space key to Run 2nd Demo\r\n===");
            WaitForKeyAsync(ConsoleKey.Spacebar);

            try
            {
                await demo2.TestRpc();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "error occured!");
                throw;
            }

            logger.LogInformation("Press enter to exit");
            Console.ReadLine();
        }

        private static void WaitForKeyAsync(ConsoleKey key)
        {
            do
            {
                while (!Console.KeyAvailable)
                {
                    //wait
                    Task.Delay(new TimeSpan(0, 0, 1)).Wait();
                }
            } while (Console.ReadKey(true).Key != key);
        }

        private static void ConfigureServices(ServiceCollection services)
        {
            services.AddLogging(configure => configure.AddSerilog());

            services.AddTransient<AccessCounterDemo>();
            services.AddTransient<CallGrainWith3rdPartyLibDemo>();
        }

        private static ILogger<T> GetLogger<T>(IServiceProvider serviceProvider)
        {
            return serviceProvider.GetService<ILogger<T>>();
        }
    }
}
