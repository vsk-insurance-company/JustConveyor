using JustConveyor.Contracts.Dependencies.Attributes;

namespace JustConveyor.Injection.Tests.TestClasses
{
	[Injecting]
	public class TestOther
	{
		[Fabrique()]
		public static TestOther CreateTestOther()
		{
			return new TestOther();
		}
	}
}