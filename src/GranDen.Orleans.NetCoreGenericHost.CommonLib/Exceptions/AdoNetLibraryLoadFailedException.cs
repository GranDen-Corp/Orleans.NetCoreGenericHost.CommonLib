using System;

namespace GranDen.Orleans.NetCoreGenericHost.CommonLib.Exceptions
{
    /// <summary>
    /// Exception for situation when SQL DB clustering library not found or initiated failed.
    /// </summary>
    public class SqlServerClusterLibLoadFailedException : OrleansLibLoadFailedException
    {
        /// <summary>
        /// Exception for situation when SQL DB clustering library not found or initiated failed.
        /// </summary>
        /// <param name="innerException"></param>
        public SqlServerClusterLibLoadFailedException(Exception innerException) : base(innerException)
        {
        }

        /// <inheritdoc />
        public sealed override string LibNugetName { get; protected set; } = "Microsoft.Orleans.Clustering.AdoNet & System.Data.SqlClient";
        /// <inheritdoc />
        public sealed override string LibPurpose { get; protected set; } = "SQL DB Orleans clustering";
    }

    /// <summary>
    /// Exception for situation when MySQL DB clustering library not found or initiated failed.
    /// </summary>
    public class MySqlClusterLibLoadFailedException : OrleansLibLoadFailedException
    {
        /// <summary>
        /// Exception for situation when MySQL DB clustering library not found or initiated failed.
        /// </summary>
        /// <param name="innerException"></param>
        public MySqlClusterLibLoadFailedException(Exception innerException) : base(innerException)
        {
        }

        /// <inheritdoc />
        public sealed override string LibNugetName { get; protected set; } = "Microsoft.Orleans.Clustering.AdoNet & MySql.Data";
        /// <inheritdoc />
        public sealed override string LibPurpose { get; protected set; } = "MySQL DB Orleans clustering";
    }

    /// <summary>
    /// Exception for situation when SQL DB Grain Storage library not found or initiated failed.
    /// </summary>
    public class SqlServerGrainStorageLibLoadFailedException : OrleansLibLoadFailedException
    {
        /// <summary>
        /// Raise when associated SQL Server DB Grain Storage library not exist or initiated failed in applied project.
        /// </summary>
        /// <param name="innerException"></param>
        public SqlServerGrainStorageLibLoadFailedException(Exception innerException) : base(innerException)
        {
        }
        
        /// <inheritdoc />
        public sealed override string LibNugetName { get; protected set; } = "Microsoft.Orleans.Persistence.AdoNet & System.Data.SqlClient";
        /// <inheritdoc />
        public sealed override string LibPurpose { get; protected set; } = "SQL DB grain storage";
    }

    /// <summary>
    /// Exception for situation when MySQL DB Grain Storage library not found or initiated failed.
    /// </summary>
    public class MySqlGrainStorageLibLoadFailedException : OrleansLibLoadFailedException
    {
        /// <summary>
        /// Raise when associated MySQL DB Grain Storage library not exist or initiated failed in applied project.
        /// </summary>
        /// <param name="innerException"></param>
        public MySqlGrainStorageLibLoadFailedException(Exception innerException) : base(innerException)
        {
        }

        /// <inheritdoc />
        public sealed override string LibNugetName { get; protected set; } = "Microsoft.Orleans.Persistence.AdoNet & MySql.Data";
        /// <inheritdoc />
        public sealed override string LibPurpose { get; protected set; } = "MySQL DB grain storage";
    }

    /// <summary>
    /// Exception for situation when SQL DB Grain Reminder library not found or initiated failed.
    /// </summary>
    public class SqlServerReminderLibLoadFailedException : OrleansLibLoadFailedException
    {
        /// <summary>
        /// Raise when associated ADO.NET DB Grain Reminder library not exist or initiated failed in applied project.
        /// </summary>
        /// <param name="innerException"></param>
        public SqlServerReminderLibLoadFailedException(Exception innerException) : base(innerException)
        {
        }

        /// <inheritdoc />
        public sealed override string LibNugetName { get; protected set; } = "Microsoft.Orleans.Reminders.AdoNet & System.Data.SqlClient";
        /// <inheritdoc />
        public sealed override string LibPurpose { get; protected set; } = "SQL DB grain reminder";
    }

    /// <summary>
    /// Exception for situation when MySQL DB Grain Reminder library not found or initiated failed.
    /// </summary>
    public class MySqlReminderLibLoadFailedException : OrleansLibLoadFailedException
    {
        /// <summary>
        /// Raise when associated MySQL DB Grain Reminder library not exist or initiated failed in applied project.
        /// </summary>
        /// <param name="innerException"></param>
        public MySqlReminderLibLoadFailedException(Exception innerException) : base(innerException)
        {
        }

        /// <inheritdoc />
        public sealed override string LibNugetName { get; protected set; } = "Microsoft.Orleans.Reminders.AdoNet & MySql.Data";
        /// <inheritdoc />
        public sealed override string LibPurpose { get; protected set; } = "MySQL DB grain reminder";
    }
}
