using System;
using System.Collections.Generic;
using System.Text;

namespace GranDen.Orleans.NetCoreGenericHost.CommonLib.Exceptions
{
    /// <summary>
    /// Throws if configuration of silo host is ill-logicial
    /// </summary>
    public class OrleansSiloHostConfigException : Exception
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="message"></param>
        public OrleansSiloHostConfigException(string message) : base(message)
        { }
    }
}
