using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace GranDen.Orleans.NetCoreGenericHost.CommonLib.Helpers
{
    public static class ConfigurationUtil
    {
        public static T GetTypedConfig<T>(this IConfigurationSection config, string sectionKey) where T : new()
        {
            var option = new T();
            config.GetSection(sectionKey).Bind(option);
            return option;
        }
    }
}
