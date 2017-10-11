namespace JustConveyor.Injection.Tests.TestClasses
{
	public class TestWithDefValueDep
	{
		public string Some { get; }

		public TestWithDefValueDep(string some = "123")
		{
			Some = some;
		}
	}
}