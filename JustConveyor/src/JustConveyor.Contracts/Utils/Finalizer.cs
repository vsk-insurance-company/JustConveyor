using System;
using System.Threading.Tasks;
using JustConveyor.Contracts.Dependencies;
using JustConveyor.Contracts.Pipelining.Contexts;
using Nito.AsyncEx;
using NLog;

namespace JustConveyor.Contracts.Utils
{
    /// <summary>
    /// Class with awating functionality for packages finalization.
    /// </summary>
    public class Finalizer
    {
        public Finalizer(Func<Package, TransferingContext, bool> finalizationPredicate, Action then = null) : this(then)
        {
            FinalizationPredicate = finalizationPredicate;
        }

        internal Finalizer(Action then = null)
        {
            mThen = then;
        }

        private readonly AsyncManualResetEvent mSyncEvent = new AsyncManualResetEvent();
        protected Func<Package, TransferingContext, bool> FinalizationPredicate;
        private readonly Action mThen;

        public Task GetWaitTask()
        {
            return mSyncEvent.WaitAsync();
        }

        public void Finalization(Package package, TransferingContext context)
        {
            if (FinalizationPredicate(package, context))
            {
                mSyncEvent.Set();
                try
                {
                    mThen?.Invoke();
                }
                catch (Exception e)
                {
                    Injection.InjectionProvider?.Get<ILogger>()?.Error($"Error during finalization: {e}");
                }
            }
        }
    }
}