﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using JustConveyor.Contracts.Attributes;
using JustConveyor.Contracts.Dependencies.Attributes;
using JustConveyor.Contracts.Pipelining;
using JustConveyor.Contracts.Pipelining.Attributes;

namespace IntegersMultiplier
{
	[PipelineBuilder("ints-processor")]
	[Injecting]
	public class IntegersProcessorPipelineBuilder
	{
		[Blueprint]
		public PipelineDescriptor CreateIntMultPipeline()
		{
			var blueprint = PipelineBlueprint
				.CreateBlueprint<int>("TestIntMultPipeline") // Create bluprint with given name (name just for metrics display)
				.Apply((utx, tctx) =>
				{
					tctx.Meta.Add("PROCESSING INT", utx.Unit.ToString());
					return utx.Unit;
				}, "SetMeta")
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

	/// <summary>
	/// Class that contains pipeline blueprint definition and processing methods.
	/// </summary>
	[Injecting]
	public class IntegersProcessor
	{
		private readonly ConcurrentBag<int> mCollector;

		public IntegersProcessor([Inject("collector")] ConcurrentBag<int> collector)
		{
			mCollector = collector;
		}

		[Processor("multiplyby2")]
		public async Task<int> MultiplyByTwo(int unit)
		{
			var randomWaitTime = TimeSpan.FromSeconds(new Random(DateTime.Now.Millisecond).Next(20, 40));
			await Task.Delay(randomWaitTime); // simmulation of async call.
			if (unit%10 == 0)
				throw new Exception("mod 10 == 0");
			return unit*2; // just muliplying on 2
		}

		[Processor("accumulate")]
		public void Accumulate(int unit)
		{
			mCollector.Add(unit); // accumulate result in List.
		}
	}
}