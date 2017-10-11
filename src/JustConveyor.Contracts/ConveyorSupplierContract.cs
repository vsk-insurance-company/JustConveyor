using System.Threading.Tasks;

namespace JustConveyor.Contracts
{
	/// <summary>
	/// Contract for convey supplier.
	/// </summary>
	public interface ConveySupplierContract
	{
		/// <summary>
        /// Method for supplying packages.
        /// </summary>
        /// <returns></returns>
	    Task<Package> SupplyNextPackage();
	}
}