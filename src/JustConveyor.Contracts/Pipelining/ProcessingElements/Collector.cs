using System;
using System.Collections.Generic;
using JustConveyor.Contracts.Pipelining.Contexts;

namespace JustConveyor.Contracts.Pipelining.ProcessingElements
{
	public class Collector : ProcessingElement
	{
		public Action<TransferingContext, UnitContext, Dictionary<string, IList<UnitContext>>> CollectAction
		{
			get;
			set;
		}

		public override ProcessingElement Clone(ProcessingElement previous)
		{
			var np = new Collector
			{
				Carrier = Carrier,
				ConcreteObject = ConcreteObject,
				ErrorProcessorMethod = ErrorProcessorMethod,
				IsAsync = IsAsync,
				Method = Method,
				Name = Name,
				Previous = previous
			};
			np.Next = Next?.Clone(np);
			return np;
		}

		public override string ToString()
		{
			return "C";
		}
	}
}