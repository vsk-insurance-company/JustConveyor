using System;

namespace JustConveyor.Contracts.Dependencies.Attributes
{
	/// <summary>
	/// Attribute
	/// </summary>
	[AttributeUsage(AttributeTargets.Parameter)]
	public class InjectAttribute : Attribute
	{
		/// <summary>
		/// Name of injecting object
		/// </summary>
		public string InjectingObjectName { get; }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="injectingObjectName"></param>
		public InjectAttribute(string injectingObjectName)
		{
			InjectingObjectName = injectingObjectName;
		}
	}
}