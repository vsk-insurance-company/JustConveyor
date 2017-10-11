using System;
using JustConveyor.Contracts;

namespace JustConveyor
{
    internal class SupplierInstance
    {
        public string Name { get; set; }
        public ConveySupplierContract Supplier { get; set; }
        public int SuppliedPackagesCount { get; set; }
        public int ErrorsCount { get; set; }
        public DateTime? Started { get; set; }

        public double PackagesPerSec => Started == null
            ? 0
            : Math.Round(SuppliedPackagesCount/
                         (DateTime.Now - Started.Value).TotalSeconds, 3);

        public string State { get; set; }
    }
}