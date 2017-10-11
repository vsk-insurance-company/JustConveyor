using JustConveyor.Contracts.Dependencies.Attributes;

namespace JustConveyor.Injection.Tests.TestClasses
{
	[Injecting]
	public class TestWithDepByName
	{
		public Test Some { get; }
		public Test SomeOther { get; }
		public TestOther TestOther { get; set; }

		public TestWithDepByName([Inject("some")] Test some, [Inject("someother")] Test someOther, TestOther testOther)
		{
			Some = some;
			SomeOther = someOther;
			TestOther = testOther;
		}
	}
}