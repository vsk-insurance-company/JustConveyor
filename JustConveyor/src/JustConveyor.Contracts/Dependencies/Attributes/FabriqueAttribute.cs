using System;

namespace JustConveyor.Contracts.Dependencies.Attributes
{
	/// <summary>
	/// Attribute for class that should be managed via container.
	/// </summary>
	[AttributeUsage(AttributeTargets.Method)]
	public class FabriqueAttribute : Attribute
	{
		/// <summary>
		/// Allow reasign another fabrique method.
		/// </summary>
		public bool AllowReassign { get; }

		/// <summary>
		/// Name of fabrique (if not setted fabrique will be created for Type).
		/// </summary>
		public string Name { get; }

		/// <summary>
		/// CTOR.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="allowReassign"></param>
		public FabriqueAttribute(string name = null, bool allowReassign = false)
		{
			AllowReassign = allowReassign;
			Name = name;
		}
	}
}