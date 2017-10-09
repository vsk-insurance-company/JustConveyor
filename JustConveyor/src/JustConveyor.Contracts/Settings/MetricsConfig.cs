using System.Collections.Generic;

namespace JustConveyor.Contracts.Settings
{
	public class MetricsConfig
	{
		/// <summary>
		/// List of loggers that should be added to 
		/// </summary>
		public List<string> IncludeLastLogsFrom { get; set; }

		/// <summary>
		/// Count of log file lines that should be read
		/// </summary>
		public int CountOfLogLines { get; set; }
	}
}