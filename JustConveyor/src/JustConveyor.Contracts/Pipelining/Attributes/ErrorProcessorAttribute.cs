using System;

namespace JustConveyor.Contracts.Pipelining.Attributes
{
	[AttributeUsage(AttributeTargets.Method)]
	public class ErrorProcessorAttribute : Attribute
	{
		public string Name { get; }

		public ErrorProcessorAttribute(string name = "")
		{
			Name = name;
		}
	}
}