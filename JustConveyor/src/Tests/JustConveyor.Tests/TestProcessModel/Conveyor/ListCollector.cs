using System.Collections.Concurrent;
using System.Collections.Generic;
using JustConveyor.Contracts.Pipelining.Attributes;
using JustConveyor.Contracts.Pipelining.Contexts;

namespace JustConveyor.Tests.TestProcessModel.Conveyor
{
	public class ListCollector
	{
		internal ConcurrentDictionary<string, List<int>> Lists = new ConcurrentDictionary<string, List<int>>();

		[Processor]
		public bool Collect(UnitContext ctx, List<int> list)
		{
			Lists.TryAdd(ctx.UnitId, list);
			return true;
		}
	}
}