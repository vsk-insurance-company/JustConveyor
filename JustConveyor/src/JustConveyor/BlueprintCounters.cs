using System;
using System.Threading;

namespace JustConveyor
{
    internal class BlueprintCounters
    {
        public DateTime? Started { get; set; }

        public double ProcessingRatePerSec =>
            Started == null
                ? 0
                : Math.Round(mOut/(DateTime.Now - Started.Value).TotalSeconds, 3);

        public long In => mIn;
        public long Out => mOut;
        public long OnQueuePrev { get; set; }
        public long OnQueueDelta { get; set; }
        public long OnQueue { get; set; }

        private long mIn;
        private long mOut;

        public void IncrementIn()
        {
            Interlocked.Increment(ref mIn);
        }

        public void IncrementOut()
        {
            Interlocked.Increment(ref mOut);
        }
    }
}