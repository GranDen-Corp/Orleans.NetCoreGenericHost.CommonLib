using System;

namespace GranDen.Orleans.NetCoreGenericHost.CommonLib.Exceptions
{
    /// <summary>
    /// Exception for situation when Mongo DB clustering library not found or initiated failed.
    /// </summary>
    public class MongoDbLibLoadFailedException : OrleansLibLoadFailedException
    {
        /// <summary>
        /// Raise when associated Mongo DB hosting configuration library not exist or initiated failed in applied project.
        /// </summary>
        /// <param name="innerException"></param>
        public MongoDbLibLoadFailedException(Exception innerException) :
            base(innerException)
        {
        }

        /// <inheritdoc />
        public sealed override string LibNugetName { get; protected set; } = "Orleans.Providers.MongoDB";
        /// <inheritdoc />
        public sealed override string LibPurpose { get; protected set; }
    }

    /// <summary>
    /// Exception for situation when Mongo DB clustering library not found or initiated failed.
    /// </summary>
    public class MongoDbClusterLibLoadFailedException : MongoDbLibLoadFailedException
    {
        /// <summary>
        /// Raise when associated Mongo DB Clustering library not exist or initiated failed in applied project.
        /// </summary>
        /// <param name="innerException"></param>
        public MongoDbClusterLibLoadFailedException(Exception innerException) : base(innerException)
        {
            LibPurpose = "mongodb clustering";
        }
    }

    /// <summary>
    /// Exception for situation when Mongo DB Grain Storage library not found or initiated failed.
    /// </summary>
    public class MongoDbGrainStorageLibLoadFailedException : MongoDbLibLoadFailedException
    {
        /// <summary>
        /// Raise when associated Mongo DB Grain Storage library not exist or initiated failed in applied project.
        /// </summary>
        /// <param name="innerException"></param>
        public MongoDbGrainStorageLibLoadFailedException(Exception innerException) : base(innerException)
        {
            LibPurpose = "grain storage";
        }
    }

    /// <summary>
    /// Exception for situation when Mongo DB Grain Reminder library not found or initiated failed.
    /// </summary>
    public class MongoDbReminderLibLoadFailedException : MongoDbLibLoadFailedException
    {
        /// <summary>
        /// Raise when associated Mongo DB Grain Reminder library not exist or initiated failed in applied project.
        /// </summary>
        /// <param name="innerException"></param>
        public MongoDbReminderLibLoadFailedException(Exception innerException) : base(innerException)
        {
            LibPurpose = "grain reminder";
        }
    }
}
