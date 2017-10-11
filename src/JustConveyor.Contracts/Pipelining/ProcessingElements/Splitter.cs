namespace JustConveyor.Contracts.Pipelining.ProcessingElements
{
	public class Splitter : ProcessingElement
	{
		public Collector ConnectedCollector { get; set; }

		public override ProcessingElement Clone(ProcessingElement previous)
		{
			var np = new Splitter
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
			return $"S:{(IsAsync ? "A" : "S")}";
		}
	}
}