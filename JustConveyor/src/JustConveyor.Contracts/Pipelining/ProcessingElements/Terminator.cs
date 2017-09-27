namespace JustConveyor.Contracts.Pipelining.ProcessingElements
{
	public class Terminator : ProcessingElement
	{
		public Terminator()
		{
			ProcessAction = (tctx, octx) => { tctx.FinalUnitContext = octx; };
		}

		public override string ToString()
		{
			return "T";
		}
	}
}