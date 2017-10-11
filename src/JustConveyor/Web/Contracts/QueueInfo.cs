namespace JustConveyor.Web.Contracts
{
    public class QueueInfo
    {
        public string QueueType { get; set; }
        public string Blueprint { get; set; }
        public long OnQueue { get; set; }
        public long OnQueuePrevious { get; set; }
        public long OnQueueDelta { get; set; }
    }
}