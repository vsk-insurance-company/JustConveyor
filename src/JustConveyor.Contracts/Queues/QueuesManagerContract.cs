namespace JustConveyor.Contracts.Queues
{
	/// <summary>
	/// Contract for queue manager.
	/// </summary>
	public interface QueuesManagerContract
	{
		/// <summary>
		/// Create queue.
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		QueueProviderContract CreateQueue(string name);

		/// <summary>
		/// Add queue provider with name.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="provider"></param>
		/// <param name="replace"></param>
		void AddQueueProvider(string name, QueueProviderContract provider, bool replace = false);

		/// <summary>
		/// Get queue manager by name.
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		QueueProviderContract GetQueue(string name);

		/// <summary>
		/// Close all queues.
		/// </summary>
		void Close();
	}
}