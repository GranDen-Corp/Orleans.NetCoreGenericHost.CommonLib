using System;

namespace GranDen.Orleans.NetCoreGenericHost.CommonLib.Exceptions
{
    /// <summary>
    /// Exception for situation when Linux telemetry library not found or initiated failed.
    /// </summary>
    public class LinuxTelemetryLibLoadFailedException : OrleansLibLoadFailedException
    {
        /// <summary>
        /// Raise when associated Linux telemetry library not exist or initiated failed in applied project.
        /// </summary>
        public LinuxTelemetryLibLoadFailedException(Exception innerException) : base(innerException)
        {
        }

        /// <inheritdoc/>
        public override string LibNugetName { get; protected set; } = "Microsoft.Orleans.OrleansTelemetryConsumers.Linux";
        /// <inheritdoc/>
        public override string LibPurpose { get; protected set; } = "Linux telemetry";
    }
}
