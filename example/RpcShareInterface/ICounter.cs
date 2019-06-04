using System.Threading.Tasks;
using Orleans.CodeGeneration;

namespace RpcShareInterface
{
    public interface ICounter : Orleans.IGrainWithIntegerKey
    {
        Task Add(int increment);

        ValueTask<int> CurrentValue();

        Task Reset();
    }
}