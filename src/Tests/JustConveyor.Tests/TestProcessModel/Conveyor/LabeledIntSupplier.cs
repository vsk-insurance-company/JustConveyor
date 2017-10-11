using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JustConveyor.Contracts;

namespace JustConveyor.Tests.TestProcessModel.Conveyor
{
	public class LabeledIntSupplier : ConveySupplierContract
	{
		public LabeledIntSupplier(IEnumerable<int> ints, string label)
		{
			mLabel = label;
            mInts = new Queue<int>(ints);
		}

		private readonly Queue<int> mInts;
		private readonly string mLabel;
        
	    public async Task<Package> SupplyNextPackage()
	    {
            await Task.Yield();

            if (mInts.Count == 0)
                return Package.Fake;

            var el = mInts.Dequeue();

	        return new Package
	        {
	            Label = mLabel,
	            Id = $"id:{el}",
	            Load = el
	        };
	    }
    }
}