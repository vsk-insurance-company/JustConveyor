using System;
using System.Collections.Generic;
using JustConveyor.Contracts.Exceptions;

namespace JustConveyor.Contracts.Pipelining.Contexts
{
	/// <summary>
	/// Context of processing unit.
	/// </summary>
	public class UnitContext : ContextWithHeaders
	{
		public UnitContext()
		{
		}

		public UnitContext(string processingId, string unitid, object unit,
			IReadOnlyDictionary<string, object> parameters = null)
			: base(parameters)
		{
			UnitId = unitid;
			ProcessingId = processingId;
			Unit = unit;
		}

		/// <summary>
		/// Unique id of unit processing sequence.
		/// </summary>
		public string ProcessingId { get; set; }

		/// <summary>
		/// Id of processing id.
		/// </summary>
		public string UnitId { get; set; }

		/// <summary>
		/// Processing unit itself.
		/// </summary>
		public object Unit { get; set; }

		/// <summary>
		/// Exception occured during processing.
		/// </summary>
		public Exception Exception { get; set; }

		/// <summary>
		/// Get unit casted to TType.
		/// </summary>
		/// <typeparam name="TType"></typeparam>
		/// <returns></returns>
		public TType GetUnit<TType>()
		{
			if (!typeof(TType).IsAssignableFrom(Unit.GetType()))
				throw new UnitTypeMismatch(UnitId, Unit.GetType(), typeof(TType));
			return (TType) Unit;
		}

		/// <summary>
		/// Check if Unit can be casted to given type.
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public bool IsUnitAssignableTo(Type type)
		{
			return type.IsInstanceOfType(Unit);
		}

		/// <summary>
		/// Get original type of Unit.
		/// </summary>
		/// <returns></returns>
		public Type GetUnitType()
		{
			return Unit.GetType();
		}

		public override string ToString()
		{
			return Unit.ToString();
		}
	}
}