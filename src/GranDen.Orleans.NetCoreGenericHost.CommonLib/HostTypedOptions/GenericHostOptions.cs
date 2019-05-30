using System.Collections.Generic;
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
        /// Default Storage Provider's name, currently only "MongoDB"
        /// </summary>
        [Required]
        public string DefaultProvider { get; set; }

        public MongoDbProviderSettings MongoDB { get; set; }
    }

    /// <summary>
    /// MongoDB Provider typed configuration class
    /// </summary>
    public class MongoDbProviderSettings
    {
        public MongoDbProviderClusterSettings Cluster { get; set; }
        public MongoDbProviderStorageSettings Storage { get; set; }
        public MongoDbProviderReminderSettings Reminder { get; set; }
    }

    /// <summary>
    /// Silo Cluster MongoDB Provider typed configuration class
    /// </summary>
    public class MongoDbProviderClusterSettings
    {
        [Required]
        public string DbConn { get; set; }
        [Required]
        public string DbName { get; set; }
        public string CollectionPrefix { get; set; }
    }

    /// <summary>
    /// Grain Storage Provider typed configuration class
    /// </summary>
    public class MongoDbProviderStorageSettings
    {
        [Required]
        public string DbConn { get; set; }
        [Required]
        public string DbName { get; set; }
        public string CollectionPrefix { get; set; }
    }

    /// <summary>
    /// Grain Reminder Provider typed configuration class
    /// </summary>
    public class MongoDbProviderReminderSettings
    {
        [Required]
        public string DbConn { get; set; }
        [Required]
        public string DbName { get; set; }
        public string CollectionPrefix { get; set; }
    }

    /// <summary>
    /// Orleans Silo typed configuration class
    /// </summary>
    public class SiloConfigOption
    {
        [Required]
        public string ClusterId { get; set; }
        [Required]
        public string ServiceId { get; set; }

        public string SiloName { get; set; }

        public string AdvertisedIp { get; set; }
        public bool ListenOnAnyHostAddress { get; set; }
        public int SiloPort { get; set; }
        public int GatewayPort { get; set; }

        public double ResponseTimeoutMinutes { get; set; } = 3.0;

        public bool? IsMultiCluster { get; set; }

        /// <summary>
        /// If IsMultiCluster is true and this has no value, Default MultiCluster will auto add ClusterId
        /// </summary>
        public IList<string> DefaultMultiCluster { get; set; }

        /// <summary>
        /// If IsMultiCluster is true, this must be set.
        /// </summary>
        public Dictionary<string, string> GossipChannels { get; set; }
    }
}
