namespace JustConveyor.Injection.Tests.TestClasses
{
	public class TestWithDep
	{
		public Test Test1 { get; }

		public TestWithDep(Test obj)
		{
			Test1 = obj;
		}
	}
}