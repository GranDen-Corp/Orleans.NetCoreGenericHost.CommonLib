using Microsoft.Extensions.Hosting;
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
            return hostBuilder.ConfigureLogging(builder => { builder.AddSerilog(dispose: true); }).UseSerilog();
        }
    }
}
