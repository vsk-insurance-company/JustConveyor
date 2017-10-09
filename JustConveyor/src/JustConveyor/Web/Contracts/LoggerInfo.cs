using System.Collections.Generic;

namespace JustConveyor.Web.Contracts
{
	public class LoggerInfo
	{
		public string Name { get; set; }
		public string FilePath { get; set; }
		public List<string> LastLogs { get; set; }
	}
}