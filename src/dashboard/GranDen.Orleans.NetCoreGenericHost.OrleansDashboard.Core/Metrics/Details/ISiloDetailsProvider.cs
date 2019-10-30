using System.Threading.Tasks;
using GranDen.Orleans.NetCoreGenericHost.OrleansDashboard.Model;

namespace GranDen.Orleans.NetCoreGenericHost.OrleansDashboard.Metrics.Details
{
    public interface ISiloDetailsProvider
    {
        Task<SiloDetails[]> GetSiloDetails();
    }
}