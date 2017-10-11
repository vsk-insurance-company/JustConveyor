using System;

namespace JustConveyor.Contracts.Pipelining.Attributes
{
	public class PipelineAttribute : Attribute
	{
		/// <summary>
		/// Name of pipeline.
		/// </summary>
		public string Name { get; }

		/// <summary>
		/// Type of seed for pipeline.
		/// </summary>
		public Type SeedType { get; }

		public PipelineAttribute(string name, Type seedType)
		{
			Name = name;
			SeedType = seedType;
		}
	}
}