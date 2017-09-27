using System.Collections.Generic;
using System.Linq;
using NLog;
using JustConveyor;
using JustConveyor.Contracts.Dependencies;
using JustConveyor.Contracts.Utils;
using JustConveyor.Injection;

namespace IntegersMultiplier
{
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

			// And boostrapping Conveyor itself
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
			BootstrapContainer().GetWaitTask().Wait();
		}
	}
}