using Microsoft.Extensions.Logging;

namespace GrainDemo.Hello
{
    public class Greeter : IGreeter
    {
        private readonly ILogger<Greeter> _logger;

        public Greeter(ILogger<Greeter> logger)
        {
            _logger = logger;
        }

        public string DoGreeting(string greeting)
        {
            _logger.LogInformation($"SayHello message received: greeting = '{greeting}'");
            return $"You said: '{greeting}', I say: Hello!";
        }
    }
}
