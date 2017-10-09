using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JustConveyor.Contracts;
using JustConveyor.Contracts.Dependencies.Attributes;

namespace IntegersMultiplier
{
    /// <summary>
    /// Class that supplies package with ints from initial array.
    /// </summary>
    [Injecting] // Attribute specifies that class lifetime should be managed by internal IoC container
    public class IntegersSupplier : ConveySupplierContract
    {
        /// <summary>
        /// CTOR.
        /// </summary>
        /// <param name="ints">Initial array.</param>
        public IntegersSupplier(IEnumerable<int> ints /*will be injected by injection provider*/)
        {
            mInts = new Queue<int>(ints);
        }

        private readonly Queue<int> mInts;

        public async Task<Package> SupplyNextPackage()
        {
            var randomWaitTime = TimeSpan.FromSeconds(new Random(DateTime.Now.Millisecond).Next(1, 5));
            await Task.Delay(randomWaitTime);

            if(mInts.Count == 0)
                return Package.Fake;

            var el = mInts.Dequeue();
            return new Package { Id = $"id:{el}", Load = el };
        }
    }
}