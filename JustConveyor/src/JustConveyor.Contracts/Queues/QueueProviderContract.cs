using System.Collections.Generic;
using System.Threading.Tasks;

namespace JustConveyor.Contracts.Queues
{
	// TODO backpressure

	/// <summary>
	/// Contract for queue provider.
	/// </summary>
	public interface QueueProviderContract
	{
		/// <summary>
		/// Get messages listener.
		/// </summary>
		/// <returns></returns>
		IEnumerable<Package> GetListener();

		/// <summary>
		/// Receive next package.
		/// </summary>
		/// <returns></returns>
		Task<Package> ReceiveNextMessageAsync();

		/// <summary>
		/// Get next message in queue.
		/// </summary>
		/// <returns></returns>
		Package ReceiveNextMessage();

		/// <summary>
		/// Publish message on queue.
		/// </summary>
		/// <param name="message"></param>
		void Publish(Package message);

        /// <summary>
        /// Count of elements on queue.
        /// </summary>
        int Count { get; }

        /// <summary>
        /// Close queue
        /// </summary>
        void Close();
	}
}