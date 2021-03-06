﻿using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GranDen.Orleans.NetCoreGenericHost.CommonLib.HostTypedOptions
{
    /// <summary>
    /// Orleans Dashboard typed configuration class
    /// </summary>
    public class OrleansDashboardOption
    {
        /// <summary>
        /// Enable loading Orleans Dashboard
        /// </summary>
        public bool Enable { get; set; } = false;
        /// <summary>
        /// Orleans Dashboard http serve port
        /// </summary>
        public int Port { get; set; } = 8088;
    }

    /// <summary>
    /// Orleans Provider typed configuration class
    /// </summary>
    public class OrleansProviderOption
    {
        /// <summary>
        /// Default Storage Provider's name, currently only "MongoDB", "SQLDB", "MYSQL" and "InMemory"
        /// </summary>
        [Required]
        public string DefaultProvider { get; set; }

        /// <summary>
        /// Top level MongoDB provider setting
        /// </summary>
        public MongoDbProviderSettings MongoDB { get; set; }

        /// <summary>
        /// Top level SQL DB provider setting
        /// </summary>
        public AdoNetProviderSettings SQLDB { get; set; }
    }

    /// <summary>
    /// SQL DB Provider typed configuration class
    /// </summary>
    public class AdoNetProviderSettings
    {
        /// <summary>
        /// Silo Cluster SQL DB provider setting
        /// </summary>
        public AdoNetProviderClusterSettings Cluster { get; set; }

        /// <summary>
        /// Silo Storage SQL DB provider setting
        /// </summary>
        public AdoNetProviderStorageSettings Storage { get; set; }

        /// <summary>
        /// Silo Reminder SQL DB provider setting
        /// </summary>
        public AdoNetProviderReminderSettings Reminder { get; set; }
    }

    /// <summary>
    /// Silo Cluster SQL DB Provider typed configuration class
    /// </summary>
    public class AdoNetProviderClusterSettings : AbstractAdoNetSettings
    {}

    /// <summary>
    /// Grain Storage SQL DB Provider typed configuration class
    /// </summary>
    public class AdoNetProviderStorageSettings : AbstractAdoNetSettings
    {}

    /// <summary>
    /// Grain Reminder SQL DB Provider typed configuration class
    /// </summary>
    public class AdoNetProviderReminderSettings : AbstractAdoNetSettings
    {}

    /// <summary>
    /// Common POCO class for MongoDB connection setting
    /// </summary>
    public abstract class AbstractAdoNetSettings
    {
        /// <summary>
        /// SQL DB connection string
        /// </summary>
        [Required]
        public string DbConn { get; set; }

        /// <summary>
        /// ADO.NET driver assembly
        /// </summary>
        public string Invariant { get; set; }
    }

    /// <summary>
    /// MongoDB Provider typed configuration class
    /// </summary>
    public class MongoDbProviderSettings
    {
        /// <summary>
        /// Silo Cluster MongoDB provider setting
        /// </summary>
        public MongoDbProviderClusterSettings Cluster { get; set; }

        /// <summary>
        /// Silo Storage MongoDB provider setting
        /// </summary>
        public MongoDbProviderStorageSettings Storage { get; set; }

        /// <summary>
        /// Silo Reminder MongoDB provider setting
        /// </summary>
        public MongoDbProviderReminderSettings Reminder { get; set; }
    }

    /// <summary>
    /// Common POCO class for MongoDB connection setting
    /// </summary>
    public abstract class AbstractMongoDBSetting
    {
        /// <summary>
        /// MongoDB connection string
        /// </summary>
        [Required]
        public string DbConn { get; set; }

        /// <summary>
        /// MongoDB database name
        /// </summary>
        [Required]
        public string DbName { get; set; }

        /// <summary>
        /// Collection name prefix
        /// </summary>
        public string CollectionPrefix { get; set; }
    }

    /// <summary>
    /// Silo Cluster MongoDB Provider typed configuration class
    /// </summary>
    public class MongoDbProviderClusterSettings : AbstractMongoDBSetting
    {
    }

    /// <summary>
    /// Grain Storage MongoDB Provider typed configuration class
    /// </summary>
    public class MongoDbProviderStorageSettings : AbstractMongoDBSetting
    {
    }

    /// <summary>
    /// Grain Reminder MongoDB Provider typed configuration class
    /// </summary>
    public class MongoDbProviderReminderSettings : AbstractMongoDBSetting
    {
    }

    /// <summary>
    /// Orleans Silo typed configuration class
    /// </summary>
    public class SiloConfigOption
    {
        /// <summary>
        /// Silo Cluster Id
        /// </summary>
        [Required]
        public string ClusterId { get; set; }

        /// <summary>
        /// Silo Service Id
        /// </summary>
        [Required]
        public string ServiceId { get; set; }

        /// <summary>
        /// Custom Silo Name
        /// </summary>
        public string SiloName { get; set; }

        /// <summary>
        /// Broadcast Ip address
        /// </summary>
        public string AdvertisedIp { get; set; }

        /// <summary>
        /// Set to true to binding all hosting environment's NICs
        /// </summary>
        public bool ListenOnAnyHostAddress { get; set; }

        /// <summary>
        /// Silo-to-Silo port number
        /// </summary>
        public int SiloPort { get; set; }

        /// <summary>
        /// Client connect port number
        /// </summary>
        public int GatewayPort { get; set; }

        /// <summary>
        /// Timeout value, default is 3 minutes
        /// </summary>
        public double ResponseTimeoutMinutes { get; set; } = 3.0;

        /// <summary>
        /// Full Name of excluded grain classes
        /// </summary>
        public IList<string> ExlcudeGrains { get; set; } = new List<string>();

        /// <summary>
        /// Set to true if using in Multi-Cluster configuration
        /// </summary>
        public bool? IsMultiCluster { get; set; }

        /// <summary>
        /// If IsMultiCluster is true and this has no value, Default MultiCluster will auto add ClusterId
        /// </summary>
        public IList<string> DefaultMultiCluster { get; set; }

        /// <summary>
        /// If IsMultiCluster is true, this must be set.
        /// </summary>
        public Dictionary<string, string> GossipChannels { get; set; }

        /// <summary>
        /// Enable Global-Single-Instance (GSI) Grain Protocol
        /// </summary>
        public bool UseGlobalSingleInstanceByDefault { get; set; } = true;

        /// <summary>
        /// Azure Application Insight logging key
        /// </summary>
        public string AzureApplicationInsightKey { get; set; }
    }
}
