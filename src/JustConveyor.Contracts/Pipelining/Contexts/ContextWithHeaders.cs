using System;
using System.Collections;
using System.Collections.Generic;
using JustConveyor.Contracts.Exceptions;

namespace JustConveyor.Contracts.Pipelining.Contexts
{
	/// <summary>
	/// Контекст, в который можно заинжектить параметры
	/// </summary>
	public abstract class ContextWithHeaders : IEnumerable<string>
	{
		private readonly Dictionary<string, object> mHeaders = new Dictionary<string, object>();

		protected ContextWithHeaders()
		{
		}

		/// <summary>
		/// Конструктор для изначального контекста
		/// </summary>
		/// <param name="headers"></param>
		protected ContextWithHeaders(IReadOnlyDictionary<string, object> headers)
		{
			if (headers != null)
				foreach (var header in headers)
					mHeaders.Add(header.Key, header.Value);
		}

		/// <summary>
		/// Внутренняя инжекция параметров.
		/// </summary>
		/// <param name="parameters"></param>
		internal void InjectHeaders(IReadOnlyDictionary<string, object> parameters)
		{
			if (parameters == null) throw new ArgumentNullException(nameof(parameters));
			foreach (var keyValuePair in parameters)
				mHeaders.Add(keyValuePair.Key, keyValuePair.Value);
		}

		/// <summary>
		/// Внутренняя инжекция параметров.
		/// </summary>
		/// <param name="parameters"></param>
		internal void ChangeParameters(IReadOnlyDictionary<string, object> parameters)
		{
			if (parameters == null) throw new ArgumentNullException(nameof(parameters));
			mHeaders.Clear();

			foreach (var pair in parameters)
				mHeaders.Add(pair.Key, pair.Value);
		}

		/// <summary>
		/// Передаваемые по контексту параметры.
		/// </summary>
		public IReadOnlyDictionary<string, object> Headers => mHeaders;

		/// <summary>
		/// Получение объекта из контекста по его типу.
		/// </summary>
		/// <typeparam name="TType">Тип, по которому был зарегистрирован объект в контексте.</typeparam>
		/// <returns></returns>
		public TType Get<TType>(string headerName)
		{
			if (headerName == null) throw new ArgumentNullException(nameof(headerName));
			var type = typeof(TType);
			if (!Headers.ContainsKey(headerName))
				throw new HeaderNotRegisteredInContext(headerName);

			var value = Headers[headerName];
			if (!value.GetType().IsAssignableFrom(type))
				throw new HeaderTypeMismatch(headerName, value.GetType(), type);

			return (TType) value;
		}

		public void Add(string headerName, object headerValue)
		{
			if (headerName == null) throw new ArgumentNullException(nameof(headerName));
			if (mHeaders.ContainsKey(headerName))
				throw new HeaderAlreadyRegisteredInContext(headerName);

			mHeaders.Add(headerName, headerValue);
		}

		/// <summary>
		/// Additional meta that can be displayed in metrics service.
		/// </summary>
		public Dictionary<string, string> Meta { get; } = new Dictionary<string, string>();

		/// <summary>
		/// Добавление типа объекта в контекст.
		/// </summary>
		/// <param name="headerName"></param>
		/// <param name="headerValue"></param>
		public void Set(string headerName, object headerValue)
		{
			if (headerName == null) throw new ArgumentNullException(nameof(headerName));

			if (mHeaders.ContainsKey(headerName))
				mHeaders[headerName] = headerValue;
			else
				mHeaders.Add(headerName, headerValue);
		}

		public IEnumerator<string> GetEnumerator()
		{
			return mHeaders.Keys.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}