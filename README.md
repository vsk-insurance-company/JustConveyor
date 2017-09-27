JustConveyor (Micro-ETL framework)
================================
Micro-ETL framework for building integration or ETL in-app processes with conveyor and pipelines model. Initial idea was to get suitable for ease of use model for such processes and use Task-based concurrency for efficient resources usage provided by [TPL](https://msdn.microsoft.com/ru-ru/library/dd460717(v=vs.110).aspx).

Conveyor and Pipelines model description
--------------------

Main entities of model are **Pipelines**, **Suppliers** and **Conveyor**.
* **Pipeline** - sequential logic of unit processing that contains input  queue.
* **Suppliers** - entity that produces packages that contains processing units.
* **Conveyor** - environment for hosting pipelines and routing packages from sources and direct requests to pipelines' input queues.

Also framework define entities:
* **Package** - special box that contains all necessary for routing and processing info about nested unit (*label, headers, load type, external id*).
* **Pipeline blueprint** - description of processing steps sequence
* **Pipeline builder** - class that contains methods for describing pipelines steps sequences
* **Pipeline descriptor** - class that contains info about pipeline: it's descriptor, concurrency limit, packages routing type.
* **Unit context** - context of concrete processing unit, that contains processing headers, processing id, unit id.
* **Transfering context** - context of package processing, that contains processing headers, processing id, unit id.

Superficial processing logic description:
1. Conveyor collects pipelines blueprints and build according to them pipelines
2. Conveyor gets supply channels and starts feed 'em to pipelines routing by packaged unit type or by package label (routing - just pushing packages in input queues by routing strategies)

Usage samples
------------------
As a very base samples we will take trivial task of multiplying given numbers on 2 (*more complex samples* look in samples directory of project *and detailed architecture* look on [project's WIKI](https://github.com/vsk-insurance-company/JustConveyor/wiki)):

**Supplier**
```csharp
[Injecting] // Attribute specifies that class lifetime should be managed by internal IoC container
public class IntegersSupplier : ConveySupplierContract
{
	public IntegersSupplier(IEnumerable<int> ints /*will be injected by injection provider*/)
	{
		mInts = new Queue<int>(ints);
	}

	private readonly Queue<int> mInts;

	public async Task<Package> SupplyNextPackage()
	{
		// emulating delays in acquiring next job unit
		var randomWaitTime = TimeSpan.FromSeconds(new Random(DateTime.Now.Millisecond).Next(1, 10));
		await Task.Delay(randomWaitTime);

		// returning the Fake package to identify end of supplying
		if (mInts.Count == 0)
			return Package.Fake;

		// supplying next package
		var el = mInts.Dequeue();
		return new Package {Id = $"id:{el}", Load = el};
	}
}
```
**Pipeline blueprint builder**
```csharp
[PipelineBuilder("ints-processor")]
public class IntegersProcessorPipelineBuilder
{
	[Blueprint]
	public PipelineDescriptor CreateIntMultPipeline()
	{
		var blueprint = PipelineBlueprint
			// Create blueprint with given name (name just for metrics display)
			.CreateBlueprint<int>("TestIntMultPipeline")			
			.Apply<IntegersProcessor>("multiplyby2")
			.Apply((utx, tctx) => // Just for longer process time duration
			{
				Task.Delay(TimeSpan.FromSeconds(10)).Wait();
				return utx.Unit;
			}, name: "JustWaiting")
			.Apply<IntegersProcessor>("accumulate"); // Accumulate result in "collector"

		return new PipelineDescriptor
		{
			Blueprint = blueprint,
			ConcurrentLinesNumber = 10,
			ForType = true
		};
	}
}
```
**Class with processing logic**
```csharp
[Injecting]
public class IntegersProcessor
{
	private readonly List<int> mCollector;

	public IntegersProcessor([Inject("collector")] List<int> collector)
	{
		mCollector = collector;
	}

	[Processor("multiplyby2")]
	public async Task<int> MultiplyByTwo(int unit)
	{
		// simmulation of async call.
		var randomWaitTime = TimeSpan.FromSeconds(new Random(DateTime.Now.Millisecond).Next(20, 90));
		await Task.Delay(randomWaitTime); 
		
		// just muliplying on 2
		return unit * 2;
	}

	[Processor("accumulate")]
	public void Accumulate(int unit)
	{
		// accumulate result in List.
		mCollector.Add(unit); 
	}
}
```
**Running**
```csharp
internal class Program
{
	// Boostraping conveyor
	private static Finalizer BootstrapContainer()
	{
		var logger = LogManager.GetCurrentClassLogger();
		var container = new IoCContainer();
		container.SetLogger(logger);
		Injection.RegisterInjectionProvider(container);

		// Preparing jobs and finalizer
		// This will be our jobs
		var processingInts = Enumerable.Range(0, 10).ToList();
		container.RegisterSingle<IEnumerable<int>>(processingInts);
		// And in "collector" we will accumulate results.
		container.RegisterSingle("collector", new List<int>());
		// To find out when we can close application we use CountFinalizer
		var finalizer = new CountFinalizer(processingInts.Count,
			() => { logger.Info($"Multiplication result: {string.Join(",", container.Get<List<int>>("collector"))}"); });

		// And boostrap Conveyor itself in fluent way
		Conveyor.Init(logger)
			.ScanForBlueprints()
			.WithMetricsService()
			.WithSupplier("IntsSupplier", Injection.InjectionProvider.Get<IntegersSupplier>())
			.WithFinalizer(finalizer.Finalization)
			.Start();

		return finalizer;
	}

	private static void Main(string[] args)
	{
		// Just bootstrap and then wait
		BootstrapContainer().GetWaitTask().Wait();
	}
}
```

Roadmap
---------------------
Version 1.x
- [ ] profiling
- [ ] show settings/version/additional meta
- [ ] emergent cancellation and suspending
- [ ] internal μservices
- [ ] get last logs
- [ ] admin web-console
- [ ] short circuiting
- [ ] interceptors
- [ ] stop counters during waiting or finish

Version 2.x
- [ ] back pressure
- [ ] external IoC Framework
- [ ] label setting rules (rule-engine)
- [ ] self-tuning
- [ ] clustering
