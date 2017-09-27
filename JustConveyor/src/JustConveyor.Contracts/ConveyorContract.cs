using System;
using System.Threading.Tasks;
using JustConveyor.Contracts.Pipelining;
using JustConveyor.Contracts.Pipelining.Contexts;
using JustConveyor.Contracts.Settings;

namespace JustConveyor.Contracts
{
	/// <summary>
	/// Contract for the Conveyor.
	/// </summary>
	public interface ConveyorContract
	{
		/// <summary>
		/// Process package in pipeline.
		/// </summary>
		/// <param name="request"></param>
		/// <param name="pipelineName"></param>
		/// <returns></returns>
		Task<Package> Process(Package request, string pipelineName = "");

		/// <summary>
		/// Register pipeline blueprint in conveyor.
		/// </summary>
		/// <param name="descriptor"></param>
		/// <returns></returns>
		ConveyorContract WithBlueprint(PipelineDescriptor descriptor);

        /// <summary>
        /// Start metrics service with given settings.
        /// </summary>
        /// <param name="service"></param>
        /// <returns></returns>
	    ConveyorContract WithMetricsService(ServiceSettings service = null);

        /// <summary>
        /// Register conveyr suupplier.
        /// </summary>
        /// <param name="supplierName"></param>
        /// <param name="supplier"></param>
        /// <returns></returns>
        ConveyorContract WithSupplier(string supplierName, ConveySupplierContract supplier);

        /// <summary>
        /// Register finalizer for given pipeline process.
        /// </summary>
        /// <param name="pipelineName"></param>
        /// <param name="finalizer"></param>
        /// <returns></returns>
	    ConveyorContract WithPipelineFinalizer(string pipelineName, Action<Package, TransferingContext> finalizer);

        /// <summary>
        /// Register finalizer for all pipelines.
        /// </summary>
        /// <param name="finalizer"></param>
        /// <returns></returns>
	    ConveyorContract WithFinalizer(Action<Package, TransferingContext> finalizer);

        /// <summary>
        /// Register lost package processor.
        /// </summary>
        /// <param name="lostPackageProcessor"></param>
        /// <returns></returns>
	    ConveyorContract WithLostPackagesProcessor(Action<Package> lostPackageProcessor);

		/// <summary>
		/// Start conveying process.
		/// </summary>
		/// <returns></returns>
		void Start();
        
        /// <summary>
		/// Start conveying process.
		/// </summary>
		/// <returns></returns>
		void Stop();
	}
}