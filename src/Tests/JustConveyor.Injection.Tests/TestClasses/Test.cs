using JustConveyor.Contracts.Dependencies.Attributes;

namespace JustConveyor.Injection.Tests.TestClasses
{
	[Injecting]
	public class Test
	{
		public static Test Some = new Test();
		public static Test SomeOther = new Test();

		[Fabrique("some")]
		public static Test ReturnSome()
		{
			return Some;
		}

		[Fabrique("someother")]
		public static Test ReturnSomeOther()
		{
			return SomeOther;
		}
	}
}