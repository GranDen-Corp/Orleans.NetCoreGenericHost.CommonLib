using System.Threading.Tasks;

namespace IGrainDemo
{
    public interface ICounter : Orleans.IGrainWithIntegerKey
    {
        Task Add(int increment);

        ValueTask<int> CurrentValue();

        Task Reset();
    }
}