using System;
using System.Net;
using System.Threading.Tasks;
using CurrenciesFetcher.Exceptions;
using CurrenciesFetcher.Settings;
using JustConveyor.Contracts.Dependencies.Attributes;
using JustConveyor.Contracts.Pipelining.Attributes;
using Newtonsoft.Json.Linq;
using RestSharp;

namespace CurrenciesFetcher
{
    [Injecting]
    public class FixerIOCurrenciesFetcher
    {
        private readonly RestClient mClient;

        public FixerIOCurrenciesFetcher(ServicesSettingsManager settingsManager)
        {
            var settings = settingsManager["fixerio"];

            if(settings == ServiceSettings.Empty)
                throw new FixerIOSettingsNotFound();

            mClient = new RestClient(settings.ConnectionString);
        }

        [Processor("date-rate")]
        public async Task<double?> GetRateForDate(DateTime date)
        {
            var uri = date.ToString("yyyy-MM-dd");
            var request = new RestRequest(uri, Method.GET);
            var response = await mClient.ExecuteTaskAsync(request);
            if (response.StatusCode == HttpStatusCode.OK)
                return JObject.Parse(response.Content)["rates"]["RUB"].Value<double>();
            return 100;
        }
    }
}