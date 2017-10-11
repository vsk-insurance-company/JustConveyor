using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JustConveyor.Contracts;
using JustConveyor.Contracts.Dependencies.Attributes;

namespace JustConveyor.Tests.TestProcessModel.Conveyor
{
	/// <summary>
	/// Class that supplies package with ints from initial array.
	/// </summary>
	[Injecting]
	public class IntSupplier : ConveySupplierContract
	{
		/// <summary>
		/// CTOR.
		/// </summary>
		/// <param name="ints">Initial array.</param>
		public IntSupplier(IEnumerable<int> ints)
		{
		    mInts = new Queue<int>(ints);
		}

		private readonly Queue<int> mInts;

        public async Task<Package> SupplyNextPackage()
	    {
	        await Task.Yield();

            if(mInts.Count == 0)
                return Package.Fake;

	        var el = mInts.Dequeue();
            return new Package { Id = $"id:{el}", Load = el };
	    }
	}
}