using System;

namespace GrainDemo
{
    public class CounterGrainState
    {
        public DateTime? LastUpdate { get; set; }
        public int Current { get; set; }
    }
}