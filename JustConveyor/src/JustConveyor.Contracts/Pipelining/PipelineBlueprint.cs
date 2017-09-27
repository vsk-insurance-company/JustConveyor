using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using JustConveyor.Contracts.Exceptions;
using JustConveyor.Contracts.Pipelining.Attributes;
using JustConveyor.Contracts.Pipelining.Contexts;
using JustConveyor.Contracts.Pipelining.ProcessingElements;

namespace JustConveyor.Contracts.Pipelining
{
	/// <summary>
	/// Blueprint of pipeline
	/// </summary>
	public class PipelineBlueprint
	{
		public ProcessingElement First { get; set; }
		public ProcessingElement Last { get; set; }

		/// <summary>
		/// Type of awating seed.
		/// </summary>
		public Type SeedType { get; private set; }

		/// <summary>
		/// Name of blueprint
		/// </summary>
		public string Name { get; private set; }

		private PipelineBlueprint()
		{
		}

		/// <summary>
		/// Function for creating blueprint.
		/// </summary>
		/// <param name="seedType">Type of seed.</param>
		/// <param name="blueprintName">Name of blueprint.</param>
		/// <returns></returns>
		public static PipelineBlueprint CreateBlueprint(Type seedType, string blueprintName)
		{
			return new PipelineBlueprint
			{
				SeedType = seedType,
				Name = blueprintName
			};
		}

		/// <summary>
		/// Function for creating blueprint.
		/// </summary>
		/// <param name="blueprintName">Name of blueprint.</param>
		/// <returns></returns>
		public static PipelineBlueprint CreateBlueprint<TType>(string blueprintName)
		{
			return CreateBlueprint(typeof(TType), blueprintName);
		}

		#region Collectors

		private PipelineBlueprint RegisterCollectorFromLabmda(MethodInfo method, MethodInfo errorProcessor)
		{
			var element = new Collector
			{
				ErrorProcessorMethod = errorProcessor,
				IsAsync = false,
				Method = method,
			};

			UpdateTail(element);
			return this;
		}

		/// <summary>
		/// Create collect step in blueprint.
		/// </summary>
		/// <param name="limiter"></param>
		/// <param name="name"></param>
		/// <typeparam name="TType"></typeparam>
		/// <returns></returns>
		public PipelineBlueprint Collect<TType>(Func<string, IEnumerable<UnitContext>, bool> limiter, string name = "")
		{
			Type carrier = typeof(TType);

			return CreateElement(name, carrier, typeof(CollectorAttribute), attr => ((CollectorAttribute) attr).Name,
				"Collector",
				method => new Collector
				{
					Carrier = carrier,
					IsAsync = IsAsync(method),
					Method = method,
				});
		}

		/// <summary>
		/// Create collect step in blueprint.
		/// </summary>
		/// <param name="idExtractor"></param>
		/// <param name="limiter"></param>
		/// <param name="errorProcessor"></param>
		/// <returns></returns>
		public PipelineBlueprint Collect(Func<UnitContext, string> idExtractor,
			Func<string, IEnumerable<UnitContext>, bool> limiter = null, Func<Exception, bool> errorProcessor = null)
		{
			return RegisterCollectorFromLabmda(idExtractor.GetMethodInfo(), errorProcessor?.GetMethodInfo());
		}

		/// <summary>
		/// Create collect step in blueprint.
		/// </summary>
		/// <param name="idExtractor"></param>
		/// <param name="limiter"></param>
		/// <param name="errorProcessor"></param>
		/// <returns></returns>
		public PipelineBlueprint Collect(Func<TransferingContext, string> idExtractor,
			Func<string, IEnumerable<UnitContext>, bool> limiter = null, Func<Exception, bool> errorProcessor = null)
		{
			return RegisterCollectorFromLabmda(idExtractor.GetMethodInfo(), errorProcessor?.GetMethodInfo());
		}

		/// <summary>
		/// Create collect step in blueprint.
		/// </summary>
		/// <typeparam name="TParameter"></typeparam>
		/// <param name="idExtractor"></param>
		/// <param name="limiter"></param>
		/// <param name="errorProcessor"></param>
		/// <returns></returns>
		public PipelineBlueprint Collect<TParameter>(Func<TParameter, string> idExtractor,
			Func<string, IEnumerable<UnitContext>, bool> limiter = null, Func<Exception, bool> errorProcessor = null)
		{
			return RegisterCollectorFromLabmda(idExtractor.GetMethodInfo(), errorProcessor?.GetMethodInfo());
		}

		/// <summary>
		/// Create collect step in blueprint.
		/// </summary>
		/// <param name="idExtractor"></param>
		/// <param name="limiter"></param>
		/// <param name="errorProcessor"></param>
		/// <returns></returns>
		public PipelineBlueprint Collect(Func<UnitContext, TransferingContext, string> idExtractor,
			Func<string, IEnumerable<UnitContext>, bool> limiter = null, Func<Exception, bool> errorProcessor = null)
		{
			return RegisterCollectorFromLabmda(idExtractor.GetMethodInfo(), errorProcessor?.GetMethodInfo());
		}

		/// <summary>
		/// Create collect step in blueprint.
		/// </summary>
		/// <typeparam name="TParameter"></typeparam>
		/// <param name="idExtractor"></param>
		/// <param name="limiter"></param>
		/// <param name="errorProcessor"></param>
		/// <returns></returns>
		public PipelineBlueprint Collect<TParameter>(Func<UnitContext, TParameter, string> idExtractor,
			Func<string, IEnumerable<UnitContext>, bool> limiter = null, Func<Exception, bool> errorProcessor = null)
		{
			return RegisterCollectorFromLabmda(idExtractor.GetMethodInfo(), errorProcessor?.GetMethodInfo());
		}

		/// <summary>
		/// Create collect step in blueprint.
		/// </summary>
		/// <typeparam name="TParameter"></typeparam>
		/// <param name="idExtractor"></param>
		/// <param name="limiter"></param>
		/// <param name="errorProcessor"></param>
		/// <returns></returns>
		public PipelineBlueprint Collect<TParameter>(Func<TransferingContext, TParameter, string> idExtractor,
			Func<string, IEnumerable<UnitContext>, bool> limiter = null, Func<Exception, bool> errorProcessor = null)
		{
			return RegisterCollectorFromLabmda(idExtractor.GetMethodInfo(), errorProcessor?.GetMethodInfo());
		}

		/// <summary>
		/// Create collect step in blueprint.
		/// </summary>
		/// <typeparam name="TParameter"></typeparam>
		/// <param name="idExtractor"></param>
		/// <param name="limiter"></param>
		/// <param name="errorProcessor"></param>
		/// <returns></returns>
		public PipelineBlueprint Collect<TParameter>(
			Func<TransferingContext, UnitContext, TParameter, string> idExtractor,
			Func<string, IEnumerable<UnitContext>, bool> limiter = null, Func<Exception, bool> errorProcessor = null)
		{
			return RegisterCollectorFromLabmda(idExtractor.GetMethodInfo(), errorProcessor?.GetMethodInfo());
		}

		/// <summary>
		/// Create trivial collect step in blueprint.
		/// </summary>
		/// <returns></returns>
		public PipelineBlueprint Collect()
		{
			var element = new Collector
			{
				Carrier = null,
				IsAsync = false,
				Method = null
			};
			UpdateTail(element);
			return this;
		}

		#endregion

		#region Split

		/// <summary>
		/// Create split step in blueprint.
		/// </summary>
		/// <typeparam name="TType"></typeparam>
		/// <param name="name"></param>
		/// <returns></returns>
		public PipelineBlueprint Split<TType>(string name = "")
		{
			Type carrier = typeof(TType);

			return CreateElement(name, carrier, typeof(SplitterAttribute), attr => ((SplitterAttribute) attr).Name, "Splitter",
				method => new Splitter
				{
					Carrier = carrier,
					IsAsync = IsAsync(method),
					Method = method
				});
		}

		private PipelineBlueprint RegisterSplitterFromLabmda(MethodInfo method, MethodInfo errorProcessor)
		{
			var element = new Splitter
			{
				ErrorProcessorMethod = errorProcessor,
				IsAsync = false,
				Method = method
			};

			UpdateTail(element);
			return this;
		}

		/// <summary>
		/// Create split step in blueprint.
		/// </summary>
		/// <typeparam name="TParameter"></typeparam>
		/// <param name="processor"></param>
		/// <param name="errorProcessor"></param>
		/// <returns></returns>
		public PipelineBlueprint Split<TParameter>(
			Func<UnitContext, TransferingContext, TParameter, IEnumerator<UnitContext>> processor,
			Func<Exception, bool> errorProcessor = null)
		{
			return RegisterSplitterFromLabmda(processor.GetMethodInfo(), errorProcessor?.GetMethodInfo());
		}

		/// <summary>
		/// Create split step in blueprint.
		/// </summary>
		/// <param name="processor"></param>
		/// <param name="errorProcessor"></param>
		/// <returns></returns>
		public PipelineBlueprint Split(Func<UnitContext, TransferingContext, IEnumerator<UnitContext>> processor,
			Func<Exception, bool> errorProcessor = null)
		{
			return RegisterSplitterFromLabmda(processor.GetMethodInfo(), errorProcessor?.GetMethodInfo());
		}

		/// <summary>
		/// Create split step in blueprint.
		/// </summary>
		/// <typeparam name="TParameter"></typeparam>
		/// <param name="processor"></param>
		/// <param name="errorProcessor"></param>
		/// <returns></returns>
		public PipelineBlueprint Split<TParameter>(
			Func<UnitContext, TParameter, IEnumerator<UnitContext>> processor,
			Func<Exception, bool> errorProcessor = null)
		{
			return RegisterSplitterFromLabmda(processor.GetMethodInfo(), errorProcessor?.GetMethodInfo());
		}

		/// <summary>
		/// Create split step in blueprint.
		/// </summary>
		/// <typeparam name="TParameter"></typeparam>
		/// <param name="processor"></param>
		/// <param name="errorProcessor"></param>
		/// <returns></returns>
		public PipelineBlueprint Split<TParameter>(
			Func<TransferingContext, TParameter, IEnumerator<UnitContext>> processor,
			Func<Exception, bool> errorProcessor = null)
		{
			return RegisterSplitterFromLabmda(processor.GetMethodInfo(), errorProcessor?.GetMethodInfo());
		}

		/// <summary>
		/// Create split step in blueprint.
		/// </summary>
		/// <param name="processor"></param>
		/// <param name="errorProcessor"></param>
		/// <returns></returns>
		public PipelineBlueprint Split(Func<UnitContext, IEnumerator<UnitContext>> processor,
			Func<Exception, bool> errorProcessor = null)
		{
			return RegisterSplitterFromLabmda(processor.GetMethodInfo(), errorProcessor?.GetMethodInfo());
		}

		/// <summary>
		/// Create split step in blueprint.
		/// </summary>
		/// <param name="processor"></param>
		/// <param name="errorProcessor"></param>
		/// <returns></returns>
		public PipelineBlueprint Split(Func<TransferingContext, IEnumerator<UnitContext>> processor,
			Func<Exception, bool> errorProcessor = null)
		{
			return RegisterSplitterFromLabmda(processor.GetMethodInfo(), errorProcessor?.GetMethodInfo());
		}

		/// <summary>
		/// Create split step in blueprint.
		/// </summary>
		/// <typeparam name="TParameter"></typeparam>
		/// <param name="processor"></param>
		/// <param name="errorProcessor"></param>
		/// <returns></returns>
		public PipelineBlueprint Split<TParameter>(Func<TParameter, IEnumerator<UnitContext>> processor,
			Func<Exception, bool> errorProcessor = null)
		{
			return RegisterSplitterFromLabmda(processor.GetMethodInfo(), errorProcessor?.GetMethodInfo());
		}

		#endregion

		#region Processors

		/// <summary>
		/// Create process step in blueprint.
		/// </summary>
		/// <typeparam name="TType"></typeparam>
		/// <param name="name"></param>
		/// <returns></returns>
		public PipelineBlueprint Apply<TType>(string name = "")
		{
			Type carrier = typeof(TType);

			return CreateElement(name, carrier, typeof(ProcessorAttribute), attr => ((ProcessorAttribute) attr).Name,
				"Processor",
				method => new Processor
				{
					Carrier = carrier,
					IsAsync = IsAsync(method),
					Method = method
				});
		}

	    /// <summary>
	    /// Create process step in blueprint.
	    /// </summary>
	    /// <typeparam name="TParameter"></typeparam>
	    /// <typeparam name="TResult"></typeparam>
	    /// <param name="processor"></param>
	    /// <param name="name"></param>
	    /// <param name="errorProcessor"></param>
	    /// <returns></returns>
	    public PipelineBlueprint Apply<TParameter, TResult>(
			Func<UnitContext, TransferingContext, TParameter, TResult> processor,
            string name = "noname",
			Func<Exception, bool> errorProcessor = null)
		{
			return RegisterProcessorFromLabmda(processor.Target, processor.GetMethodInfo(), errorProcessor?.GetMethodInfo(), name: name);
		}

	    /// <summary>
	    /// Create process step in blueprint.
	    /// </summary>
	    /// <typeparam name="TResult"></typeparam>
	    /// <param name="processor"></param>
	    /// <param name="name"></param>
	    /// <param name="errorProcessor"></param>
	    /// <returns></returns>
	    public PipelineBlueprint Apply<TResult>(Func<UnitContext, TransferingContext, TResult> processor,
            string name = "noname",
            Func<Exception, bool> errorProcessor = null)
		{
			return RegisterProcessorFromLabmda(processor.Target, processor.GetMethodInfo(), errorProcessor?.GetMethodInfo(), name: name);
		}

	    /// <summary>
	    /// Create process step in blueprint.
	    /// </summary>
	    /// <typeparam name="TParameter"></typeparam>
	    /// <typeparam name="TResult"></typeparam>
	    /// <param name="processor"></param>
	    /// <param name="name"></param>
	    /// <param name="errorProcessor"></param>
	    /// <returns></returns>
	    public PipelineBlueprint Apply<TParameter, TResult>(Func<UnitContext, TParameter, TResult> processor,
            string name = "noname",
            Func<Exception, bool> errorProcessor = null)
		{
			return RegisterProcessorFromLabmda(processor.Target, processor.GetMethodInfo(), errorProcessor?.GetMethodInfo(), name: name);
		}

	    /// <summary>
	    /// Create process step in blueprint.
	    /// </summary>
	    /// <typeparam name="TParameter"></typeparam>
	    /// <typeparam name="TResult"></typeparam>
	    /// <param name="processor"></param>
	    /// <param name="name"></param>
	    /// <param name="errorProcessor"></param>
	    /// <returns></returns>
	    public PipelineBlueprint Apply<TParameter, TResult>(Func<TransferingContext, TParameter, TResult> processor,
            string name = "noname",
            Func<Exception, bool> errorProcessor = null)
		{
			return RegisterProcessorFromLabmda(processor.Target, processor.GetMethodInfo(), errorProcessor?.GetMethodInfo(), name: name);
		}

	    /// <summary>
	    /// Create process step in blueprint.
	    /// </summary>
	    /// <typeparam name="TResult"></typeparam>
	    /// <param name="processor"></param>
	    /// <param name="name"></param>
	    /// <param name="errorProcessor"></param>
	    /// <returns></returns>
	    public PipelineBlueprint Apply<TResult>(Func<UnitContext, TResult> processor,
            string name = "noname",
            Func<Exception, bool> errorProcessor = null)
		{
			return RegisterProcessorFromLabmda(processor.Target, processor.GetMethodInfo(), errorProcessor?.GetMethodInfo(), name: name);
		}

	    /// <summary>
	    /// Create process step in blueprint.
	    /// </summary>
	    /// <typeparam name="TResult"></typeparam>
	    /// <param name="processor"></param>
	    /// <param name="name"></param>
	    /// <param name="errorProcessor"></param>
	    /// <returns></returns>
	    public PipelineBlueprint Apply<TResult>(Func<TransferingContext, TResult> processor,
            string name = "noname",
            Func<Exception, bool> errorProcessor = null)
		{
			return RegisterProcessorFromLabmda(processor.Target, processor.GetMethodInfo(), errorProcessor?.GetMethodInfo(), name: name);
		}

	    /// <summary>
	    /// Create process step in blueprint.
	    /// </summary>
	    /// <typeparam name="TParameter"></typeparam>
	    /// <typeparam name="TResult"></typeparam>
	    /// <param name="processor"></param>
	    /// <param name="name"></param>
	    /// <param name="errorProcessor"></param>
	    /// <returns></returns>
	    public PipelineBlueprint Apply<TParameter, TResult>(Func<TParameter, TResult> processor,
            string name = "noname",
            Func<Exception, bool> errorProcessor = null)
		{
			return RegisterProcessorFromLabmda(processor.Target, processor.GetMethodInfo(), errorProcessor?.GetMethodInfo());
		}

		private PipelineBlueprint RegisterProcessorFromLabmda(object lambda, MethodInfo method, MethodInfo errorProcessor, string name = "noname")
		{
			var element = new Processor
			{
                Name = name,
				ConcreteObject = lambda,
				ErrorProcessorMethod = errorProcessor,
				IsAsync = IsAsync(method),
				Method = method
			};

			UpdateTail(element);
			return this;
		}

		#endregion

		private bool IsAsync(MethodInfo method)
		{
			return (method.ReturnType.IsConstructedGenericType && method.ReturnType.GetGenericTypeDefinition() == typeof(Task<>))
			       || method.ReturnType == typeof(Task);
		}

		private void UpdateTail(ProcessingElement el)
		{
			el.Previous = Last;
			if (Last != null)
				Last.Next = el;

			Last = el;

			if (First == null)
				First = el;
		}

		private static readonly List<Type> AllowedErrorProcessorTypes = new List<Type>
		{
			typeof(Exception),
			typeof(UnitContext),
			typeof(TransferingContext)
		};

		private PipelineBlueprint CreateElement(string name,
			Type carrier,
			Type attribute,
			Func<Attribute, string> nameExtractor,
			string functionType,
			Func<MethodInfo, ProcessingElement> createElement)
		{
			var methods = carrier.GetMethods();

			var elements = methods
				.Select(el => new {method = el, attributes = el.GetCustomAttributes(attribute)})
				.Where(el => el.attributes.Any())
				.Select(el => new {el.method, attribute = el.attributes.First()}).ToList();

			var errorProcessors = methods.Select(
					el => new {method = el, attirutes = el.GetCustomAttributes(typeof(ErrorProcessorAttribute))})
				.Where(el => el.attirutes.Any())
				.Select(el => new {el.method, attribute = (ErrorProcessorAttribute) el.attirutes.First()});

			if (string.IsNullOrEmpty(name))
			{
				if (elements.Count > 1)
					throw new MoreThanOneUnnamedFunctionsFoundException(carrier, functionType);

				if (elements.Count == 0)
					throw new FunctionNotFoundException(carrier, functionType);
			}
			else if (elements.All(el => nameExtractor(el.attribute) != name))
			{
				throw new FunctionNotFoundException(carrier, functionType, name);
			}

			var function = string.IsNullOrEmpty(name)
				? elements.First()
				: elements.First(el => nameExtractor(el.attribute) == name);

			var errorProcessor = string.IsNullOrEmpty(name)
				? errorProcessors.FirstOrDefault()
				: errorProcessors.FirstOrDefault(el => el.attribute.Name == name);

			if (errorProcessor != null)
			{
				if (errorProcessor.method.ReturnType != typeof(void) && errorProcessor.method.ReturnType != typeof(bool))
					throw new IncorrectErrorProcessorException(carrier, errorProcessor.method.Name,
						$"Return type should be void or bool. but found: '{errorProcessor.method.ReturnType.AssemblyQualifiedName}'");

				var parameterInfos = errorProcessor.method.GetParameters();

				if (parameterInfos.Length > 2 &&
				    parameterInfos.Any(el => !AllowedErrorProcessorTypes.Contains(el.ParameterType)))
					throw new IncorrectErrorProcessorException(carrier, errorProcessor.method.Name,
						$"AllowedTypes for error processor parameters [{string.Join(",", AllowedErrorProcessorTypes.Select(el => $"'{el.AssemblyQualifiedName}'"))}] " +
						$"but found [{string.Join(",", parameterInfos.Select(el => $"'{el.ParameterType.AssemblyQualifiedName}'"))}]");
			}

			var processingElement = createElement(function.method);
			processingElement.ErrorProcessorMethod = errorProcessor?.method;

			UpdateTail(processingElement);

			return this;
		}

		private IEnumerator<ProcessingElement> GetEnumerator()
		{
			var current = First;
			while (current != null)
			{
				yield return current;
				current = current.Next;
			}
		}

		public override string ToString()
		{
			var els = new List<ProcessingElement>();
			
			using (var en = GetEnumerator())
				while (en.MoveNext())
					els.Add(en.Current);

			return string.Join(" -> ", els);
		}
	}
}