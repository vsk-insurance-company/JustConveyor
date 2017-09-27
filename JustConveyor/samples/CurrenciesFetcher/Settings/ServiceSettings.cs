namespace CurrenciesFetcher.Settings
{
    public class ServiceSettings
    {
        /// <summary>
        /// Connection string for service access.
        /// </summary>
        public string ConnectionString { get; set; }

        /// <summary>
        /// For case when no any settings for requesting service.
        /// </summary>
        public static ServiceSettings Empty = new ServiceSettings();
    }
}