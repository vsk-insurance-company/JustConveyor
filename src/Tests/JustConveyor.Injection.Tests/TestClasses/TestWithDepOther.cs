using JustConveyor.Contracts.Dependencies.Attributes;

namespace JustConveyor.Injection.Tests.TestClasses
{
	[Injecting]
	public class TestWithDepOther
	{
		public TestOther Test1 { get; }

		public TestWithDepOther(TestOther obj)
		{
			Test1 = obj;
		}
	}
}