﻿using System.Reflection;
using Orleans;
using Orleans.ApplicationParts;

namespace GranDen.Orleans.Server.SharedInterface
{
    /// <summary>
    /// Extension methods for easier add external assembly
    /// </summary>
    public static class ApplicationPartExtension
    {
        /// <summary>
        /// Utility method for add external assembly to SiloBuilder
        /// </summary>
        /// <param name="applicationPartManager"></param>
        /// <param name="assembly"></param>
        public static void AddDynamicPart(this IApplicationPartManager applicationPartManager, Assembly assembly)
        {
            applicationPartManager.AddApplicationPart(assembly).WithReferences();
        }
    }
}