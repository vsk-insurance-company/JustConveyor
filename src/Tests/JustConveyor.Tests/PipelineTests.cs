using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using JustConveyor.Contracts;
using JustConveyor.Contracts.Exceptions;
using JustConveyor.Contracts.Pipelining;
using JustConveyor.Contracts.Pipelining.Contexts;
using JustConveyor.Injection;
using JustConveyor.Pipelining;
using JustConveyor.Tests.TestProcessModel.Pipelines;
using NLog;
using NLog.Config;
using NLog.Targets;
using NUnit.Framework;

namespace JustConveyor.Tests
{
    [TestFixture]
    public class PipelineTests
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
        public void WithoutTasksShouldWorksWell()
        {
            // GIVEN
            mLogs.Logs.Clear();
            var injectionProvider = new IoCContainer();
            injectionProvider.Register<TestMultiplicatorStep>();
            Contracts.Dependencies.Injection.RegisterInjectionProvider(injectionProvider, false);
            PipelineBlueprint blueprint = PipelineBlueprint.CreateBlueprint(typeof(int), "SingleApply");
            PipelineBlueprint blueprintOther = PipelineBlueprint.CreateBlueprint(typeof(int), "DoubleApply");

            // WHEN
            TransferingContext tctx = new TransferingContext(new Headers {{"factor", 10}}.Dict);
            new Pipeline(blueprint.Apply<TestMultiplicatorStep>("sync_mbf"))
                .Process(tctx, 1).Wait();

            TransferingContext otherTctx = new TransferingContext(new Headers {{"factor", 10}}.Dict);
            new Pipeline(blueprintOther.Apply<TestMultiplicatorStep>("sync_mbf")
                .Apply<TestMultiplicatorStep>("sync_mbf"))
                .Process(otherTctx, 1).Wait();

            // THAN
            tctx.GetResult<int>().Should().Be(10);
            otherTctx.GetResult<int>().Should().Be(100);
        }

        [Test]
        public void SingleTaskWithActionsShouldWorksWell()
        {
            // GIVEN
            mLogs.Logs.Clear();
            var injectionProvider = new IoCContainer();
            injectionProvider.Register<TestMultiplicatorStep>();
            Contracts.Dependencies.Injection.RegisterInjectionProvider(injectionProvider, false);
            PipelineBlueprint blueprint = PipelineBlueprint.CreateBlueprint(typeof(int), "SingleApply");
            PipelineBlueprint blueprintOther = PipelineBlueprint.CreateBlueprint(typeof(int), "DoubleApply");

            // WHEN
            TransferingContext tctx = new TransferingContext(new Headers {{"factor", 10}}.Dict);
            new Pipeline(blueprint
                .Apply<TestMultiplicatorStep>("multiplyby2")
                .Apply<TestMultiplicatorStep>("sync_mbf"))
                .Process(tctx, 1).Wait();

            TransferingContext otherTctx = new TransferingContext(new Headers {{"factor", 10}}.Dict);
            new Pipeline(blueprintOther
                .Apply<TestMultiplicatorStep>("sync_mbf")
                .Apply<TestMultiplicatorStep>("multiplyby2")
                .Apply<TestMultiplicatorStep>("sync_mbf"))
                .Process(otherTctx, 1).Wait();

            // THAN
            tctx.GetResult<int>().Should().Be(20);
            otherTctx.GetResult<int>().Should().Be(200);
        }

        [Test]
        public void OnIncorrectParameterTypeExceptionShouldReturns()
        {
            // GIVEN
            mLogs.Logs.Clear();
            var injectionProvider = new IoCContainer();
            injectionProvider.Register<TestMultiplicatorStep>();
            Contracts.Dependencies.Injection.RegisterInjectionProvider(injectionProvider, false);
            PipelineBlueprint blueprint = PipelineBlueprint.CreateBlueprint(typeof(int), "SingleApply");

            // WHEN
            TransferingContext tctx = new TransferingContext();
            new Pipeline(blueprint
                .Apply((string str) => str))
                .Process(tctx, 1).Wait();

            // THAN
            tctx.Exception.Should().NotBeNull();
            tctx.Exception.InnerException.Should().NotBeNull();
            tctx.Exception.InnerException.GetType().Should().Be(typeof(ParameterTypeMissmatchException));
        }

        [Test]
        public void SingleTaskShouldWorksWell()
        {
            // GIVEN
            mLogs.Logs.Clear();
            var injectionProvider = new IoCContainer();
            injectionProvider.Register<TestMultiplicatorStep>();
            Contracts.Dependencies.Injection.RegisterInjectionProvider(injectionProvider, false);
            PipelineBlueprint blueprint = PipelineBlueprint.CreateBlueprint(typeof(int), "SingleApply");

            // WHEN
            TransferingContext tctx = new TransferingContext();
            new Pipeline(blueprint.Apply<TestMultiplicatorStep>("multiplyby2"))
                .Process(tctx, 1).Wait();

            // THAN
            tctx.GetResult<int>().Should().Be(2);
        }

        [Test]
        public void SyncSpitCollectShouldWorksWell()
        {
            // GIVEN
            mLogs.Logs.Clear();

            var ints = new List<int> {1, 2, 3};

            var injectionProvider = new IoCContainer();
            injectionProvider.RegisterSingle(ints);
            injectionProvider.Register<TestIntSource>();
            injectionProvider.Register<TestMultiplicatorStep>();
            Contracts.Dependencies.Injection.RegisterInjectionProvider(injectionProvider, false);

            PipelineBlueprint blueprint = PipelineBlueprint.CreateBlueprint(typeof(int), "SyncSplitCollect");

            // WHEN
            var result = blueprint
                .Split<TestIntSource>()
                .Apply((int el) => el*10)
                .Collect()
                .Apply((IList<UnitContext> octx) => octx.Select(el => el.GetUnit<int>()));

            TransferingContext tctx = new TransferingContext();
            new Pipeline(result)
                .Process(tctx, 1)
                .Wait();

            // THAN
            tctx.Exception.Should().BeNull();
            tctx.GetResult<IEnumerable<int>>().ShouldBeEquivalentTo(new List<int> {10, 20, 30});
        }

        [Test]
        public void AsyncSpitCollectShouldWorksWell()
        {
            // GIVEN
            mLogs.Logs.Clear();

            var ints = new List<int> {1, 2, 3};

            var injectionProvider = new IoCContainer();
            injectionProvider.RegisterSingle(ints);
            injectionProvider.Register<TestIntSource>();
            injectionProvider.Register<TestMultiplicatorStep>();
            Contracts.Dependencies.Injection.RegisterInjectionProvider(injectionProvider, false);

            PipelineBlueprint blueprint = PipelineBlueprint.CreateBlueprint(typeof(int), "AsyncSplitCollect");
            PipelineBlueprint blueprintOther = PipelineBlueprint.CreateBlueprint(typeof(int), "OtherAsyncSplitCollect");
            PipelineBlueprint blueprintOneOther = PipelineBlueprint.CreateBlueprint(typeof(int),
                "OneOtherAsyncSplitCollect");

            // WHEN
            TransferingContext tctx = new TransferingContext(new Headers { { "factor", 10 } }.Dict);
            new Pipeline(blueprint
                .Split<TestIntSource>()
                .Apply<TestMultiplicatorStep>("multiplybyfactor")
                .Collect()
                .Apply((IList<UnitContext> octx) => octx.Select(el => el.GetUnit<int>()).ToList()))
                .Process(tctx, 1)
                .Wait();

            TransferingContext otherTctx = new TransferingContext(new Headers { { "factor", 10 } }.Dict);
            new Pipeline(blueprintOther
                .Split<TestIntSource>()
                .Apply<TestMultiplicatorStep>("multiplybyfactor")
                .Collect()
                .Apply<TestMultiplicatorStep>("strip"))
                .Process(otherTctx, 1)
                .Wait();

            TransferingContext oneAnotherTctx = new TransferingContext(new Headers {{"factor", 10}}.Dict);
            new Pipeline(blueprintOneOther
                .Apply<TestMultiplicatorStep>("multiplybyfactor")
                .Split<TestIntSource>()
                .Apply<TestMultiplicatorStep>("multiplybyfactor")
                .Collect()
                .Apply<TestMultiplicatorStep>("strip"))
                .Process(oneAnotherTctx, 1)
                .Wait();

            // THAN
            tctx.Exception?.InnerException.Should().BeNull();
            tctx.GetResult<List<int>>().ShouldBeEquivalentTo(new List<int> {10, 20, 30});

            otherTctx.Exception?.InnerException.Should().BeNull();
            otherTctx.GetResult<List<int>>().ShouldBeEquivalentTo(new List<int> {10, 20, 30});

            oneAnotherTctx.Exception?.InnerException.Should().BeNull();
            oneAnotherTctx.GetResult<List<int>>().ShouldBeEquivalentTo(new List<int> {10, 20, 30});
        }

        [Test]
        public void IncorrectNumberOfSplitsAndCollectorsShouldThrowException()
        {
            // GIVEN
            mLogs.Logs.Clear();
            var injectionProvider = new IoCContainer();
            injectionProvider.RegisterSingle(new List<int>());
            injectionProvider.Register<TestIntSource>();
            Contracts.Dependencies.Injection.RegisterInjectionProvider(injectionProvider, false);

            PipelineBlueprint blueprintNormal = PipelineBlueprint.CreateBlueprint(typeof(int), "Normal");
            PipelineBlueprint blueprintOnlyCollect = PipelineBlueprint.CreateBlueprint(typeof(int), "OnlyCollect");
            PipelineBlueprint blueprintOnlySplit = PipelineBlueprint.CreateBlueprint(typeof(int), "OnlySplit");
            PipelineBlueprint blueprintSplitsMoreThanCollects = PipelineBlueprint.CreateBlueprint(typeof(int),
                "SplitsMoreThanCollects");
            PipelineBlueprint blueprintCollectsMoreThanSplits = PipelineBlueprint.CreateBlueprint(typeof(int),
                "CollectsMoreThanSplits");

            // WHEN
            Action createNormal = () => new Pipeline(blueprintNormal
                .Split<TestIntSource>()
                .Collect());

            Action createOnlySplit = () => new Pipeline(blueprintOnlySplit
                .Split<TestIntSource>());

            Action createOnlyCollect = () => new Pipeline(blueprintOnlyCollect
                .Collect());

            Action createSplitsMoreThanCollects = () => new Pipeline(blueprintSplitsMoreThanCollects
                .Split<TestIntSource>()
                .Split<TestIntSource>()
                .Collect());

            Action createCollectsMoreThanSplits = () => new Pipeline(blueprintCollectsMoreThanSplits
                .Split<TestIntSource>()
                .Collect()
                .Collect());


            // THAN
            createNormal.ShouldNotThrow();
            createOnlySplit.ShouldThrow<InvalidSplitCollectException>();
            createOnlyCollect.ShouldThrow<InvalidSplitCollectException>();
            createSplitsMoreThanCollects.ShouldThrow<InvalidSplitCollectException>();
            createCollectsMoreThanSplits.ShouldThrow<InvalidSplitCollectException>();
        }

        [Test]
        public void TwoTaskWithActionsShouldWorksWell()
        {
            // GIVEN
            mLogs.Logs.Clear();
            var injectionProvider = new IoCContainer();
            injectionProvider.Register<TestMultiplicatorStep>();
            JustConveyor.Contracts.Dependencies.Injection.RegisterInjectionProvider(injectionProvider, false);
            PipelineBlueprint blueprint = PipelineBlueprint.CreateBlueprint(typeof(int), "SingleApply");
            PipelineBlueprint blueprintOther = PipelineBlueprint.CreateBlueprint(typeof(int), "DoubleApply");

            // WHEN
            TransferingContext tctx = new TransferingContext();
            new Pipeline(blueprint
                .Apply<TestMultiplicatorStep>("multiplyby2")
                .Apply<TestMultiplicatorStep>("multiplyby2"))
                .Process(tctx, 1).Wait();

            TransferingContext otherTctx = new TransferingContext(new Headers {{"factor", 10}}.Dict);
            new Pipeline(blueprintOther
                .Apply<TestMultiplicatorStep>("multiplyby2")
                .Apply<TestMultiplicatorStep>("sync_mbf")
                .Apply<TestMultiplicatorStep>("multiplyby2")
                .Apply<TestMultiplicatorStep>("sync_mbf"))
                .Process(otherTctx, 1).Wait();

            // THAN
            tctx.GetResult<int>().Should().Be(4);
            otherTctx.GetResult<int>().Should().Be(400);
        }

        [Test]
        public void SplitShouldWorksWell()
        {
            // GIVEN
            mLogs.Logs.Clear();
            var intSource = new List<int> {2, 4, 6};

            var injectionProvider = new IoCContainer();
            injectionProvider.RegisterSingle(intSource);
            JustConveyor.Contracts.Dependencies.Injection.RegisterInjectionProvider(injectionProvider);

            var blueprint = PipelineBlueprint.CreateBlueprint(typeof(int), "IntMultiplicatorBlueprint")
                .Split<TestIntSource>()
                .Apply<TestMultiplicatorStep>("multiplybyfactor")
                .Collect()
                .Apply((List<UnitContext> unit) => unit.Select(el => el.GetUnit<int>()).ToList());

            // WHEN
            Pipeline pipeline = new Pipeline(blueprint);
            TransferingContext tctx = new TransferingContext(new Headers {{"factor", 10}}.Dict);
            pipeline.Process(tctx, 1).Wait();
            List<int> result = tctx.GetResult<List<int>>();

            // THAN
            result.ShouldBeEquivalentTo(new List<int> {20, 40, 60});
        }

        [Test]
        public void ExceptionShouldBeProcessedWithErrorProcessor()
        {
            // GIVEN
            var injectionProvider = new IoCContainer();
            var step = new TestIntSource(new List<int> {101});
            injectionProvider.RegisterSingle(step);
            JustConveyor.Contracts.Dependencies.Injection.RegisterInjectionProvider(injectionProvider, false);


            // WHEN
            TransferingContext tctx = new TransferingContext();
            new Pipeline(PipelineBlueprint.CreateBlueprint<int>("WithExceptionBlueprint")
                .Split<TestIntSource>()
                .Collect())
                .Process(tctx, 1).Wait();

            //// THAN
            tctx.Exception.Should().NotBeNull();
            step.ExceptionMessage.Should().Be("More than 100.");
            tctx.FinalUnitContext.Should().BeNull();
        }
    }
}