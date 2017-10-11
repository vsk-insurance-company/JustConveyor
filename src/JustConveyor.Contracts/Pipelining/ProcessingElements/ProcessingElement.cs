using System;
using System.Reflection;
using System.Threading.Tasks;
using JustConveyor.Contracts.Pipelining.Contexts;

namespace JustConveyor.Contracts.Pipelining.ProcessingElements
{
	public class ProcessingElement
	{
		// TODO Use processing element names from pipeline building
		public string Name { get; set; }

		/// <summary>
		/// Function for processing error. 
		/// Returns true when processing should keep working.
		/// </summary>
		public MethodInfo ErrorProcessorMethod { get; set; }

		/// <summary>
		/// Next processing element of current.
		/// </summary>
		public ProcessingElement Next { get; set; }

		/// <summary>
		/// Previous processing element of current.
		/// </summary>
		public ProcessingElement Previous { get; set; }

		/// <summary>
		/// Method that will do job.
		/// </summary>
		public MethodInfo Method { get; set; }

		/// <summary>
		/// Processing element carrier type.
		/// </summary>
		public Type Carrier { get; set; }

		/// <summary>
		/// Is processing element async.
		/// </summary>
		public bool IsAsync { get; set; }

		/// <summary>
		/// For case when invoking object already known (like lambdas).
		/// </summary>
		public object ConcreteObject { get; set; }

		/// <summary>
		/// Sync Processor.
		/// </summary>
		public Action<TransferingContext, UnitContext> ProcessAction { get; set; }

		/// <summary>
		/// Async Processor.
		/// </summary>
		public Func<TransferingContext, UnitContext, Task> ProcessTask { get; set; }

		/// <summary>
		/// Find next async processor in chain.
		/// </summary>
		/// <param name="el"></param>
		/// <returns></returns>
		public static ProcessingElement NextAsyncProcessor(ProcessingElement el)
		{
			if (el == null || el is Collector)
				return null;

			return el.IsAsync ? el : NextAsyncProcessor(el.Next);
		}

		public ProcessingElement GetTail()
		{
			return Next == null ? this : Next.GetTail();
		}

		public virtual ProcessingElement Clone(ProcessingElement previous)
		{
			var cloned = new ProcessingElement
			{
				Name = Name,
				IsAsync = IsAsync,
				ErrorProcessorMethod = ErrorProcessorMethod,
				Carrier = Carrier,
				ConcreteObject = ConcreteObject,
				Method = ErrorProcessorMethod,
				Previous = previous
			};

			cloned.Next = Next?.Clone(cloned);

			return cloned;
		}
	}
}