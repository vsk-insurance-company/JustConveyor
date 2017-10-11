using System.Collections.Generic;
using JustConveyor.Contracts;

namespace JustConveyor
{
    internal class PipelineInstance
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public PipelineBlueprintWrapper Blueprint { get; set; }
        public string State { get; set; }

        public PipelineInstantCounters Counters { get; set; } = new PipelineInstantCounters();
        public Stack<PackageProcessProfile> PackagesProfiles { get; set; } = new Stack<PackageProcessProfile>();
    }
}