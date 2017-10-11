namespace JustConveyor
{
    /// <summary>
    /// Class with run setting for the Conveyor
    /// </summary>
    public class ConveyorRunSettings
    {
        /// <summary>
        /// Time for pipeline to wait if no any packages available on listening queue
        /// </summary>
        public int PipelineCoolDownSecondsTimeout { get; set; }

        /// <summary>
        /// Timeout with which instant counters for the Conveyor elements will be harvested.
        /// </summary>
        public int InstantCountersHarversingSecondsTimeout { get; set; }

        /// <summary>
        /// Default settings.
        /// </summary>
        public static ConveyorRunSettings Default { get; } = new ConveyorRunSettings
        {
            PipelineCoolDownSecondsTimeout = 1,
            InstantCountersHarversingSecondsTimeout = 5
        };
    }
}