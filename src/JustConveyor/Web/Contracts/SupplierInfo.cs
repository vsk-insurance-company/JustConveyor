namespace JustConveyor.Web.Contracts
{
    public class SupplierInfo
    {
        public string Name { get; set; }
        public string State { get; set; }
        public string SupplierType { get; set; }
        public int Supplied { get; set; }
        public long Errors { get; set; }
		public double PackagesRatePerSec { get; set; }
    }
}