using System;

namespace JustConveyor.Contracts.Attributes
{
	/// <summary>
	/// Attribute that defines classes with pipeline building logic.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class)]
	public class PipelineBuilderAttribute : Attribute
	{
		/// <summary>
		/// Name of pipeline builder.
		/// </summary>
		public string Name { get; }

		/// <summary>
		/// CTOR.
		/// </summary>
		/// <param name="name"></param>
		public PipelineBuilderAttribute(string name)
		{
			Name = name;
		}
	}
}