using System;
using System.Threading;

namespace JustConveyor.Contracts.Utils
{
    /// <summary>
    /// Class with finalizer that count outcomming packages;
    /// </summary>
    public class CountFinalizer: Finalizer
    {
        private readonly int mCount;
        private int mCounter;

        public CountFinalizer(int count, Action then = null): base(then)
        {
            mCount = count;
            FinalizationPredicate = (package, ctx) => Interlocked.Increment(ref mCounter) >= mCount;
        }
    }
}