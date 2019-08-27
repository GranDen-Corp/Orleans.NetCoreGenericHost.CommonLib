using System;

namespace GrainDemo.Counter
{
    public class CounterGrainState
    {
        public DateTime? LastUpdate { get; set; }
        public int Current { get; set; }
    }
}