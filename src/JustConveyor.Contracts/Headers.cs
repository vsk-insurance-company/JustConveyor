using System.Collections;
using System.Collections.Generic;

namespace JustConveyor.Contracts
{
	/// <summary>
	/// Headers collection
	/// </summary>
	public class Headers : IEnumerable<KeyValuePair<string, object>>
	{
		public Headers(IReadOnlyDictionary<string, object> headers = null)
		{
			if (headers != null)
				foreach (var header in headers)
					Dict.Add(header.Key, header.Value);
		}

		public Dictionary<string, object> Dict = new Dictionary<string, object>();

		public void Add(string headerName, object headerValue)
		{
			Dict.Add(headerName, headerValue);
		}

		public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
		{
			return Dict.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}