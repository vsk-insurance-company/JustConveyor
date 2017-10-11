using System;
using NLog;

namespace JustConveyor.Contracts.Dependencies
{
	/// <summary>
	/// Contract for injection providers
	/// </summary>
	public interface InjectionProviderContract
	{
		/// <summary>
		/// Register single object.
		/// </summary>
		/// <param name="obj"></param>
		/// <param name="reasign">Flag that indicates that if type already registed it should be replaced.</param>
		/// <typeparam name="TDep"></typeparam>
		void RegisterSingle<TDep>(TDep obj, bool reasign = false);

		/// <summary>
		/// Register single object.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="obj"></param>
		/// <param name="reasign">Flag that indicates that if type already registed it should be replaced.</param>
		void RegisterSingle(string name, object obj, bool reasign = false);

		/// <summary>
		/// Register by type.
		/// </summary>
		/// <param name="pattern"></param>
		/// <param name="reasign">Flag that indicates that if type already registed it should be replaced.</param>
		/// <typeparam name="TDep"></typeparam>
		void Register<TDep>(CreationPattern pattern, bool reasign = false) where TDep : class;

		/// <summary>
		/// Register by type.
		/// </summary>
		/// <param name="pattern"></param>
		/// <param name="reasign">Flag that indicates that if type already registed it should be replaced.</param>
		/// <param name="name"></param>
		/// <typeparam name="TDep"></typeparam>
		void Register<TDep>(string name, CreationPattern pattern, bool reasign = false) where TDep : class;

		/// <summary>
		/// Register by type.
		/// </summary>
		/// <param name="type"></param>
		/// <param name="pattern"></param>
		/// <param name="reasign">Flag that indicates that if type already registed it should be replaced.</param>
		/// <param name="name"></param>
		void Register(string name, Type type, CreationPattern pattern, bool reasign = false);

		/// <summary>
		/// Register fabrique of objects by type.
		/// </summary>
		/// <param name="type"></param>
		/// <param name="fabriqueFunction"></param>
		/// <param name="reasign">Flag that indicates that if type already registed it should be replaced.</param>
		void RegisterFabrique(Type type, Func<object> fabriqueFunction, bool reasign = false);

		/// <summary>
		/// Register fabrique of objects.
		/// </summary>
		/// <param name="fabriqueFunction"></param>
		/// <param name="reasign">Flag that indicates that if type already registed it should be replaced.</param>
		/// <typeparam name="TDep"></typeparam>
		void RegisterFabrique<TDep>(Func<object> fabriqueFunction, bool reasign = false);

		/// <summary>
		/// Register fabrique of objects.
		/// </summary>
		/// <param name="name">Name of fabrique.</param>
		/// <param name="fabriqueFunction">Function of object creation.</param>
		/// <param name="reasign">Flag that indicates that if type already registed it should be replaced.</param>
		void RegisterFabrique(string name, Func<object> fabriqueFunction, bool reasign = false);

		/// <summary>
		/// Register object resolver.
		/// </summary>
		/// <param name="reasign">Flag that indicates that if type already registed it should be replaced.</param>
		/// <typeparam name="TDep"></typeparam>
		/// <param name="resolver"></param>
		void RegisterResolver<TDep>(Func<dynamic, object> resolver, bool reasign = false);

		/// <summary>
		/// Register object resolver.
		/// </summary>
		/// <typeparam name="TDep"></typeparam>
		/// <param name="name">Name of resolver.</param>
		/// <param name="resolver"></param>
		/// <param name="reasign">Flag that indicates that if type already registed it should be replaced.</param>
		void RegisterResolver(string name, Func<dynamic, object> resolver, bool reasign = false);
		
		/// <summary>
		/// Get object by name.
		/// </summary>
		/// <param name="name"></param>
		/// <typeparam name="TDep"></typeparam>
		/// <returns></returns>
		TDep Get<TDep>(string name) where TDep : class;

		/// <summary>
		/// Get object by name.
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		object Get(string name);

		/// <summary>
		/// Get object by type.
		/// </summary>
		/// <typeparam name="TDep"></typeparam>
		/// <returns></returns>
		TDep Get<TDep>() where TDep : class;

		/// <summary>
		/// Get object by type.
		/// </summary>
		/// <returns></returns>
		object Get(Type type);
		
		/// <summary>
		/// Get object by resolving arguments
		/// </summary>
		/// <typeparam name="TDep"></typeparam>
		/// <param name="resolvingArguments"></param>
		/// <returns></returns>
		TDep Resolve<TDep>(dynamic resolvingArguments) where TDep : class;

		/// <summary>
		/// Get object by resolving arguments
		/// </summary>
		/// <typeparam name="TDep"></typeparam>
		/// <param name="resolverName"></param>
		/// <param name="resolvingArguments"></param>
		/// <returns></returns>
		TDep Resolve<TDep>(string resolverName, dynamic resolvingArguments) where TDep : class;

		/// <summary>
		/// Scan assemblies for objects that should be managed via container.
		/// </summary>
		void ScanAssemblies();

		/// <summary>
		/// Set logger for container.
		/// </summary>
		void SetLogger(ILogger logger);
	}
}