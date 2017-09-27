using System;

namespace JustConveyor.Contracts.Dependencies.Attributes
{
	/// <summary>
	/// Attribute for class that should be managed via container.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class)]
	public class InjectingAttribute : Attribute
	{
		/// <summary>
		/// Name of registered type.
		/// </summary>
		public string Name { get; }

		/// <summary>
		/// Is injection allow reasign previously assigned type resolution.
		/// </summary>
		public bool AllowReassign { get; }

		/// <summary>
		/// Pattern with which class lifetime will be managed.
		/// </summary>
		public CreationPattern Pattern { get; }

		/// <summary>
		/// CTOR.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="pattern"></param>
		/// <param name="allowReassign"></param>
		public InjectingAttribute(string name = "", CreationPattern pattern = CreationPattern.CreateOnGet,
			bool allowReassign = false)
		{
			AllowReassign = allowReassign;
			Name = name;
			Pattern = pattern;
		}
	}
}