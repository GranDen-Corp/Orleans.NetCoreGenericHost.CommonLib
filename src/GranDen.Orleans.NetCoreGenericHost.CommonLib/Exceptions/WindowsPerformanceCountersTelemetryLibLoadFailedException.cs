using System;

namespace GranDen.Orleans.NetCoreGenericHost.CommonLib.Exceptions
{
    /// <summary>
    /// Exception for situation when Windows Performance Counters telemetry library not found or initiated failed.
    /// </summary>
    public class WindowsPerformanceCountersTelemetryLibLoadFailedException : OrleansLibLoadFailedException 
    {
        /// <summary>
        /// Raise when associated Windows Performance Counters telemetry library not exist or initiated failed in applied project.
        /// </summary>
        public WindowsPerformanceCountersTelemetryLibLoadFailedException(Exception innerException) : base(innerException)
        {
        }

        /// <inheritdoc/>
        public override string LibNugetName { get; protected set; } = "Microsoft.Orleans.OrleansTelemetryConsumers.Counters";
        /// <inheritdoc/>
        public override string LibPurpose { get; protected set; } = "Windows Performance Counters telemetry";
    }
}
