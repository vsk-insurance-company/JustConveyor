using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JustConveyor.Contracts;
using JustConveyor.Contracts.Attributes;
using JustConveyor.Contracts.Dependencies;
using JustConveyor.Contracts.Exceptions;
using JustConveyor.Contracts.Pipelining;
using JustConveyor.Contracts.Pipelining.Contexts;
using JustConveyor.Contracts.Queues;
using JustConveyor.Contracts.Settings;
using JustConveyor.Contracts.Utils;
using JustConveyor.Pipelining;
using JustConveyor.Queues;
using JustConveyor.Utils;
using JustConveyor.Web;
using MathNet.Numerics.Statistics;
using Nito.AsyncEx;
using NLog;

namespace JustConveyor
{
	public sealed class Conveyor : ConveyorContract
	{
		/// <summary>
		/// Class for storing pipeline data.
		/// </summary>
		/// <summary>
		/// Instance of conveyor.
		/// </summary>
		public static Conveyor Instance { get; private set; }

		internal readonly ConcurrentBag<PipelineInstance> RunningPipelines = new ConcurrentBag<PipelineInstance>();
		internal readonly ConcurrentBag<SupplierInstance> ConveySuppliers = new ConcurrentBag<SupplierInstance>();

		internal readonly Dictionary<string, PipelineBlueprintWrapper> BlueprintsMap =
			new Dictionary<string, PipelineBlueprintWrapper>();

		internal readonly ConcurrentDictionary<string, TransferingContext> RunningContextes =
			new ConcurrentDictionary<string, TransferingContext>();

		/// <summary>
		/// Initialize instance of conveyor.
		/// </summary>
		/// <param name="logger"></param>
		/// <param name="settings"></param>
		/// <returns></returns>
		public static Conveyor Init(Logger logger = null,
			ConveyorRunSettings settings = null)
		{
			if (Instance != null)
				throw new ConveyorInstanceAlreadyInitializer();
			Instance = new Conveyor(settings ?? ConveyorRunSettings.Default, logger);
			return Instance;
		}

		/// <summary>
		/// CTOR.
		/// </summary>
		/// <param name="settings"></param>
		/// <param name="logger"></param>
		private Conveyor(ConveyorRunSettings settings, Logger logger = null)
		{
			mSettings = settings;
			mLogger = logger;

			mWaitThread = new Thread(SetTrigger)
			{
				IsBackground = true
			};
			mWaitThread.Start();
			mCountersHarvestThread = new Thread(HarvestCounters)
			{
				IsBackground = true
			};
			mCountersHarvestThread.Start();

			try
			{
				mQueueManager = Injection.InjectionProvider.Get<QueuesManagerContract>();
			}
			catch (Exception e)
			{
				mLogger?.Warn(
					$"No any queue managers registered by contract QueueManagerContract: {e}. Default InMemoryQueueManager will be loaded.");
				mQueueManager = new InMemoryQueueManager();
			}

			Injection.InjectionProvider.RegisterSingle<ConveyorContract>(this);
		}

		private readonly AsyncManualResetEvent mWaitEvent = new AsyncManualResetEvent();
		private readonly Thread mWaitThread;
		private readonly Thread mCountersHarvestThread;


		private readonly ConcurrentDictionary<Guid, WaitBox> mWaitBoxes = new ConcurrentDictionary<Guid, WaitBox>();

		private readonly List<Action<Package>> mLostPackagesProcessors = new List<Action<Package>>();

		private readonly Dictionary<string, SupplierInstance> mSuppliers =
			new Dictionary<string, SupplierInstance>();

		private readonly QueuesManagerContract mQueueManager;
		private Logger mLogger;

		private readonly ConveyorRunSettings mSettings;

		internal DateTime Started;
		private MetricsServiceSettings mServiceSettings;

		private void SetTrigger()
		{
			while (true)
			{
				Thread.Sleep(TimeSpan.FromSeconds(mSettings.PipelineCoolDownSecondsTimeout));
				mWaitEvent.Set();
				mWaitEvent.Reset();
			}
		}

		private void HarvestCounters()
		{
			while (true)
			{
				Thread.Sleep(TimeSpan.FromSeconds(mSettings.InstantCountersHarversingSecondsTimeout));

				foreach (var blueprint in BlueprintsMap.Values)
				{
					blueprint.Counters.OnQueuePrev = blueprint.Counters.OnQueue;
					blueprint.Counters.OnQueue = blueprint.QueueProvider.Count;
					blueprint.Counters.OnQueueDelta = blueprint.Counters.OnQueue - blueprint.Counters.OnQueuePrev;
				}
				foreach (var pipelineInstance in RunningPipelines)
				{
					var profiles = pipelineInstance.PackagesProfiles;
					pipelineInstance.PackagesProfiles = new Stack<PackageProcessProfile>();
					var profilesList = profiles.ToList();
					pipelineInstance.Counters.WaitTimeProfile =
						GetStats(profilesList.Select(el => (el.Dequeued - el.Queued).Value.TotalSeconds).ToList());
					pipelineInstance.Counters.ProcesTimeProfile =
						GetStats(
							profilesList.Select(el => (el.ProcessFinished - el.Dequeued).Value.TotalSeconds).ToList());
				}
			}
		}

		private static TimeProfile GetStats(List<double> sequence)
		{
			var any = sequence.Any();
			return new TimeProfile
			{
				Max = any ? sequence.Max() : 0,
				Min = any ? sequence.Min() : 0,
				Median = any ? sequence.Median() : 0
			};
		}

		private void RouteMessage(Package package)
		{
			if (package == null) throw new ArgumentNullException(nameof(package));
			var routed = false;
			var queue = BlueprintsMap.ContainsKey(package.LoadType?.AssemblyQualifiedName ?? "")
				? BlueprintsMap[package.LoadType?.AssemblyQualifiedName ?? ""].QueueProvider
				: (BlueprintsMap.ContainsKey(package.Label ?? "")
					? BlueprintsMap[package.Label ?? ""].QueueProvider
					: null);
			if (queue != null)
			{
				routed = true;
				package.Profile.Queued = DateTime.Now;
				queue.Publish(package);
			}

			if (!routed)
			{
				mLogger?.Warn(
					$"Package with id:{package.Id} label:{package.Label} and loadType:{package.LoadType} wasn't routed and will be processed by registered processors.");
				foreach (var processor in mLostPackagesProcessors)
				{
					try
					{
						processor(package);
					}
					catch (Exception exception)
					{
						mLogger?.Error(
							$"Exception occured during processing lost package id:{package.Id}. Exception: {exception}");
					}
				}
			}
		}

		private async Task RunPipeline(PipelineInstance instance)
		{
			await Task.Yield();

			RunningPipelines.Add(instance);

			mLogger?.Trace($"Processing line id:{instance.Id} started for blueprint '{instance.Name}'");
			instance.State = "Initialized";

			Package package = await instance.Blueprint.QueueProvider.ReceiveNextMessageAsync();
			while (package != null)
			{
				if (package == Package.Fake)
				{
					mLogger?.Trace($"line:{instance.Id} | Cool down before next attempt to get next package.");
					await mWaitEvent.WaitAsync();
				}
				else
				{
					package.Profile.Dequeued = DateTime.Now;
					instance.Blueprint.Counters.IncrementIn();

					mLogger?.Trace($"line:{instance.Id} | Incomming package with id:{package.Id}.");
					instance.State = "Processing";

					var transferingContext = new TransferingContext
					{
						Id = $"{package.Id}:{instance.Id}:{Guid.NewGuid().ToString("n")}",
						ProcessingStart = DateTime.Now
					};
					transferingContext.Meta.Add("ID", transferingContext.Id);
					transferingContext.Meta.Add("Package label", package.Label);
					transferingContext.Meta.Add("Package ID", package.Id);

					try
					{
						if (!RunningContextes.TryAdd(transferingContext.Id, transferingContext))
							mLogger?.Warn($"Unable to register transfering context with id:{transferingContext.Id}");

						await instance.Blueprint.Pipeline.Process(transferingContext, package.Load, package.Id,
							Guid.NewGuid().ToString("n"),
							new Headers(package.Headers));

						if (transferingContext.Exception != null)
						{
							instance.Blueprint.Counters.Errors++;
							mLogger?.Error(
								$"Exception occured during processing on line id:{instance.Id}. Exception: {transferingContext.Exception}");
						}

						if (package.ExternalDeliverBoxId != null)
							mWaitBoxes[package.ExternalDeliverBoxId.Value].SetValue(
								transferingContext.FinalUnitContext.Unit,
								transferingContext.Exception);
					}
					catch (Exception e)
					{
						instance.Blueprint.Counters.Errors++;

						if (package.ExternalDeliverBoxId != null)
							mWaitBoxes[package.ExternalDeliverBoxId.Value].SetValue(null, e);
					}
					finally
					{
						ProcessingInfo info = new ProcessingInfo
						{
							StepName = "FINALIZATION",
							Started = DateTime.Now
						};
						transferingContext.ProcessingHistory.Push(info);

						foreach (var finalizer in instance.Blueprint.Finalizers)
						{
							try
							{
								finalizer(package, transferingContext);
							}
							catch (Exception ex)
							{
								mLogger?.Error(
									$"Error occured during finalizing package with id:{package.Id}. Exception: {ex}");
							}
						}
						info.Finished = DateTime.Now;

						if (!RunningContextes.TryRemove(transferingContext.Id, out transferingContext))
							mLogger?.Warn($"Unable to unregister transfering context with id:{transferingContext.Id}");

						instance.Blueprint.Counters.IncrementOut();

						package.Profile.ProcessFinished = DateTime.Now;
						instance.PackagesProfiles.Push(package.Profile);
						package.Profile = null;
					}
				}

				instance.State = "WaitForMessage";
				mLogger?.Trace($"line:{instance.Id} | Wait for package.");
				package = await instance.Blueprint.QueueProvider.ReceiveNextMessageAsync();
			}

			instance.State = "Finished";
			mLogger?.Trace($"Processing line id:{instance.Id} finished for blueprint '{instance.Name}'");
		}

		async Task RunSupplier(SupplierInstance instance)
		{
			await Task.Yield();

			ConveySuppliers.Add(instance);
			instance.State = "Initialized";
			instance.Started = DateTime.Now;

			mLogger?.Trace($"Worker for '{instance.Name}' supplier started.");

			Package nextPackage = null;
			while (nextPackage != Package.Fake)
			{
				instance.State = "WaitForNextPackage";

				try
				{
					nextPackage = await instance.Supplier.SupplyNextPackage();

					if (nextPackage != null)
					{
						instance.SuppliedPackagesCount++;
						instance.State = "Routing";
						RouteMessage(nextPackage);
					}
				}
				catch (Exception exception)
				{
					instance.ErrorsCount++;
					mLogger?.Error($"Error occured during supplying package: {exception}");
				}
			}

			instance.State = "Finished";

			mLogger?.Trace($"Worker for '{instance.Name}' supplier exhausted supply channel.");
		}

		public Conveyor WithLogger(Logger logger)
		{
			mLogger = logger;
			return this;
		}

		public Conveyor WithDefaultLogger()
		{
			mLogger = LogManager.GetLogger("conveyor");
			return this;
		}

		public Conveyor ScanForBlueprints()
		{
			foreach (
				var pipelineBuilder in
					AppDomain.CurrentDomain.GetAssemblies()
						.SelectMany(
							el =>
								el.GetTypes()
									.Where(
										t => Attribute.GetCustomAttribute(t, typeof(PipelineBuilderAttribute)) != null))
						.Select(
							el =>
								new
								{
									type = el,
									attribute =
										(PipelineBuilderAttribute)
											Attribute.GetCustomAttribute(el, typeof(PipelineBuilderAttribute))
								}))
			{
				var builderObject = Injection.InjectionProvider.Get(pipelineBuilder.type);
				foreach (
					var blueprint in
						pipelineBuilder.type.GetMethods()
							.Where(el => Attribute.GetCustomAttribute(el, typeof(BlueprintAttribute)) != null)
							.Select(
								el =>
									new
									{
										method = el,
										attribute =
											(BlueprintAttribute)
												Attribute.GetCustomAttribute(el, typeof(BlueprintAttribute))
									}))
				{
					var descriptor = (PipelineDescriptor) blueprint.method.Invoke(builderObject, new object[] {});
					descriptor.BuilderType = builderObject.GetType();
					WithBlueprint(descriptor);
				}
			}
			return this;
		}

		public async Task<Package> Process(Package request, string pipelineName = "")
		{
			var deliveryId = Guid.NewGuid();
			request.ExternalDeliverBoxId = deliveryId;
			var waitBox = new WaitBox();

			if (!mWaitBoxes.TryAdd(deliveryId, waitBox))
				throw new UnableToPostPackageOnProcessing();
			try
			{
				RouteMessage(request);
				var result = await waitBox.WaitTask();
				return new Package
				{
					Load = result.Load
				};
			}
			finally
			{
				mWaitBoxes.TryRemove(deliveryId, out waitBox);
			}
		}

		/// <summary>
		/// Set blueprint that should be added to the Conveyor.
		/// </summary>
		/// <param name="descriptor"></param>
		/// <returns></returns>
		public ConveyorContract WithBlueprint(PipelineDescriptor descriptor)
		{
			if (descriptor == null) throw new ArgumentNullException(nameof(descriptor));
			var blueprintWrapper = new PipelineBlueprintWrapper
			{
				BuilderType = descriptor.BuilderType,
				Pipeline = new Pipeline(descriptor.Blueprint),
				QueueProvider = mQueueManager.CreateQueue($"{descriptor.Blueprint.Name}_queue"),
				ConcurrentLines = descriptor.ConcurrentLinesNumber,
				RoutingName =
					descriptor.ForType ? descriptor.Blueprint.SeedType.AssemblyQualifiedName : descriptor.Blueprint.Name
			};

			BlueprintsMap.Add(blueprintWrapper.RoutingName, blueprintWrapper);
			return this;
		}

		public ConveyorContract WithMetricsService(MetricsServiceSettings service = null)
		{
			mServiceSettings = service ?? new MetricsServiceSettings
			{
				BaseAddress = "http://*:9910/",
				CorsAddresses = new List<string> {"http://localhost/*"}
			};
			Injection.InjectionProvider.RegisterSingle("conveyor:metrics-service-settings", mServiceSettings);
			MetricsWebService.StartService(mServiceSettings);
			return this;
		}

		public ConveyorContract WithSupplier(string supplierName, ConveySupplierContract supplier)
		{
			if (supplier == null) throw new ArgumentNullException(nameof(supplier));
			mSuppliers.Add(supplierName, new SupplierInstance
			{
				Name = supplierName,
				Supplier = supplier
			});
			return this;
		}

		public ConveyorContract WithPipelineFinalizer(string pipelineName, Action<Package, TransferingContext> finalizer)
		{
			if (finalizer == null) throw new ArgumentNullException(nameof(finalizer));
			if (!BlueprintsMap.ContainsKey(pipelineName))
				throw new BlueprintNotRegisteredException(pipelineName);

			BlueprintsMap[pipelineName].Finalizers.Add(finalizer);
			return this;
		}

		public ConveyorContract WithFinalizer(Finalizer finalizer)
		{
			if (finalizer == null) throw new ArgumentNullException(nameof(finalizer));
			foreach (var pipelineBlueprintWrapper in BlueprintsMap)
				pipelineBlueprintWrapper.Value.Finalizers.Add(finalizer.Finalization);
			return this;
		}

		public ConveyorContract WithLostPackagesProcessor(Action<Package> lostPackageProcessor)
		{
			if (lostPackageProcessor == null) throw new ArgumentNullException(nameof(lostPackageProcessor));
			mLostPackagesProcessors.Add(lostPackageProcessor);
			return this;
		}

		public void Stop()
		{
			mWaitThread.Abort();
			mCountersHarvestThread.Abort();
			mQueueManager.Close();
		}

		public void Start()
		{
			if (mSuppliers.Count == 0)
				throw new NoAnySupplierRegisteredException();

			if (BlueprintsMap.Count == 0)
				throw new NoAnyBlueprintRegisteredException();

			Started = DateTime.Now;

			var tasks = new List<Task>();
			foreach (var blueprintWrapper in BlueprintsMap)
			{
				blueprintWrapper.Value.Counters.Started = DateTime.Now;
				tasks.Add(RunPipeline(new PipelineInstance
				{
					Blueprint = blueprintWrapper.Value,
					Id = $"{blueprintWrapper.Value.Pipeline.Id}:{1}",
					Name = blueprintWrapper.Key
				}));
			}

			foreach (var conveySupplier in mSuppliers)
			{
				tasks.Add(RunSupplier(conveySupplier.Value));
			}

			foreach (var blueprintWrapper in BlueprintsMap)
			{
				if ((int) blueprintWrapper.Value.ConcurrentLines > 1)
					Enumerable.Range(2, (int) blueprintWrapper.Value.ConcurrentLines - 1).ToList().ForEach(num =>
					{
						tasks.Add(RunPipeline(new PipelineInstance
						{
							Blueprint = blueprintWrapper.Value,
							Id = $"{blueprintWrapper.Value.Pipeline.Id}:{num}",
							Name = blueprintWrapper.Key
						}));
					});
			}

			mLogger?.Trace($"Starting process...");
		}
	}
}