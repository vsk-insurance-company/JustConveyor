using System.Collections.Generic;
using System.Linq;
using JustConveyor.Contracts.Attributes;
using JustConveyor.Contracts.Pipelining;
using JustConveyor.Contracts.Pipelining.Contexts;
using JustConveyor.Tests.TestProcessModel.Pipelines;

namespace JustConveyor.Tests.TestProcessModel.Conveyor
{
	[PipelineBuilder("tests")]
	public class TestPipelinesBuilder
	{
		[Blueprint]
		public PipelineDescriptor CreateIntPipeline()
		{
			var blueprint = PipelineBlueprint
				.CreateBlueprint<int>("TestPipeline")
				.Split<TestIntSource>()
				.Apply<TestMultiplicatorStep>("multiplyby2")
				.Collect()
				.Apply((IList<UnitContext> ctx) => ctx.Select(el => el.GetUnit<int>()).ToList())
				.Apply<ListCollector>();

			return new PipelineDescriptor
			{
				Blueprint = blueprint,
				ConcurrentLinesNumber = 70,
				ForType = true
			};
		}

		[Blueprint]
		public PipelineDescriptor CreateIntMultPipeline()
		{
			var blueprint = PipelineBlueprint
				.CreateBlueprint<int>("TestIntMultPipeline")
				.Apply<TestMultiplicatorStep>("multiplyby2");

			return new PipelineDescriptor
			{
				Blueprint = blueprint,
				ConcurrentLinesNumber = 10
			};
		}

		[Blueprint]
		public PipelineDescriptor CreatePipelineWithCallOfAnother()
		{
			var blueprint = PipelineBlueprint
				.CreateBlueprint<int>("TestPipelineWithCallOfAnother")
				.Split<TestIntSource>()
				.CallPipeline("TestIntMultPipeline")
				.Collect()
				.Apply((IList<UnitContext> ctx) => ctx.Select(el => el.GetUnit<int>()).ToList())
				.Apply<ListCollector>();

			return new PipelineDescriptor
			{
				Blueprint = blueprint,
				ConcurrentLinesNumber = 70
			};
		}
	}
}