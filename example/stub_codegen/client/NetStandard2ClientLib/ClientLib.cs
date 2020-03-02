using System;
using System.Collections.Generic;
using GranDen.Orleans.Client.CommonLib;
using HelloNetStandard.ShareInterface;
using Orleans;
using Orleans.Hosting;

namespace NetStandard2ClientLib
{
    public static class ClientLib
    {
        public static IClientBuilder CreateOrleansClientBuilder(
            int gatewayPort = 30000,
            string clusterId = "dev",
            string serviceId = "dev",
            IEnumerable<Type> applicationPartTypes = null
            )
        {
            var builder = OrleansClientBuilder
                .CreateLocalhostClientBuilder(gatewayPort, clusterId, serviceId, applicationPartTypes)
                .ConfigureApplicationParts(_ => _.AddApplicationPart(typeof(IHello).Assembly).WithCodeGeneration().WithReferences());

            return builder;
        }
    }
}