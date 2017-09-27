using System;

namespace CurrenciesFetcher.Models
{
    /// <summary>
    /// Class for representing diapason of dates for analizing currency rates.
    /// </summary>
    public class DatesDiapason
    {
        /// <summary>
        /// From which date.
        /// </summary>
        public DateTime From { get; set; }

        /// <summary>
        /// To which date.
        /// </summary>
        public DateTime To { get; set; }
    }
}