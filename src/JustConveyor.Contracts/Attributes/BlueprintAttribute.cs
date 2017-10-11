using System;

namespace JustConveyor.Contracts.Attributes
{
	/// <summary>
	/// Attribute that define methods of PipelineBuilder that gives blueprints.
	/// </summary>
	[AttributeUsage(AttributeTargets.Method)]
	public class BlueprintAttribute : Attribute
	{
	}
}