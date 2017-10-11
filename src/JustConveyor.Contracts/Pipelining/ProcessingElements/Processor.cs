namespace JustConveyor.Contracts.Pipelining.ProcessingElements
{
	public class Processor : ProcessingElement
	{
		public override ProcessingElement Clone(ProcessingElement previous)
		{
			var np = new Processor
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
			return $"P:{(IsAsync ? "A" : "S")}";
		}
	}
}