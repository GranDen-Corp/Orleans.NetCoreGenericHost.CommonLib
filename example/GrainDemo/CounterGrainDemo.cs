using Microsoft.Extensions.Logging;
using Orleans;
using RpcShareInterface;
using System;
using System.Threading.Tasks;
using Orleans.CodeGeneration;


// for shared interface code generate, if the shared interface project not being referenced in silo host project.
// see https://kimsereyblog.blogspot.com/2017/12/microsoft-orleans-logs-warnings-and.html
[assembly: KnownAssembly(typeof(ICounter))]
namespace GrainDemo
{
    public class CounterGrainDemo : Grain<CounterGrainState>, ICounter
    {
        private readonly ILogger<CounterGrainDemo> _logger;

        public CounterGrainDemo(ILogger<CounterGrainDemo> logger)
        {
            _logger = logger;
        }

        #region Grain Interface Implement
               
        public Task Add(int increment)
        {
            _logger.LogInformation($"Try to increment {increment}");
            State.LastUpdate = DateTime.Now;
            State.Current += increment;
            return Task.CompletedTask;
        }

        public ValueTask<int> CurrentValue()
        {
            _logger.LogInformation("Current value is {0}, update at {1}",State.Current, 
                State.LastUpdate?.ToShortTimeString());
            return new ValueTask<int>(State.Current);
        }

        public Task Reset()
        {
            _logger.LogInformation("Reset counter");
            State.LastUpdate = null;
            State.Current = 0;
            return Task.CompletedTask;
        }

        #endregion

        public override async Task OnDeactivateAsync()
        {
            await base.WriteStateAsync();
            await base.OnDeactivateAsync();
        }
    }
}
