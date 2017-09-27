using System;
using System.Linq;
using System.Web.Http;
using JustConveyor.Web.Contracts;

namespace JustConveyor.Web.Controllers
{
    public class MetricsController : ApiController
    {
        public ConveyorMetrics Get()
        {
            var conveyor = Conveyor.Instance;
            var response = new ConveyorMetrics
            {
                Started = conveyor.Started,
                InFlightTime = StringifyTimespan(DateTime.Now - conveyor.Started),
                Pipelines = conveyor.RunningPipelines.Select(el => new PipelineInfo
                {
                    Id = el.Id,
                    State = el.State
                }),
                Blueprints = conveyor.BlueprintsMap.Values.Select(el => new BlueprintInfo
                {
                    BuilderClass =
                        el.BuilderType != null ? el.BuilderType.AssemblyQualifiedName : "Unknown",
                    ConcurrencyLevel = el.ConcurrentLines,
                    Name = el.Pipeline.Blueprint.Name,
                    RoutingName = el.RoutingName,
                    In = el.Counters.In,
                    Out = el.Counters.Out,
                    ProcessingRatePerSec = el.Counters.ProcessingRatePerSec
                }),
                Queues = conveyor.BlueprintsMap.Values.Select(el => new QueueInfo
                {
                    Blueprint = el.Pipeline.Id,
                    QueueType = el.QueueProvider.GetType().AssemblyQualifiedName,
                    OnQueue = el.Counters.OnQueue,
                    OnQueuePrevious = el.Counters.OnQueuePrev,
                    OnQueueDelta = el.Counters.OnQueueDelta,
                }),
                Suppliers = conveyor.ConveySuppliers.Select(el => new SupplierInfo
                {
                    Name = el.Name,
                    State = el.State,
                    SupplierType = el.Supplier.GetType().AssemblyQualifiedName,
                    Supplied = el.SuppliedPackagesCount,
                    PackagesRatePerSec = el.PackagesPerSec
                }),
                Contextes = conveyor.RunningContextes.Select(el => new TransferingContextInfo
                {
                    Id = el.Key,
                    Step = el.Value.ProcessingHistory.Peek().StepName,
                    ProcessingStart = el.Value.ProcessingStart,
                    ProcessingHistory =
                        el.Value.ProcessingHistory.Where(hel => hel.Finished != null)
                            .Select(hel => new ContextProcessingHistoryInfo
                            {
                                StepName = hel.StepName,
                                ProcessingTime = StringifyTimespan(hel.Finished.Value - hel.Started)
                            }),
                    InProcessing = StringifyTimespan(DateTime.Now - el.Value.ProcessingStart)
                })
            };
            return response;
        }

        private static string StringifyTimespan(TimeSpan ts)
        {
            return
                $"{Render(ts.Days, "d")}{Render(ts.Hours, "h")}{Render(ts.Minutes, "m")}{ts.Seconds},{ts.Milliseconds.ToString().PadLeft(3, '0')}s";
        }

        private static string Render(int num, string letter)
        {
            return num == 0 ? "" : $"{num}{letter}";
        }
    }
}