using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JustConveyor.Contracts.Dependencies;
using JustConveyor.Contracts.Dependencies.Attributes;
using JustConveyor.Contracts.Pipelining.Attributes;
using JustConveyor.Contracts.Pipelining.Contexts;

namespace JustConveyor.Tests.TestProcessModel.Pipelines
{
	[Injecting(pattern: CreationPattern.SingleImmediatly)]
	public class TestMultiplicatorStep
	{
		[Processor("multiplybyfactor")]
		public async Task<int> ProcessAsync(TransferingContext tctx, int number)
		{
			await Task.Yield();
			return number * tctx.Get<int>("factor");
		}

		[Processor("sync_mbf")]
		public int Process(TransferingContext tctx, int number)
		{
			return number * tctx.Get<int>("factor");
		}

		[Processor("multiplyby2")]
		public async Task<int> MultiplyByTwo(int unit)
		{
			await Task.Yield();
			return unit * 2;
		}

		[Processor("strip")]
		public async Task<List<int>> Strip(IList<UnitContext> els)
		{
			await Task.Yield();
			return els.Select(el => el.GetUnit<int>()).ToList();
		}
	}
}