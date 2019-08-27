using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using IGrainDemo;

namespace GrainDemo.Hello
{
    public class HelloGrain : Orleans.Grain, IHello
    {
        private readonly ILogger<HelloGrain> _logger;
        private readonly IGreeter _greeter;

        public HelloGrain(ILogger<HelloGrain> logger, IGreeter greeter)
        {
            _logger = logger;
            _greeter = greeter;
        }

        public Task<string> SayHello(string greeting)
        {
            _logger.LogInformation("HelloGrain receive RPC method invocation request");
            var ret = _greeter.DoGreeting(greeting);
            return Task.FromResult(ret);
        }
    }
}
