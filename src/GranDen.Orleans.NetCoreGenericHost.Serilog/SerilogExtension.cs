using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;

// ReSharper disable once CheckNamespace
namespace GranDen.Orleans.NetCoreGenericHost.CommonLib
{
    /// <summary>
    /// Extension method for add Serilog to Generic Host
    /// </summary>
    public static class SerilogExtension
    {
        /// <summary>
        /// Make HostBuilder use Serilog
        /// </summary>
        /// <param name="hostBuilder"></param>
        /// <returns></returns>
        // ReSharper disable once UnusedMember.Global
        public static IHostBuilder ApplySerilog(this IHostBuilder hostBuilder)
        {
            return hostBuilder.ConfigureLogging(logBuilder => 
            {
                //because this management grain is very noisy when using Orleans Dashboard
                logBuilder.AddFilter("Orleans.Runtime.Management.ManagementGrain", LogLevel.Warning)
                          .AddFilter("Orleans.Runtime.SiloControl", LogLevel.Warning);

                logBuilder.AddSerilog(dispose: true);
            }).UseSerilog();
        }
    }
}
