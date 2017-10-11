using System;
using System.Threading.Tasks;
using JustConveyor.Contracts;
using Nito.AsyncEx;

namespace JustConveyor.Utils
{
	internal class WaitBox
	{
		private readonly AsyncManualResetEvent mEvent = new AsyncManualResetEvent();
		private Exception Error { get; set; }
		private object Result { get; set; }

		public void SetValue(object result, Exception exception = null)
		{
			Error = exception;
			Result = result;

			mEvent.Set();
		}

		public async Task<Package> WaitTask()
		{
			await mEvent.WaitAsync();

			if (Error != null)
				throw Error;

			return new Package { Load =  Result };
		}
	}
}