using System;
using System.Collections.Generic;
using JustConveyor.Contracts.Dependencies;
using JustConveyor.Contracts.Dependencies.Attributes;
using JustConveyor.Contracts.Pipelining.Attributes;
using JustConveyor.Contracts.Pipelining.Contexts;

namespace JustConveyor.Tests.TestProcessModel.Pipelines
{
	[Injecting(pattern: CreationPattern.SingleImmediatly)]
	public class TestIntSource
	{
		public TestIntSource(List<int> ints)
		{
			Ints = ints;
		}

		public List<int> Ints { get; }

		public string ExceptionMessage { get; set; }

		[Splitter]
		public IEnumerator<UnitContext> Generate(UnitContext ctx)
		{
			foreach (var i in Ints)
			{
				if (i > 100)
					throw new Exception("More than 100.");

				var nctx = new UnitContext(Guid.NewGuid().ToString("n"), $"id:{i}", i);
				yield return nctx;
			}
		}

		[ErrorProcessor]
		public bool ErrorProcessor(Exception ex)
		{
			ExceptionMessage = ex.Message;
			return false;
		}
	}
}