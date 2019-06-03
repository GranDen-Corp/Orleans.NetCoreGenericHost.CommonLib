using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Sinks.SystemConsole.Themes;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace DemoClient {
    class Program {
        static async Task Main (string[] args) {
            Log.Logger = new LoggerConfiguration ().WriteTo.Console (theme: AnsiConsoleTheme.Code).CreateLogger ();

            var serviceCollection = new ServiceCollection ();
            ConfigureServices (serviceCollection);

            var serviceProvider = serviceCollection.BuildServiceProvider ();

            var logger = GetLogger<Program> (serviceProvider);

            logger.LogInformation ("Press enter to run demo");
            Console.ReadLine ();

            try {
                var demo = serviceProvider.GetService<AccessCounterDemo> ();
                await demo.RunCounter ();
            } catch (Exception ex) {
                logger.LogError (ex, "error occured!");
            }

            logger.LogInformation ("Press enter to exit");
            Console.ReadLine ();
        }

        private static void ConfigureServices (ServiceCollection services) {
            services.AddLogging (configure => configure.AddSerilog ());

            services.AddTransient<AccessCounterDemo> ();
        }

        private static ILogger<T> GetLogger<T> (ServiceProvider serviceProvider) {
            return serviceProvider.GetService<ILogger<T>> ();
        }
    }
}