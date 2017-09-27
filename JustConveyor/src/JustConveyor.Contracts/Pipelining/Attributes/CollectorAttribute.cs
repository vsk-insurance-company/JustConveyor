using System;

namespace JustConveyor.Contracts.Pipelining.Attributes
{
	[AttributeUsage(AttributeTargets.Method)]
	public class CollectorAttribute : Attribute
	{
		/// <summary>
		/// Name of folder for selecting.
		/// </summary>
		public string Name { get; }

		public CollectorAttribute(string name = "")
		{
			Name = name;
		}
	}
}