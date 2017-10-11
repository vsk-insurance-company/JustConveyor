using System;

namespace JustConveyor.Contracts
{
    /// <summary>
    /// Class for collecting profiling timestamps.
    /// </summary>
    internal class PackageProcessProfile
    {
        /// <summary>
        /// Timestamp of queuing
        /// </summary>
        public DateTime? Queued { get; set; }

        /// <summary>
        /// Timestamp of dequeuing
        /// </summary>
        public DateTime? Dequeued { get; set; }

        /// <summary>
        /// Timestamp of processing finishing.
        /// </summary>
        public DateTime? ProcessFinished { get; set; }
    }
}