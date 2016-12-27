namespace SimpleInjector.CodeSamples.Tests.Unit
{
    using System;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SimpleInjector.Tests.Unit;

    [TestClass]
    public class ParameterConventionTests
    {
        public interface IService
        {
        }

        public interface IDependency
        {
        }

        [TestMethod]
        public void RegisterConcrete_WithAllValidParameters_Succeeds()
        {
            // Arrange
            var container = new Container();

            var convention = new ParameterConvention();

            container.Options.RegisterParameterConvention(convention);

            // Act
            // ctor: ClassWithPrimiveConstructorParams(string someValue, DateTime now, string name)
            container.Register<ClassWithOnlyPrimitiveConstructorParams>(
                convention.WithParameter("someValue", "foo"),
                convention.WithParameter(() => DateTime.MinValue),
                convention.WithParameter("name", () => "bar"));

            container.Verify();

            container.GetInstance<ClassWithOnlyPrimitiveConstructorParams>();
        }

        [TestMethod]
        public void Register_WithAllValidParameters_Succeeds()
        {
            // Arrange
            var container = new Container();

            var convention = new ParameterConvention();

            container.Options.RegisterParameterConvention(convention);

            // Act
            // ctor: ClassWithPrimiveConstructorParams(string someValue, DateTime now, string name)
            container.Register<IService, ClassWithOnlyPrimitiveConstructorParams>(
                convention.WithParameter("someValue", "foo"),
                convention.WithParameter(() => DateTime.MinValue),
                convention.WithParameter("name", () => "bar"));

            container.Verify();

            container.GetInstance<ClassWithOnlyPrimitiveConstructorParams>();
        }

        [TestMethod]
        public void RegisterSingleConcrete_WithAllValidParameters_Succeeds()
        {
            // Arrange
            var container = new Container();

            var convention = new ParameterConvention();

            container.Options.RegisterParameterConvention(convention);

            // Act
            // ctor: ClassWithPrimiveConstructorParams(string someValue, DateTime now, string name)
            container.RegisterSingleton<ClassWithOnlyPrimitiveConstructorParams>(
                convention.WithParameter("someValue", "foo"),
                convention.WithParameter(DateTime.MinValue),
                convention.WithParameter("name", "bar"));

            container.Verify();

            container.GetInstance<ClassWithOnlyPrimitiveConstructorParams>();
        }

        [TestMethod]
        public void RegisterSingle_WithAllValidParameters_Succeeds()
        {
            // Arrange
            var container = new Container();

            var convention = new ParameterConvention();

            container.Options.RegisterParameterConvention(convention);

            // Act
            // ctor: ClassWithPrimiveConstructorParams(string someValue, DateTime now, string name)
            container.RegisterSingleton<IService, ClassWithOnlyPrimitiveConstructorParams>(
                convention.WithParameter("someValue", "foo"),
                convention.WithParameter(DateTime.MinValue),
                convention.WithParameter("name", "bar"));

            container.Verify();

            container.GetInstance<ClassWithOnlyPrimitiveConstructorParams>();
        }

        [TestMethod]
        public void GetInstance_TypeWithAllValidParametersRegistered_InjectsExpectedValues()
        {
            // Arrange
            string someValue = "foo";
            DateTime now = new DateTime(2012, 03, 4);
            string name = "bar";

            var container = new Container();

            var convention = new ParameterConvention();

            container.Options.RegisterParameterConvention(convention);

            // ctor: ClassWithPrimiveConstructorParams(string someValue, DateTime now, string name)
            container.Register<ClassWithOnlyPrimitiveConstructorParams>(
                convention.WithParameter("someValue", someValue),
                convention.WithParameter(() => now),
                convention.WithParameter("name", () => name));

            container.Verify();

            // Act
            var instance = container.GetInstance<ClassWithOnlyPrimitiveConstructorParams>();

            // Assert
            Assert.AreEqual(now, instance.Now);
            Assert.AreEqual(someValue, instance.SomeValue);
            Assert.AreEqual(name, instance.Name);
        }

        [TestMethod]
        public void Register_TypeWithMixedParameters_Succeeds()
        {
            // Arrange
            var container = new Container();

            var convention = new ParameterConvention();

            container.Options.RegisterParameterConvention(convention);

            container.RegisterSingleton<IDependency>(new Dependency());

            // Act
            // ctor: ClassWithAPrimitiveConstructorParam(IDependency dependency, Decimal someDecimal)
            container.RegisterSingleton<ClassWithAPrimitiveConstructorParam>(
                convention.WithParameter("someDecimal", decimal.MinValue));

            container.Verify();
        }

        [TestMethod]
        public void GetInstance_Type_TypeWithMixedParameters_InjectsExpectedValues()
        {
            // Arrange
            var expectedDependency = new Dependency();
            decimal expectedDecimal = decimal.MaxValue;

            var container = new Container();
            var convention = new ParameterConvention();
            container.Options.RegisterParameterConvention(convention);
            container.RegisterSingleton<IDependency>(expectedDependency);

            // ctor: ClassWithAPrimitiveConstructorParam(IDependency dependency, Decimal someDecimal)
            container.RegisterSingleton<ClassWithAPrimitiveConstructorParam>(
                convention.WithParameter("someDecimal", expectedDecimal));

            container.Verify();

            // Act
            var instance = container.GetInstance<ClassWithAPrimitiveConstructorParam>();

            // Assert
            Assert.IsTrue(object.ReferenceEquals(expectedDependency, instance.Dependency));
            Assert.AreEqual(expectedDecimal, instance.SomeDecimal);
        }

        [TestMethod]
        public void Register_WithParameterWithNonExistingParamName_FailsWithExpectedException()
        {
            // Arrange
            var container = new Container();

            var convention = new ParameterConvention();

            container.Options.RegisterParameterConvention(convention);

            try
            {
                // Act
                // ctor: ClassWithPrimiveConstructorParams(string someValue, DateTime now, string name)
                container.Register<ClassWithOnlyPrimitiveConstructorParams>(
                    convention.WithParameter("notExistingParamName", "foo"),
                    convention.WithParameter(() => DateTime.MinValue),
                    convention.WithParameter("name", () => "bar"));

                // Assert
                Assert.Fail("Exception expected.");
            }
            catch (ArgumentException ex)
            {
                AssertThat.StringContains(
                    "Parameter with name 'notExistingParamName' of type String is not a parameter of " +
                    "the constructor of type ClassWithOnlyPrimitiveConstructorParams",
                    ex.Message);
            }
        }

        [TestMethod]
        public void Register_AmbiguousWithParameterRegistrations_FailsWithExpectedException()
        {
            // Arrange
            var container = new Container();

            var convention = new ParameterConvention();

            container.Options.RegisterParameterConvention(convention);

            try
            {
                // Act
                // ctor: ClassWithPrimiveConstructorParams(string someValue, DateTime now, string name)
                container.Register<ClassWithOnlyPrimitiveConstructorParams>(
                    convention.WithParameter<string>("foo"),
                    convention.WithParameter(() => DateTime.MinValue),
                    convention.WithParameter("name", () => "bar"));

                // Assert
                Assert.Fail("Exception expected.");
            }
            catch (ArgumentException ex)
            {
                AssertThat.StringContains(
                    "Multiple parameter registrations found for type ClassWithOnlyPrimitiveConstructorParams " +
                    "that match to parameter with name 'name' of type String.",
                    ex.Message);
            }
        }

        public class ClassWithOnlyPrimitiveConstructorParams : IService
        {
            public ClassWithOnlyPrimitiveConstructorParams(string someValue, DateTime now, string name)
            {
                this.SomeValue = someValue;
                this.Now = now;
                this.Name = name;
            }

            public string SomeValue { get; }

            public DateTime Now { get; }

            public string Name { get; }
        }

        public class ClassWithAPrimitiveConstructorParam
        {
            public ClassWithAPrimitiveConstructorParam(IDependency dependency, decimal someDecimal)
            {
                this.Dependency = dependency;
                this.SomeDecimal = someDecimal;
            }

            public IDependency Dependency { get; }

            public decimal SomeDecimal { get; }
        }

        public class Dependency : IDependency
        {
        }
    }
}