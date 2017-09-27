namespace JustConveyor.Contracts.Dependencies
{
	/// <summary>
	/// Lifetime of object
	/// </summary>
	public enum CreationPattern
	{
		/// <summary>
		/// Only single object will be create
		/// </summary>
		SingleImmediatly,

		/// <summary>
		/// Only single object will be created on first call
		/// </summary>
		SingleOnCall,

		/// <summary>
		/// New object will be created on get.
		/// </summary>
		CreateOnGet
	}
}