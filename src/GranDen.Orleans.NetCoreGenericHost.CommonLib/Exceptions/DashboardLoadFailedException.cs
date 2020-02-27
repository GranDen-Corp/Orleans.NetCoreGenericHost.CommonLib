using System;

namespace GranDen.Orleans.NetCoreGenericHost.CommonLib.Exceptions
{
    /// <summary>
    /// Exception for situation when dashboard library not found or initiated failed.
    /// </summary>
    public class DashboardLoadFailedException : Exception
    {
#if NETCOREAPP2_1
        const string DashboardNugetName = "GranDen.Orleans.NetCoreGenericHost.OrleansDashboard";
#else
        private const string DashboardNugetName = "OrleansDashboard";
#endif
        /// <summary>
        /// Raise when associated Orleans Dashboard library not exist or initiated failed in applied project.
        /// </summary>
        public DashboardLoadFailedException(Exception innerException) : base($"Please install {DashboardNugetName} nuget and configure properly.", innerException)
        {
        }
    }
}
