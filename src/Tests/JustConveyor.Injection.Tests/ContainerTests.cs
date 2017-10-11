using System;
using FluentAssertions;
using JustConveyor.Contracts.Dependencies;
using JustConveyor.Injection.Tests.TestClasses;
using NUnit.Framework;

namespace JustConveyor.Injection.Tests
{
	[TestFixture]
	public class ContainerTests
	{
		[Test]
		public void SimpleInjectWithSingleCreationShouldWorksWell()
		{
			// GIVEN
			IoCContainer container = new IoCContainer();

			// WHEN
			container.Register<Test>(CreationPattern.SingleImmediatly);
			var first = container.Get<Test>();
			var second = container.Get<Test>();

			// THEN
			first.Should().Be(second);
		}

		[Test]
		public void RegisterByInterfaceShouldWorksWell()
		{
			// GIVEN
			IoCContainer container = new IoCContainer();

			// WHEN
			TestContractFirst obj = new TestContractFirst();
			container.RegisterSingle<TestContract>(obj);
			Action create = () => container.Get<TestContract>();

			// THEN
			create.ShouldNotThrow();
		}

		[Test]
		public void ReassignShouldWorksWell()
		{
			// GIVEN
			IoCContainer container = new IoCContainer();

			// WHEN
			TestContractFirst objFirst = new TestContractFirst();
			TestContractSecond objSecond = new TestContractSecond();
			container.RegisterSingle<TestContract>(objFirst);
			Action reassign = () => container.RegisterSingle<TestContract>(objSecond, true);
			TestContract result = null;
			Action get = () => result = container.Get<TestContract>();

			// THEN
			reassign.ShouldNotThrow();
			get.ShouldNotThrow();
			result.Should().Be(objSecond);
		}

		[Test]
		public void SimpleInjectWithCreationOnCallShouldWorksWell()
		{
			// GIVEN
			IoCContainer container = new IoCContainer();

			// WHEN
			container.Register<Test>();
			var first = container.Get<Test>();
			var second = container.Get<Test>();

			// THEN
			first.Should().NotBe(second);
		}

		[Test]
		public void InjectWithDependenciesShouldWorksWell()
		{
			// GIVEN
			IoCContainer container = new IoCContainer();

			// WHEN
			container.Register<Test>(CreationPattern.SingleImmediatly);
			container.Register<TestWithDep>(CreationPattern.SingleImmediatly);
			Action create = () => container.Get<TestWithDep>();

			// THEN
			create.ShouldNotThrow();
		}

		[Test]
		public void InjectByNameShouldWorksWell()
		{
			// GIVEN
			IoCContainer container = new IoCContainer();

			// WHEN
			container.Register<Test>("test", CreationPattern.SingleImmediatly);
			Action create = () => container.Get<Test>("test");

			// THEN
			create.ShouldNotThrow();
		}

		[Test]
		public void FabriqueByTypeShouldWorksWell()
		{
			// GIVEN
			IoCContainer container = new IoCContainer();

			// WHEN
			Test obj = new Test();
			int hits = 0;
			Func<Test> fabriqueMethod = () =>
			{
				hits++;
				return obj;
			};
			container.RegisterFabrique<Test>(fabriqueMethod);
			Action createFirst = () => container.Get<Test>();
			Action createSecond = () => container.Get<Test>();

			// THEN
			createFirst.ShouldNotThrow();
			createSecond.ShouldNotThrow();
			hits.Should().Be(2);
		}

		[Test]
		public void FabriqueByNameShouldWorksWell()
		{
			// GIVEN
			IoCContainer container = new IoCContainer();

			// WHEN
			Test obj = new Test();
			int hits = 0;
			Func<Test> fabriqueMethod = () =>
			{
				hits++;
				return obj;
			};
			container.RegisterFabrique("test", fabriqueMethod);
			Action createFirst = () => container.Get("test");
			Action createSecond = () => container.Get("test");

			// THEN
			createFirst.ShouldNotThrow();
			createSecond.ShouldNotThrow();
			hits.Should().Be(2);
		}

		[Test]
		public void InjectAttributeByNameShouldWorksWell()
		{
			// GIVEN
			IoCContainer container = new IoCContainer();

			// WHEN
			container.Register<Test>("some", CreationPattern.CreateOnGet);
			container.Register<Test>("someother", CreationPattern.CreateOnGet);
			container.Register<TestOther>(CreationPattern.CreateOnGet);
			container.Register<TestWithDepByName>(CreationPattern.SingleOnCall);
			Action create = () => container.Get<TestWithDepByName>();

			// THEN
			create.ShouldNotThrow();
		}

		[Test]
		public void CtorWithDefaultValueShouldWorksWell()
		{

			// GIVEN
			IoCContainer container = new IoCContainer();

			// WHEN
			container.Register<TestWithDefValueDep>(CreationPattern.SingleOnCall);
			Action create = () => container.Get<TestWithDefValueDep>();

			// THEN
			create.ShouldNotThrow();
			container.Get<TestWithDefValueDep>().Some.Should().Be("123");
		}

		[Test]
		public void ScanShouldWorksWell()
		{
			// GIVEN
			IoCContainer container = new IoCContainer();

			// WHEN
			Action scan = () => container.ScanAssemblies();
			Action create = () => container.Get<TestWithDepByName>();

			// THEN
			scan.ShouldNotThrow();
			create.ShouldNotThrow();
		}
	}
}
