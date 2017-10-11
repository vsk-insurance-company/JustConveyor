using System;
using System.Collections.Generic;
using System.Linq;
using JustConveyor.Contracts.Dependencies;
using JustConveyor.Contracts.Dependencies.Attributes;
using JustConveyor.Contracts.Exceptions;
using NLog;

namespace JustConveyor.Injection
{
	public class IoCContainer : InjectionProviderContract
	{
		private readonly Dictionary<Type, Lazy<object>> mInstances =
			new Dictionary<Type, Lazy<object>>();

		private readonly Dictionary<string, Lazy<object>> mNamedInstances =
			new Dictionary<string, Lazy<object>>();

		private readonly Dictionary<Type, Func<object>> mFabriques = new Dictionary<Type, Func<object>>();
		private readonly Dictionary<string, Func<object>> mNamedFabriques = new Dictionary<string, Func<object>>();

		private readonly Dictionary<Type, Func<dynamic, object>> mResolvers = new Dictionary<Type, Func<dynamic, object>>();

		private readonly Dictionary<string, Func<dynamic, object>> mNamedResolvers =
			new Dictionary<string, Func<dynamic, object>>();

		private ILogger mLogger;

		public void RegisterSingle<TDep>(TDep obj, bool reassign = false)
		{
			if (obj == null) throw new ArgumentNullException(nameof(obj), "Unable to register 'null' value");

			var type = typeof(TDep);
			if (!reassign && (mInstances.ContainsKey(type) || mFabriques.ContainsKey(type)))
				throw new TypeAlreadyRegisteredException(type);

			if (!mInstances.ContainsKey(type))
			{
				mInstances.Add(type, new Lazy<object>(() => obj));
				mLogger?.Trace($"Type '{type.AssemblyQualifiedName}' registered.");
			}
			else
			{
				mInstances[type] = new Lazy<object>(() => obj);
				mLogger?.Trace($"Type '{type.AssemblyQualifiedName}' reassigned.");
			}
		}

		public void RegisterSingle(string name, object obj, bool reassign = false)
		{
			if (obj == null) throw new ArgumentNullException(nameof(obj), "Unable to register 'null' value");

			if (!reassign && (mNamedInstances.ContainsKey(name) || mNamedFabriques.ContainsKey(name)))
				throw new TypeAlreadyRegisteredException(name);

			if (!mNamedInstances.ContainsKey(name))
			{
				mNamedInstances.Add(name, new Lazy<object>(() => obj));
				mLogger?.Trace($"Type '{obj.GetType().AssemblyQualifiedName}' with name '{name}' registered.");
			}
			else
			{
				mNamedInstances[name] = new Lazy<object>(() => obj);
				mLogger?.Trace($"Type '{obj.GetType().AssemblyQualifiedName}' with name '{name}' reassigned.");
			}
		}

		private void Register(Type t, Action<Lazy<object>> adding, Action createFabrique, CreationPattern pattern)
		{
			if (t.GetConstructors().Length > 1)
				throw new IncorrectContructorException(t, "Type has more than one constructor.");

			if (pattern == CreationPattern.CreateOnGet)
			{
				createFabrique();
			}
			else
			{
				Lazy<object> objInit;
				if (pattern == CreationPattern.SingleImmediatly)
				{
					var obj = CreateObject(t);
					objInit = new Lazy<object>(() => obj);
				}
				else
				{
					objInit = new Lazy<object>(() => CreateObject(t));
				}
				adding(objInit);
			}
		}

		public void Register<TDep>(CreationPattern pattern = CreationPattern.CreateOnGet, bool reassign = false)
			where TDep : class
		{
			var type = typeof(TDep);
			if (!reassign && (mInstances.ContainsKey(type) || mFabriques.ContainsKey(type)))
				throw new TypeAlreadyRegisteredException(type);

			Type t = typeof(TDep);
			if (t.GetConstructors().Length > 1)
				throw new IncorrectContructorException(t, "Type has more than one constructor.");

			Register(t, obj =>
			{
				if (mInstances.ContainsKey(type))
					mInstances[type] = obj;
				else
					mInstances.Add(type, obj);
			}, () =>
			{
				if (mFabriques.ContainsKey(type))
					mFabriques[type] = CreateObject<TDep>;
				else
					mFabriques.Add(type, CreateObject<TDep>);
			},
				pattern);

			mLogger?.Trace($"Type '{type.AssemblyQualifiedName}' registered with pattern '{pattern}'.");
		}

		public void Register(Type type, CreationPattern pattern = CreationPattern.CreateOnGet, bool reassign = false)
		{
			if (!reassign && (mInstances.ContainsKey(type) || mFabriques.ContainsKey(type)))
				throw new TypeAlreadyRegisteredException(type);

			if (type.GetConstructors().Length > 1)
				throw new IncorrectContructorException(type, "Type has more than one constructor.");

			Register(type, obj =>
			{
				if (mInstances.ContainsKey(type))
					mInstances[type] = obj;
				else
					mInstances.Add(type, obj);
			}, () =>
			{
				if (mFabriques.ContainsKey(type))
					mFabriques[type] = () => CreateObject(type);
				else
					mFabriques.Add(type, () => CreateObject(type));
			},
				pattern);

			mLogger?.Trace($"Type '{type.AssemblyQualifiedName}' registered with pattern '{pattern}'.");
		}

		public void Register<TDep>(string name, CreationPattern pattern = CreationPattern.CreateOnGet, bool reassign = false)
			where TDep : class
		{
			if (!reassign && (mNamedInstances.ContainsKey(name) || mNamedFabriques.ContainsKey(name)))
				throw new TypeAlreadyRegisteredException(name);

			var type = typeof(TDep);
			Register(type, obj => mNamedInstances.Add(name, obj), () => mNamedFabriques.Add(name, CreateObject<TDep>), pattern);
			mLogger?.Trace($"Type '{type}' with name '{name}' registered with pattern {pattern}.");
		}

		public void Register(string name, Type type, CreationPattern pattern = CreationPattern.CreateOnGet,
			bool reassign = false)
		{
			if (!reassign && (mNamedInstances.ContainsKey(name) || mNamedFabriques.ContainsKey(name)))
				throw new TypeAlreadyRegisteredException(name);

			Register(type, obj => mNamedInstances.Add(name, obj), () => mNamedFabriques.Add(name, () => CreateObject(type)),
				pattern);
			mLogger?.Trace($"Type '{type}' with name '{name}' registered with pattern {pattern}.");
		}

		public void RegisterFabrique(Type type, Func<object> fabriqueFunction, bool reassign = false)
		{
			if (!reassign && (mInstances.ContainsKey(type) || mFabriques.ContainsKey(type)))
				throw new TypeAlreadyRegisteredException(type);

			if (!mFabriques.ContainsKey(type))
				mFabriques.Add(type, fabriqueFunction);
			else
				mFabriques[type] = fabriqueFunction;

			mLogger?.Trace($"Type '{type.AssemblyQualifiedName}' registered with Fabrique.");
		}

		public object CreateObject(Type t)
		{
			mLogger?.Trace($"Attempt to create object of type '{t}'.");
			if (t.IsInterface)
				throw new IncorrectContructorException(t,
					"Type is interface. Registering only by type that is interface is not allowed.");
			var ctor = t.GetConstructors().FirstOrDefault();
			var ctorParams = ctor.GetParameters();
			List<object> ctorParamsObjects = new List<object>();
			foreach (var parameterInfo in ctorParams)
			{
				InjectAttribute attr =
					(InjectAttribute) Attribute.GetCustomAttributes(parameterInfo, typeof(InjectAttribute)).FirstOrDefault();
				if (parameterInfo.ParameterType == typeof(IoCContainer) ||
				    parameterInfo.ParameterType == typeof(InjectionProviderContract))
					ctorParamsObjects.Add(this);
				else
				{
					if (!parameterInfo.HasDefaultValue)
						ctorParamsObjects.Add(attr != null ? Get<object>(attr.InjectingObjectName) : Get(parameterInfo.ParameterType));
					else
						ctorParamsObjects.Add(parameterInfo.DefaultValue);
				}
			}

			var result = ctor.Invoke(ctorParamsObjects.ToArray());
			mLogger?.Trace($"Object of type '{t}' created.");
			return result;
		}

		public TObj CreateObject<TObj>()
		{
			return (TObj) CreateObject(typeof(TObj));
		}

		public void RegisterFabrique<TDep>(Func<object> fabriqueFunction, bool reassign = false)
		{
			var type = typeof(TDep);
			RegisterFabrique(type, fabriqueFunction, reassign);
		}

		public void RegisterFabrique(string name, Func<object> fabriqueFunction, bool reassign = false)
		{
			if (!reassign && (mNamedInstances.ContainsKey(name) || mNamedFabriques.ContainsKey(name)))
				throw new TypeAlreadyRegisteredException(name);

			if (mNamedFabriques.ContainsKey(name))
				mNamedFabriques[name] = fabriqueFunction;
			else
				mNamedFabriques.Add(name, fabriqueFunction);

			mLogger?.Trace($"Fabrique with name '{name}' registered as fabrique.");
		}

		public void RegisterResolver<TDep>(Func<dynamic, object> resolver, bool reassign = false)
		{
			var type = typeof(TDep);
			if (!reassign && mResolvers.ContainsKey(type))
				throw new ResolverAlreadyRegisteredException(type);

			if (mResolvers.ContainsKey(type))
				mResolvers[type] = resolver;
			else
				mResolvers.Add(type, resolver);

			mLogger?.Trace($"Resolver for type '{type}' registered.");
		}

		public void RegisterResolver(string name, Func<dynamic, object> resolver, bool reassign = false)
		{
			if (!reassign && mNamedResolvers.ContainsKey(name))
				throw new ResolverAlreadyRegisteredException(name);

			if (mNamedResolvers.ContainsKey(name))
				mNamedResolvers[name] = resolver;
			else
				mNamedResolvers.Add(name, resolver);

			mLogger?.Trace($"Resolver with name '{name}' registered.");
		}

		public TDep Get<TDep>() where TDep : class
		{
			var type = typeof(TDep);
			var value = Get(type);

			var result = value as TDep;
			if (result == null)
				throw new TypeMismatchException(type, value.GetType());

			return result;
		}

		public object Get(Type type)
		{
			object value;

			if (!mInstances.ContainsKey(type))
			{
				if (!mFabriques.ContainsKey(type))
					throw new TypeNotRegisteredException(type);

				value = mFabriques[type]();
			}
			else
			{
				value = mInstances[type].Value;
			}

			if (value == null)
				throw new NullNotAllowedException();

			return value;
		}

		public object Get(string name)
		{
			object value;
			if (!mNamedInstances.ContainsKey(name))
			{
				if (!mNamedFabriques.ContainsKey(name))
					throw new TypeNotRegisteredException(name);

				value = mNamedFabriques[name]();
			}
			else
			{
				value = mNamedInstances[name].Value;
			}

			if (value == null)
				throw new NullNotAllowedException();

			return value;
		}

		public TDep Get<TDep>(string name) where TDep : class
		{
			object value = Get(name);

			var result = value as TDep;
			if (result == null)
				throw new TypeMismatchException(typeof(TDep), value.GetType());

			return result;
		}

		public TDep Resolve<TDep>(dynamic resolvingArguments) where TDep : class
		{
			var type = typeof(TDep);
			if (!mResolvers.ContainsKey(type))
				throw new ResolverNotRegisteredException(type);

			var value = mResolvers[type](resolvingArguments);
			if (value == null)
				throw new NullNotAllowedException();

			var result = value as TDep;
			if (result == null)
				throw new TypeMismatchException(type, value.GetType());

			return result;
		}

		public TDep Resolve<TDep>(string resolverName, dynamic resolvingArguments) where TDep : class
		{
			if (!mNamedResolvers.ContainsKey(resolverName))
				throw new ResolverNotRegisteredException(resolverName);

			var value = mNamedResolvers[resolverName](resolvingArguments);
			if (value == null)
				throw new NullNotAllowedException();

			var result = value as TDep;
			if (result == null)
				throw new TypeMismatchException(typeof(TDep), value.GetType());

			return result;
		}

		public void ScanAssemblies()
		{
			foreach (
				var injectingObject in
					AppDomain.CurrentDomain.GetAssemblies()
						.SelectMany(
							el =>
								el.GetTypes()
									.Select(type => new {type, attribute = Attribute.GetCustomAttributes(type, typeof(InjectingAttribute))}))
						.Where(el => el.attribute.Any())
						.Select(el => new {el.type, attribute = (InjectingAttribute) el.attribute.First()}))
			{
				var fabriqueMethods =
					injectingObject.type.GetMethods()
						.Select(el => new {method = el, attributes = Attribute.GetCustomAttributes(el, typeof(FabriqueAttribute))})
						.Where(el => el.attributes.Any())
						.Select(el => new {el.method, attribute = (FabriqueAttribute) el.attributes.First()});

				var fabReg = false;
				foreach (var fabriqueMethod in fabriqueMethods)
				{
					fabReg = true;
					Type t = fabriqueMethod.method.ReturnType;
					if (!t.IsAssignableFrom(injectingObject.type))
						throw new FabriqueMethodReturnTypeMismatchException(injectingObject.type, t, fabriqueMethod.method);

					if (string.IsNullOrEmpty(fabriqueMethod.attribute.Name))
						RegisterFabrique(t, () => fabriqueMethod.method.Invoke(null, new object[] {}),
							fabriqueMethod.attribute.AllowReassign);
					else
						RegisterFabrique(fabriqueMethod.attribute.Name, () => fabriqueMethod.method.Invoke(null, new object[] {}),
							fabriqueMethod.attribute.AllowReassign);
				}
				if (fabReg) continue;

				if (string.IsNullOrEmpty(injectingObject.attribute.Name))
					Register(injectingObject.type, injectingObject.attribute.Pattern, injectingObject.attribute.AllowReassign);
				else
					Register(injectingObject.attribute.Name, injectingObject.type, injectingObject.attribute.Pattern,
						injectingObject.attribute.AllowReassign);
			}
		}

		public void SetLogger(ILogger logger)
		{
			mLogger = logger;
		}
	}
}