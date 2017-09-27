namespace JustConveyor.Web.Contracts
{
    public class BlueprintInfo
    {
        public string BuilderClass { get; set; }
        public string Name { get; set; }
        public string RoutingName { get; set; }
        public uint ConcurrencyLevel { get; set; }
        public long In { get; set; }
        public long Out { get; set; }
        public double ProcessingRatePerSec { get; set; }
    }
}