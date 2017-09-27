using System;
using System.Reflection;

namespace JustConveyor.Contracts.Exceptions
{
	/// <summary>
	/// Exception for case when instance of conveyor was already initialized.
	/// </summary>
	public class ConveyorInstanceAlreadyInitializer : Exception
	{
		public ConveyorInstanceAlreadyInitializer()
			: base("Instance of Conveyor class was already initalized. Reinitalization unavaliable.")
		{
			
		}
	}

	/// <summary>
	/// Exception that occurs if not splits in blueprint has corresponding collect steps.
	/// </summary>
	public class InvalidSplitCollectException : Exception
	{
		/// <summary>
		/// CTOR.
		/// </summary>
		/// <param name="blueprintPipelineName"></param>
		public InvalidSplitCollectException(string blueprintPipelineName)
			: base($"not all splits has corresponding collect in {blueprintPipelineName}")
		{

		}
	}

	/// <summary>
	/// Exception for case when injection provider not ready.
	/// </summary>
	public class InjectionProviderNotReadyException : Exception
	{
		/// <summary>
		/// CTOR.
		/// </summary>
		public InjectionProviderNotReadyException()
			: base("Injection provider was not initialized. Call RegisterInjectionProvider to initialize provider.")
		{

		}
	}

	/// <summary>
	/// Exception for case when injection provider not ready.
	/// </summary>
	public class TypeNotRegisteredException : Exception
	{
		/// <summary>
		/// CTOR.
		/// </summary>
		public TypeNotRegisteredException(Type type)
			: base($"Type '{type.AssemblyQualifiedName}' not registered.")
		{

		}

		/// <summary>
		/// CTOR.
		/// </summary>
		public TypeNotRegisteredException(string name)
			: base($"Type by name '{name}' not registered.")
		{

		}
	}

	/// <summary>
	/// Exception for case when injection provider not ready.
	/// </summary>
	public class ResolverNotRegisteredException : Exception
	{
		/// <summary>
		/// CTOR.
		/// </summary>
		public ResolverNotRegisteredException(Type type)
			: base($"Resolver for type '{type.AssemblyQualifiedName}' not registered.")
		{

		}

		/// <summary>
		/// CTOR.
		/// </summary>
		public ResolverNotRegisteredException(string name)
			: base($"Resolver with name '{name}' not registered.")
		{

		}
	}

	/// <summary>
	/// Exception for case when injection provider not ready.
	/// </summary>
	public class TypeAlreadyRegisteredException : Exception
	{
		/// <summary>
		/// CTOR.
		/// </summary>
		public TypeAlreadyRegisteredException(Type type)
			: base($"Type '{type.AssemblyQualifiedName}' already registered.")
		{

		}

		/// <summary>
		/// CTOR.
		/// </summary>
		public TypeAlreadyRegisteredException(string name)
			: base($"Type by name '{name}' already registered.")
		{

		}
	}

	/// <summary>
	/// Exception for case when injection provider not ready.
	/// </summary>
	public class ResolverAlreadyRegisteredException : Exception
	{
		/// <summary>
		/// CTOR.
		/// </summary>
		public ResolverAlreadyRegisteredException(Type type)
			: base($"Resolver for type '{type.AssemblyQualifiedName}' already registered.")
		{

		}

		/// <summary>
		/// CTOR.
		/// </summary>
		public ResolverAlreadyRegisteredException(string name)
			: base($"Resolver with name '{name}' already registered.")
		{

		}
	}

	/// <summary>
	/// Exception for case when injection provider not ready.
	/// </summary>
	public class TypeMismatchException : Exception
	{
		/// <summary>
		/// CTOR.
		/// </summary>
		public TypeMismatchException(Type type, Type realType)
			: base($"Unable to cast to type '{type.AssemblyQualifiedName}' that realy is '{realType.AssemblyQualifiedName}'")
		{

		}
	}

	/// <summary>
	/// Exception for case when injection provider not ready.
	/// </summary>
	public class NullNotAllowedException : Exception
	{
		/// <summary>
		/// CTOR.
		/// </summary>
		public NullNotAllowedException()
			: base($"On object creation null was returned. It's not allowed to return null in Fabriques or Resolvers.")
		{

		}
	}

	/// <summary>
	/// Exception for case when injection provider not ready.
	/// </summary>
	public class FabriqueMethodReturnTypeMismatchException : Exception
	{
		/// <summary>
		/// CTOR.
		/// </summary>
		public FabriqueMethodReturnTypeMismatchException(Type type, Type incorrectType, MethodInfo method)
			: base($"Fabrique method '{method.Name}' for type '{type.AssemblyQualifiedName}' return object of type '{incorrectType.AssemblyQualifiedName}' that is not assignable to '{type.AssemblyQualifiedName}'.")
		{

		}
	}

	/// <summary>
	/// Exception for case when injection provider not ready.
	/// </summary>
	public class NotStaticFabriqueMethodException : Exception
	{
		/// <summary>
		/// CTOR.
		/// </summary>
		public NotStaticFabriqueMethodException(Type type, MethodInfo method)
			: base($"Fabrique method '{method.Name}' of type '{type.AssemblyQualifiedName}' .")
		{

		}
	}

	/// <summary>
	/// Exception for case when object has incorrect ctor.
	/// </summary>
	public class IncorrectContructorException : Exception
	{
		/// <summary>
		/// CTOR.
		/// </summary>
		/// <param name="type"></param>
		/// <param name="reason"></param>
		public IncorrectContructorException(Type type, string reason)
			: base($"Type '{type.AssemblyQualifiedName}' has incorrect constructor: {reason}")
		{

		}
	}

	/// <summary>
	/// Exception that occurs during data processing.
	/// </summary>
	public class ProcessingException : Exception
	{
		public string FailedUnitId { get; private set; }

		public ProcessingException(string message,
			string failedUnitId,
			Exception innerException) : base(message, innerException)
		{
			FailedUnitId = failedUnitId;
		}
	}

	/// <summary>
	/// Exception that occurs if header is not registered in context.
	/// </summary>
	public class HeaderNotRegisteredInContext : Exception
	{
		public HeaderNotRegisteredInContext(string name)
			: base($"Header with name '{name}' is not registed in context")
		{
		}
	}

    /// <summary>
    /// Exception that occurs if header already registered in context.
    /// </summary>
    public class HeaderAlreadyRegisteredInContext : Exception
	{
		public HeaderAlreadyRegisteredInContext(string name)
			: base($"Header with name '{name}' is already registed in context")
		{
		}
	}

    /// <summary>
    /// Exception that occurs if header has different type.
    /// </summary>
    public class HeaderTypeMismatch : Exception
	{
		public HeaderTypeMismatch(string name, Type type, Type incorrectType)
			: base($"Header with name '{name}' is not assignable to type '{incorrectType.AssemblyQualifiedName}' cause it's type '{type.AssemblyQualifiedName}'")
		{
		}
	}

    /// <summary>
    /// Exception that occurs if unit has different type.
    /// </summary>
    public class UnitTypeMismatch : Exception
	{
		public UnitTypeMismatch(string unitId, Type type, Type incorrectType)
			: base($"Unit with Id:{unitId} is not assignable to type '{incorrectType.AssemblyQualifiedName}' cause it's type '{type.AssemblyQualifiedName}'")
		{
		}
	}

    /// <summary>
    /// Exception that occurs if function was not found in assembly.
    /// </summary>
    public class FunctionNotFoundException : Exception
	{
		public FunctionNotFoundException(Type type, string functionType)
			: base($"{functionType} function in type '{type.AssemblyQualifiedName}' not found.")
		{
		}

		public FunctionNotFoundException(Type type, string functionType, string name)
			: base($"{functionType} function in type '{type.AssemblyQualifiedName}' with name '{name}' not found.")
		{
		}
	}

    /// <summary>
    /// Exception that occurs if found more that one function without explicit name setted with attribute.
    /// </summary>
    public class MoreThanOneUnnamedFunctionsFoundException : Exception
	{
		public MoreThanOneUnnamedFunctionsFoundException(Type type, string functionType)
			: base($"Type '{type.AssemblyQualifiedName}' contains more that one {functionType} function without explicit name from setted attribute.")
		{
		}
	}

	/// <summary>
	/// Исключение, возникающее в случае если объект не был
	/// зарегистрирован в контексте по типу.
	/// </summary>
	public class ParameterTypeMissmatchException : Exception
	{
		/// <summary>
		/// CTOR.
		/// </summary>
		/// <param name="carrier"></param>
		/// <param name="functionName"></param>
		/// <param name="type"></param>
		/// <param name="incorrectType"></param>
		public ParameterTypeMissmatchException(Type carrier, string functionName, Type type, Type incorrectType)
			: base(
				$"Function {functionName} in type {(carrier != null ? carrier.AssemblyQualifiedName : "lambda")} " +
				$"gets parameter of type '{incorrectType}' but operation context stores unit of type '{type}'.")
		{
		}
	}

	/// <summary>
	/// Исключение, возникающее в случае если объект не был
	/// зарегистрирован в контексте по типу.
	/// </summary>
	public class IncorrectErrorProcessorException : Exception
	{
		public IncorrectErrorProcessorException(Type carrier, string functionName, string reason)
			: base($"Error processor {functionName} of type '{carrier.AssemblyQualifiedName}' has incorrect signature. {reason}")
		{
		}
	}

	/// <summary>
	/// Exception that occurs on exception during processing unit in pipeline.
	/// </summary>
	public class PipelineProcessingException : Exception
	{
		/// <summary>
		/// CTOR.
		/// </summary>
		/// <param name="innerException">Exception that occured during processing unit in pipeline.</param>
		public PipelineProcessingException(Exception innerException) : base("Exception occured during pipeline working.", innerException)
		{
		}
	}

	/// <summary>
	/// Exception that occurs when no any suppliers registered in conveyor.
	/// </summary>
	public class NoAnySupplierRegisteredException : Exception
	{
		/// <summary>
		/// CTOR.
		/// </summary>
		public NoAnySupplierRegisteredException() : 
			base("No any supplier registered in coneyor.")
		{
			
		}
	}
	
	/// <summary>
	/// Exception that occurs when pipeline blueprint not found in Conveyor's registry.
	/// </summary>
	public class BlueprintNotRegisteredException : Exception
	{
		/// <summary>
		/// CTOR.
		/// </summary>
		public BlueprintNotRegisteredException(string name) : 
			base($"Bluprint with name={name} not registered.")
		{
			
		}
	}

	/// <summary>
	/// Exception that occurs when no any pipeline blueprint registered in conveyor.
	/// </summary>
	public class NoAnyBlueprintRegisteredException : Exception
	{
		/// <summary>
		/// CTOR.
		/// </summary>
		public NoAnyBlueprintRegisteredException() : 
			base("No any blueprint registered in coneyor.")
		{
			
		}
	}

	/// <summary>
	/// Exception that occurs during inablity to add package on processing.
	/// </summary>
	public class UnableToPostPackageOnProcessing : Exception
	{
		
	}
}