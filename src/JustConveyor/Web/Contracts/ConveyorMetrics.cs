using System;
using System.Collections.Generic;

namespace JustConveyor.Web.Contracts
{
	public class ConveyorMetrics
    {
        public DateTime Started { get; set; }
        public string InFlightTime { get; set; }
        public IEnumerable<PipelineInfo> Pipelines { get; set; }
        public IEnumerable<QueueInfo> Queues { get; set; }
        public IEnumerable<BlueprintInfo> Blueprints { get; set; }
        public IEnumerable<SupplierInfo> Suppliers { get; set; }
        public IEnumerable<TransferingContextInfo> Contextes { get; set; }
		public List<LoggerInfo> Loggers { get; set; } = new List<LoggerInfo>();
    }
}