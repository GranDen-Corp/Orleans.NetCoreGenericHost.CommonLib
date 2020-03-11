using System;

namespace GranDen.Orleans.NetCoreGenericHost.CommonLib.Exceptions
{

    /// <summary>
    /// Exception for situation when Azure Application Insight telemetry library not found or initiated failed.
    /// </summary>
    public class AzureApplicationInsightLibLoadFailedException : OrleansLibLoadFailedException
    {
        /// <summary>
        /// Raise when associated Azure Application Insight telemetry library not exist or initiated failed in applied project.
        /// </summary>
        /// <param name="innerException"></param>
        public AzureApplicationInsightLibLoadFailedException(Exception innerException) : base(innerException)
        {
        }

        /// <inheritdoc />
        public sealed override string LibNugetName { get; protected set; } = "Microsoft.Orleans.OrleansTelemetryConsumers.AI";
        /// <inheritdoc />
        public sealed override string LibPurpose { get; protected set; } = "Azure Application Insight telemetry";
    }
}
