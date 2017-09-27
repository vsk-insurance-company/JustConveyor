namespace JustConveyor.Contracts.Pipelining.ProcessingElements
{
	public class Initiator : ProcessingElement
	{
		public Initiator()
		{
			ProcessTask = async (tctx, octx) =>
			{
				Next?.ProcessAction?.Invoke(tctx, octx);
				var nextAsyncProcessor = NextAsyncProcessor(Next);
				var processTask = nextAsyncProcessor?.ProcessTask(tctx, octx);
				if (processTask != null)
					await processTask;
			};
		}

		public override string ToString()
		{
			return "I";
		}
	}
}