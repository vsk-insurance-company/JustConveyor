using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using JustConveyor.Contracts;
using JustConveyor.Contracts.Queues;

namespace JustConveyor.Queues
{
	public class InMemoryQueue : QueueProviderContract
	{
		class BufferedBlockUnfolder<TType> : IEnumerable<TType>
		{
			private volatile bool mClosed;
			private readonly BufferBlock<TType> mCollection;
			private readonly TimeSpan mTimeout;
			private readonly bool mAutoclose;

			public BufferedBlockUnfolder(BufferBlock<TType> collection, TimeSpan timeout, bool autoclose = true)
			{
				mAutoclose = autoclose;
				mTimeout = timeout;
				mCollection = collection;
			}

			public IEnumerator<TType> GetEnumerator()
			{
				while (true)
				{
					TType nextValue = default(TType);
					try
					{
						nextValue = mCollection.Receive(mTimeout);
					}
					catch (TimeoutException)
					{
						if (mAutoclose && mClosed && mCollection.Count == 0)
							yield break;
					}

					yield return nextValue;

					if (mAutoclose && mClosed && mCollection.Count == 0)
						yield break;
				}
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return GetEnumerator();
			}

			public void Close()
			{
				mClosed = true;
			}
		}

		private readonly BufferBlock<Package> inputQueue = new BufferBlock<Package>();
		private readonly BufferedBlockUnfolder<Package> mNumerableQueue;
		private readonly TimeSpan? mTimeout;
		private readonly bool mAutoclose;
		private volatile bool mClosed;

		public InMemoryQueue(TimeSpan? timeout = null, bool autoclose = true)
		{
			mAutoclose = autoclose;
			mTimeout = timeout;
			mNumerableQueue = new BufferedBlockUnfolder<Package>(inputQueue, timeout ?? TimeSpan.FromSeconds(10), autoclose);
		}

		public IEnumerable<Package> GetListener()
		{
			return mNumerableQueue;
		}

		public async Task<Package> ReceiveNextMessageAsync()
		{
			try
			{
				return await inputQueue.ReceiveAsync(mTimeout ?? TimeSpan.FromSeconds(1));
			}
			catch (TimeoutException)
			{
				if (mAutoclose && mClosed && inputQueue.Count == 0)
					return null;
				return Package.Fake;
			}
		}

		public Package ReceiveNextMessage()
		{
			throw new NotImplementedException();
		}

		public Package GetNextMessage()
		{
			return mNumerableQueue.FirstOrDefault();
		}

		public int Count => inputQueue.Count;

		public void Publish(Package message)
		{
			if (message == null) throw new ArgumentNullException(nameof(message));
			inputQueue.Post(message);
		}

		public void Close()
		{
			mClosed = true;
			mNumerableQueue.Close();
		}
	}
}