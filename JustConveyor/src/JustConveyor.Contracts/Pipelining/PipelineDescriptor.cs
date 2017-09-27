using System;

namespace JustConveyor.Contracts.Pipelining
{
	public class PipelineDescriptor
	{
        /// <summary>
        /// Type that supplies blueprint.
        /// </summary>
	    public Type BuilderType { get; set; }

		/// <summary>
		/// Number of concurent lines of pipeline.
		/// </summary>
		public uint ConcurrentLinesNumber { get; set; } = 1;

		/// <summary>
		/// Pipeline it self
		/// </summary>
		public PipelineBlueprint Blueprint { get; set; }

		/// <summary>
		/// Is pipeline should be routed by type.
		/// </summary>
		public bool ForType { get; set; }
	}
}