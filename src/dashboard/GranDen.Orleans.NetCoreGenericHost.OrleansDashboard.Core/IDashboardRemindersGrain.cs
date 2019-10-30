using System.Threading.Tasks;
using Orleans;
using Orleans.Concurrency;
using GranDen.Orleans.NetCoreGenericHost.OrleansDashboard.Model;

namespace GranDen.Orleans.NetCoreGenericHost.OrleansDashboard
{
    public interface IDashboardRemindersGrain : IGrainWithIntegerKey
    {
        Task<Immutable<ReminderResponse>> GetReminders(int pageNumber, int pageSize);
    }
}