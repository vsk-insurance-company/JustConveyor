using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using FluentAssertions;
using JustConveyor.Contracts.Utils;
using JustConveyor.Injection;
using JustConveyor.Tests.TestProcessModel.Conveyor;
using JustConveyor.Tests.TestProcessModel.Pipelines;
using NLog;
using NLog.Config;
using NLog.Targets;
using NUnit.Framework;

namespace JustConveyor.Tests
{
	[TestFixture]
	public class ConveyorTests
	{
		private MemoryTarget mLogs;
		private Logger mLogger;

		[SetUp]
		public void Setup()
		{
			mLogs = new MemoryTarget {Layout = @"${date:format=HH\:mm\:ss} ${logger} ${message}"};
			SimpleConfigurator.ConfigureForTargetLogging(mLogs, LogLevel.Debug);

			mLogger = LogManager.GetLogger("Tests");
			Contracts.Dependencies.Injection.RegisterLogger(mLogger);
		}

		[Test]
		public void SimpleConveyorShouldWorksWell()
		{
			// GIVEN
			var injectionProvider = new IoCContainer();
			var listCollector = new ListCollector();

			injectionProvider.RegisterSingle(new List<int> {1, 2, 3, 4, 5});
			injectionProvider.Register<TestIntSource>();
			injectionProvider.Register<TestMultiplicatorStep>();
			injectionProvider.Register<TestPipelinesBuilder>();
			injectionProvider.RegisterSingle(listCollector);

			Contracts.Dependencies.Injection.RegisterLogger(mLogger);
			Contracts.Dependencies.Injection.RegisterInjectionProvider(injectionProvider, false);

			var range = new List<int> {1, 2, 3, 4, 5, 6, 7};

			// WHEN
		    var countFinalizer = new CountFinalizer(range.Count);
            Conveyor.Init(mLogger)
		        .ScanForBlueprints()
		        .WithSupplier("IntsSupplier", new IntSupplier(range))
                .WithFinalizer(countFinalizer.Finalization)
		        .Start();

		    countFinalizer.GetWaitTask().Wait();

			// THEN
			listCollector.Lists.Count.Should().Be(7, "All lists should be added to list collector");
			listCollector.Lists.Values.ToList().ForEach(el => el.ShouldAllBeEquivalentTo(new List<int> {2, 4, 6, 8, 10}));
			listCollector.Lists.Keys.ShouldBeEquivalentTo(range.Select(el => $"id:{el}"));
		}

		[Test]
		public void ConveyorWithPipelineCallShouldWorksWell()
		{
			Stopwatch timer = Stopwatch.StartNew();
			// GIVEN
			var injectionProvider = new IoCContainer();
			var listCollector = new ListCollector();

			injectionProvider.RegisterSingle(new List<int> {1, 2, 3, 4, 5});
			injectionProvider.Register<TestIntSource>();
			injectionProvider.Register<TestMultiplicatorStep>();
			injectionProvider.Register<TestPipelinesBuilder>();
			injectionProvider.RegisterSingle(listCollector);

			Contracts.Dependencies.Injection.RegisterLogger(mLogger);
			Contracts.Dependencies.Injection.RegisterInjectionProvider(injectionProvider, false);

			var count = 10;
			var range = Enumerable.Range(1, count).ToList();

			var builder = new TestPipelinesBuilder();

			// WHEN
		    var countFinalizer = new CountFinalizer(count);
			Conveyor.Init(mLogger)
				.WithBlueprint(builder.CreateIntMultPipeline())
				.WithBlueprint(builder.CreatePipelineWithCallOfAnother())
                .WithPipelineFinalizer("TestPipelineWithCallOfAnother", countFinalizer.Finalization)
				.WithSupplier("IntsSupplier", new LabeledIntSupplier(range, "TestPipelineWithCallOfAnother"))
				.Start();

		    countFinalizer.GetWaitTask().Wait();

			// THEN
			listCollector.Lists.Count.Should().Be(count, "All lists should be added to list collector");
			listCollector.Lists.Values.ToList().ForEach(el => el.ShouldAllBeEquivalentTo(new List<int> {2, 4, 6, 8, 10}));
			listCollector.Lists.Keys.ToList().ShouldBeEquivalentTo(range.Select(el => $"id:{el}").ToList());
			timer.Stop();
		}
	}
}