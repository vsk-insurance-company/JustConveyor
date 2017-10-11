using System;

namespace JustConveyor.Contracts.Pipelining.Attributes
{
	[AttributeUsage(AttributeTargets.Method)]
	public class ProcessorAttribute : Attribute
	{
		public string Name { get; }

		public ProcessorAttribute(string name = "")
		{
			Name = name;
		}
	}
}