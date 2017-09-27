using System;
using System.Collections.Generic;

namespace JustConveyor.Contracts.Pipelining.Contexts
{
    /// <summary>
    /// Context of conveyor execution.
    /// </summary>
    public class TransferingContext : ContextWithHeaders
	{
		/// <summary>
		/// Id of transaction.
		/// </summary>
		public string Id { get; set; }
        
        /// <summary>
        /// Processing history.
        /// </summary>
        public Stack<ProcessingInfo> ProcessingHistory { get; } = new Stack<ProcessingInfo>();

		/// <summary>
		/// Exception occured during transfering.
		/// </summary>
		public Exception Exception { get; set; }

		/// <summary>
		/// Final state of unit context.
		/// </summary>
		public UnitContext FinalUnitContext { get; set; }

        /// <summary>
        /// Processing start timestamp.
        /// </summary>
	    public DateTime ProcessingStart { get; set; }

		public TType GetResult<TType>()
		{
			return (TType) FinalUnitContext?.Unit;
		}

		/// <summary>
		/// Конструктор для изначального контекста
		/// </summary>
		/// <param name="parameters"></param>
		public TransferingContext(IReadOnlyDictionary<string, object> parameters = null) : base(parameters)
		{
		}
	}
}