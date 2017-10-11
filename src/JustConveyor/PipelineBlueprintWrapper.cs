using System;
using System.Collections.Generic;
using JustConveyor.Contracts;
using JustConveyor.Contracts.Pipelining.Contexts;
using JustConveyor.Contracts.Queues;
using JustConveyor.Pipelining;

namespace JustConveyor
{
    internal class PipelineBlueprintWrapper
    {
        public BlueprintCounters Counters { get; } = new BlueprintCounters();
        public Type BuilderType { get; set; }
        public List<Action<Package, TransferingContext>> Finalizers { get; } = new List<Action<Package, TransferingContext>>();
        public uint ConcurrentLines { get; set; }
        public string RoutingName { get; set; }
        public Pipeline Pipeline { get; set; }
        public QueueProviderContract QueueProvider { get; set; }
    }
}