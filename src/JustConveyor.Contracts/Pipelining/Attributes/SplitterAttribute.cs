using System;

namespace JustConveyor.Contracts.Pipelining.Attributes
{
	[AttributeUsage(AttributeTargets.Method)]
	public class SplitterAttribute : Attribute
	{
		public string Name { get; }

		public SplitterAttribute(string name = "")
		{
			Name = name;
		}
	}
}