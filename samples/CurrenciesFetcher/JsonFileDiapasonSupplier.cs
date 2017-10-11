using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using CurrenciesFetcher.Models;
using JustConveyor.Contracts;
using Newtonsoft.Json;

namespace CurrenciesFetcher
{
    public class JsonFileDiapasonSupplier : ConveySupplierContract
    {
        public DatesDiapason Diapason { get; }

        public JsonFileDiapasonSupplier(string filePath)
        {
            Diapason = JsonConvert.DeserializeObject<DatesDiapason>(File.ReadAllText(filePath));
        }

        public async Task<Package> SupplyNextPackage()
        {
            await Task.Yield();
            return new Package
            {
                Load = Diapason
            };
        }
    }
}