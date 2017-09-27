using System;
using System.Collections.Generic;

namespace JustConveyor.Contracts
{
    /// <summary>
	/// Container for supply channel.
	/// </summary>
	public class Package
	{
        /// <summary>
        /// Time profile of package processing.
        /// </summary>
        internal PackageProcessProfile Profile { get; set; } = new PackageProcessProfile();

        /// <summary>
        /// Label of package.
        /// </summary>
        public string Label { get; set; }

		/// <summary>
		/// Id for internal delivery.
		/// </summary>
		public Guid? ExternalDeliverBoxId { get; set; }
		
		/// <summary>
		/// Label of package.
		/// </summary>
		public string Id { get; set; }

		/// <summary>
		/// Headers of package.
		/// </summary>
		public Dictionary<string, object> Headers { get; } = new Dictionary<string, object>();

		/// <summary>
		/// Type of load.
		/// </summary>
		public Type LoadType => Load?.GetType();

		/// <summary>
		/// Load of package.
		/// </summary>
		public object Load { get; set; }

		/// <summary>
		/// Fake package returned if queue is empty.
		/// </summary>
		public static Package Fake = new Package
		{
		    Id = "FAKEPACKAGE"
		};
	}
}