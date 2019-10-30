using System.Linq;
using System.Threading.Tasks;
using Orleans.Runtime;
using GranDen.Orleans.NetCoreGenericHost.OrleansDashboard.Metrics.Details;
using GranDen.Orleans.NetCoreGenericHost.OrleansDashboard.Model;

namespace GranDen.Orleans.NetCoreGenericHost.OrleansDashboard.Implementation.Details
{
    /// <summary>
    /// Simple silo details provider
    /// Uses ISiloStatusOracle internally
    /// </summary>
    public sealed class SiloStatusOracleSiloDetailsProvider : ISiloDetailsProvider
    {
        private readonly ISiloStatusOracle siloStatusOracle;

        public SiloStatusOracleSiloDetailsProvider(ISiloStatusOracle siloStatusOracle)
        {
            this.siloStatusOracle = siloStatusOracle;
        }

        public Task<SiloDetails[]> GetSiloDetails()
        {
            // todo this could be improved by using a ISiloStatusListener
            // and caching / projecting the changes instead of polling
            // should reduce allocations of array's etc

            return Task.FromResult(siloStatusOracle.GetApproximateSiloStatuses(true)
                .Select(x => new SiloDetails
                {
                    Status = x.Value.ToString(),
                    SiloStatus = x.Value,
                    SiloAddress = x.Key.ToParsableString(),
                    SiloName = x.Key.ToParsableString() //use the address for naming
                })
                .ToArray());
        }
    }
}