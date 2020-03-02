using System;

namespace GranDen.Orleans.NetCoreGenericHost.CommonLib.Exceptions
{
    /// <summary>
    /// Exception for situation when In Memory Grain Storage library not found or initiated failed.
    /// </summary>
    public class InMemoryGrainStorageLibLoadFailedException : OrleansLibLoadFailedException
    {
        /// <summary>
        /// Raise when associated In Memory Grain Storage library not exist or initiated failed in applied project.
        /// </summary>
        /// <param name="innerException"></param>
        public InMemoryGrainStorageLibLoadFailedException(Exception innerException) : base(innerException)
        {
        }

        /// <inheritdoc />
        public sealed override string LibNugetName { get; protected set; } = "Microsoft.Orleans.OrleansProviders";
        /// <inheritdoc />
        public sealed override string LibPurpose { get; protected set; } = "In Memory grain storage";
    }
}
