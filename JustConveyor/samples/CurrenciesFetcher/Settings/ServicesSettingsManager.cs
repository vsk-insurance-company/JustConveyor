using System.Collections.Generic;

namespace CurrenciesFetcher.Settings
{
    public sealed class ServicesSettingsManager
    {
        private readonly Dictionary<string, ServiceSettings> mSettingsRegistry =
            new Dictionary<string, ServiceSettings>();

        public ServicesSettingsManager(Dictionary<string, ServiceSettings> initialRegistry)
        {
            foreach (var serviceSettings in initialRegistry)
                mSettingsRegistry.Add(serviceSettings.Key, serviceSettings.Value);
        }

        public ServiceSettings this[string serviceName] => mSettingsRegistry.ContainsKey(serviceName)
            ? mSettingsRegistry[serviceName]
            : ServiceSettings.Empty;
    }
}