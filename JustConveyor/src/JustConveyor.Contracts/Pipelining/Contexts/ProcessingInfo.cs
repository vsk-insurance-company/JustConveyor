using System;

namespace JustConveyor.Contracts.Pipelining.Contexts
{
    /// <summary>
    /// Info about processing.
    /// </summary>
    public class ProcessingInfo
    {
        public string StepName { get; set; }
        public DateTime Started { get; set; }
        public DateTime? Finished { get; set; }
    }
}