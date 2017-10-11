using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace JustConveyor.Web.Contracts
{
    public class TransferingContextInfo
    {
        public string Id { get; set; }
        public string Step { get; set; }
        public DateTime ProcessingStart { get; set; }
        public IEnumerable<ContextProcessingHistoryInfo> ProcessingHistory { get; set; }
        public string InProcessing { get; set; }
	    public JObject Meta { get; set; }
    }
}