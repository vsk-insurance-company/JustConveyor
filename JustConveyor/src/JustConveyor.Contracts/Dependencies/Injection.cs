using System;
using JustConveyor.Contracts.Exceptions;
using NLog;

namespace JustConveyor.Contracts.Dependencies
{
	/// <summary>
	/// Static class for manpulating with injection.
	/// </summary>
	public static class Injection
	{
		private static InjectionProviderContract mInjectionProvider;
		private static ILogger mLogger;

		/// <summary>
		/// Get injection provider.
		/// </summary>
		public static InjectionProviderContract InjectionProvider
		{
			get
			{
				if (mInjectionProvider == null)
					throw new InjectionProviderNotReadyException();
				return mInjectionProvider;
			}
		}

		/// <summary>
		/// Register logger for injector.
		/// </summary>
		/// <param name="logger"></param>
		public static void RegisterLogger(ILogger logger)
		{
			mLogger = logger;
		}

		/// <summary>
		/// Register injection provider.
		/// </summary>
		/// <param name="injectionProvider">Registering provider.</param>
		/// <param name="scanAssemblies">Is assmeblies should be scaned for managing objects.</param>
		public static void RegisterInjectionProvider(InjectionProviderContract injectionProvider, bool scanAssemblies = true)
		{
			if (injectionProvider == null) throw new ArgumentNullException(nameof(injectionProvider));

			mLogger?.Trace($"Registering {injectionProvider.GetType().AssemblyQualifiedName} as injection provider.");
			mInjectionProvider = injectionProvider;

			if (mLogger != null)
				mInjectionProvider.SetLogger(mLogger);

			if (scanAssemblies)
				mInjectionProvider.ScanAssemblies();
		}
	}
}