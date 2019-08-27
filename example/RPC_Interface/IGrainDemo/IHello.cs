using System.Threading.Tasks;

namespace IGrainDemo
{
    public interface IHello : Orleans.IGrainWithIntegerKey
    {
        Task<string> SayHello(string greeting);
    }
}
