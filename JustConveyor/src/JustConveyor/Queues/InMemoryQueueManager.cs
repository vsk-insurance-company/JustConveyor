using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JustConveyor.Contracts.Queues;

namespace JustConveyor.Queues
{
	// TODO types and names restictions.
	// TODO process case when queue closed.
	// TODO timeout from settings
	public class InMemoryQueueManager : QueuesManagerContract
	{
		private readonly Dictionary<string, InMemoryQueue> mQueues =
			new Dictionary<string, InMemoryQueue>();

		private readonly object mQueuesLock = new object();
		private readonly TimeSpan? mDefaultTimeout;
		private readonly bool mAutoclose;

		public InMemoryQueueManager(TimeSpan? defaultTimeout = null, bool autoclose = true)
		{
			mAutoclose = autoclose;
			mDefaultTimeout = defaultTimeout;
		}

		public QueueProviderContract CreateQueue(string name)
		{
			var newQueue = new InMemoryQueue(mDefaultTimeout, mAutoclose);
			lock (mQueuesLock)
				mQueues.Add(name, newQueue);
			return newQueue;
		}

		public void AddQueueProvider(string name, QueueProviderContract provider, bool replace = false)
		{
			lock (mQueuesLock)
				mQueues.Add(name, (InMemoryQueue) provider);
		}

		public QueueProviderContract GetQueue(string name)
		{
			lock (mQueuesLock)
				return mQueues[name];
		}

		public void Close()
		{
			Task.Run(async () =>
			{
				while (mQueues.Values.Any(el => el.Count != 0))
					await Task.Delay(TimeSpan.FromSeconds(10));

				lock (mQueuesLock)
				{
					foreach (var inMemoryQueue in mQueues)
						inMemoryQueue.Value.Close();
				}
			});
		}
	}
}