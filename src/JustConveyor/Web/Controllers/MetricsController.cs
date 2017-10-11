using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Http;
using JustConveyor.Contracts.Dependencies;
using JustConveyor.Contracts.Settings;
using JustConveyor.Web.Contracts;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;
using NLog.Targets;

namespace JustConveyor.Web.Controllers
{
	public class MetricsController : ApiController
	{
		private readonly MetricsServiceSettings mSettings;
		private readonly ILogger mLogger;

		public MetricsController()
		{
			mSettings = Injection.InjectionProvider.Get<MetricsServiceSettings>("conveyor:metrics-service-settings");
			try
			{
				mLogger = Injection.InjectionProvider.Get<ILogger>("metrics-service");
			}
			catch (Exception)
			{
				var logger = LogManager.GetCurrentClassLogger();
				Injection.InjectionProvider.RegisterSingle("metrics-service", logger);
				logger.Warn("Logger for metrics service not found.");
			}
		}

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
					Errors = el.Counters.Errors,
					In = el.Counters.In,
					Out = el.Counters.Out,
					ProcessingRatePerSec = el.Counters.ProcessingRatePerSec
				}),
				Queues = conveyor.BlueprintsMap.Values.Select(el => new QueueInfo
				{
					Blueprint = el.Pipeline.Blueprint.Name,
					QueueType = el.QueueProvider.GetType().AssemblyQualifiedName,
					OnQueue = el.Counters.OnQueue,
					OnQueuePrevious = el.Counters.OnQueuePrev,
					OnQueueDelta = el.Counters.OnQueueDelta,
				}),
				Suppliers = conveyor.ConveySuppliers.Select(el => new SupplierInfo
				{
					Name = el.Name,
					State = el.State,
					Errors = el.ErrorsCount,
					SupplierType = el.Supplier.GetType().AssemblyQualifiedName,
					Supplied = el.SuppliedPackagesCount,
					PackagesRatePerSec = el.PackagesPerSec
				}),
				Contexts = conveyor.RunningContextes.Select(el => new TransferingContextInfo
				{
					Id = el.Key,
					Step = el.Value.ProcessingHistory.Peek().StepName,
					Meta = JObject.FromObject(el.Value.Meta),
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

			if (mSettings.MetricsConfig != null)
				foreach (var targetName in mSettings.MetricsConfig.IncludeLastLogsFrom)
				{
					try
					{
						var target = LogManager.Configuration.FindTargetByName(targetName);
						if (target is FileTarget)
						{
							var fileTarget = (FileTarget) target;
							response.Loggers.Add(ReadLastLines(fileTarget));
						}
						else
						{
							mLogger?.Warn($"Target {targetName} is not FileTarget: Unable to read not file target.");
						}
					}
					catch (Exception e)
					{
						mLogger?.Error($"Unable to read target {targetName}: {e}");
					}
				}

			return response;
		}

		private LoggerInfo ReadLastLines(FileTarget fileTarget)
		{
			var logEventInfo = new LogEventInfo {TimeStamp = DateTime.Now};
			string fileName = fileTarget.FileName.Render(logEventInfo);
			if (!File.Exists(fileName))
				throw new Exception("Log file does not exist.");

			string resultString = "";
			int cntr = 1;
			using (var fs = File.OpenRead(fileName))
			{
				while (resultString.Count(el => el == '\n') < mSettings.MetricsConfig.CountOfLogLines)
				{
					var offset = 1024*cntr < fs.Length ? 1024*cntr : fs.Length;
					var length = 1024*cntr < fs.Length ? 1024 : fs.Length - 1024*(cntr - 1);
					fs.Seek(-offset, SeekOrigin.End);
					var buf = new byte[length];
					fs.Read(buf, 0, (int) length);
					resultString = Encoding.UTF8.GetString(buf) + resultString;
					cntr++;

					if (offset == fs.Length)
						break;
				}
			}

			var result = new LoggerInfo
			{
				Name = fileTarget.Name,
				FilePath = fileName,
				LastLogs =
					resultString.Substring(resultString.IndexOf('\n') + 1)
						.Replace("\r", "")
						.Split('\n')
						.Where(el => !string.IsNullOrEmpty(el))
						.ToList()
			};
			return result;
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