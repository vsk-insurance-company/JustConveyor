using System;

namespace CurrenciesFetcher.Exceptions
{
    public class FixerIOSettingsNotFound : Exception
    {
        public FixerIOSettingsNotFound() : 
            base("Settings for FixerIO service not found.")
        {
            
        }
    }
}