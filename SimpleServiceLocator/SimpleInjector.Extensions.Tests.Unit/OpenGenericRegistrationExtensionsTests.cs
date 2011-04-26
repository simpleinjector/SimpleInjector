using System;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SimpleInjector.Extensions.Tests.Unit
{
    [TestClass]
    public class OpenGenericRegistrationExtensionsTests
    {
        // This is the open generic interface that will be used as service type.
        private interface IService<TA, TB>
        {
        }

        private interface IValidate<T>
        {
            IService<T, int> Service { get; }
        }

        [TestMethod]
        public void RegisterOpenGeneric_WithValidArguments_ReturnsExpectedTypeOnGetInstance()
        {
            // Arrange
            var container = new Container();

            // Act
            container.RegisterOpenGeneric(typeof(IService<,>), typeof(ServiceImpl<,>));

            // Assert
            var impl = container.GetInstance<IService<int, string>>();

            Assert.IsInstanceOfType(impl, typeof(ServiceImpl<int, string>));
        }

        [TestMethod]
        public void RegisterOpenGeneric_WithValidArguments_ReturnsNewInstanceOnEachRequest()
        {
            // Arrange
            var container = new Container();

            container.RegisterOpenGeneric(typeof(IService<,>), typeof(ServiceImpl<,>));

            // Act
            var instance1 = container.GetInstance<IService<int, string>>();
            var instance2 = container.GetInstance<IService<int, string>>();

            // Assert
            Assert.AreNotEqual(instance1, instance2, "Transient objects are expected to be returned.");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void RegisterOpenGeneric_WithClosedServiceType_ThrowsException()
        {
            // Arrange
            var container = new Container();

            // Act
            container.RegisterOpenGeneric(typeof(IService<int, string>), typeof(ServiceImpl<,>));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void RegisterOpenGeneric_WithClosedImplementation_ThrowsException()
        {
            // Arrange
            var container = new Container();

            // Act
            container.RegisterOpenGeneric(typeof(IService<,>), typeof(ServiceImpl<int, int>));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void RegisterOpenGeneric_WithNonRelatedTypes_ThrowsException()
        {
            // Arrange
            var container = new Container();

            // Act
            container.RegisterOpenGeneric(typeof(IService<,>), typeof(Func<,>));

            container.GetInstance<IService<int, string>>();
        }

        [TestMethod]
        public void GetInstance_WithDependedRegisterOpenGenericRegistrations_Succeeds()
        {
            // Arrange
            var container = new Container();

            // The DefaultValidator<T> contains an IService<T, int> as constructor argument.
            container.RegisterOpenGeneric(typeof(IValidate<>), typeof(DefaultValidator<>));
            container.RegisterOpenGeneric(typeof(IService<,>), typeof(ServiceImpl<,>));

            // Act
            var validator = container.GetInstance<IValidate<string>>();

            // Assert
            Assert.IsInstanceOfType(validator, typeof(DefaultValidator<string>));
            Assert.IsInstanceOfType(validator.Service, typeof(ServiceImpl<string, int>));
        }

        [TestMethod]
        public void RegisterSingleOpenGeneric_WithValidArguments_ReturnsExpectedTypeOnGetInstance()
        {
            // Arrange
            var container = new Container();

            // Act
            container.RegisterSingleOpenGeneric(typeof(IService<,>), typeof(ServiceImpl<,>));

            // Assert
            var impl = container.GetInstance<IService<int, string>>();

            Assert.IsInstanceOfType(impl, typeof(ServiceImpl<int, string>));
        }

        [TestMethod]
        public void RegisterSingleOpenGeneric_WithValidArguments_ReturnsNewInstanceOnEachRequest()
        {
            // Arrange
            var container = new Container();

            container.RegisterSingleOpenGeneric(typeof(IService<,>), typeof(ServiceImpl<,>));

            // Act
            var instance1 = container.GetInstance<IService<int, string>>();
            var instance2 = container.GetInstance<IService<int, string>>();

            // Assert
            Assert.AreEqual(instance1, instance2, "Singleton object is expected to be returned.");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void RegisterSingleOpenGeneric_WithClosedServiceType_ThrowsException()
        {
            // Arrange
            var container = new Container();

            // Act
            container.RegisterSingleOpenGeneric(typeof(IService<int, string>), typeof(ServiceImpl<,>));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void RegisterSingleOpenGeneric_WithClosedImplementation_ThrowsException()
        {
            // Arrange
            var container = new Container();

            // Act
            container.RegisterSingleOpenGeneric(typeof(IService<,>), typeof(ServiceImpl<int, int>));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void RegisterSingleOpenGeneric_WithNonRelatedTypes_ThrowsException()
        {
            // Arrange
            var container = new Container();

            // Act
            container.RegisterSingleOpenGeneric(typeof(IService<,>), typeof(Func<,>));
        }

        [TestMethod]
        public void GetInstance_WithDependedRegisterSingleOpenGenericRegistrations_Succeeds()
        {
            // Arrange
            var container = new Container();

            // The DefaultValidator<T> contains an IService<T, int> as constructor argument.
            container.RegisterSingleOpenGeneric(typeof(IValidate<>), typeof(DefaultValidator<>));
            container.RegisterSingleOpenGeneric(typeof(IService<,>), typeof(ServiceImpl<,>));

            // Act
            var validator = container.GetInstance<IValidate<string>>();

            // Assert
            Assert.IsInstanceOfType(validator, typeof(DefaultValidator<string>));
            Assert.IsInstanceOfType(validator.Service, typeof(ServiceImpl<string, int>));
        }

        private class ServiceImpl<TA, TB> : IService<TA, TB>
        {
        }

        private class DefaultValidator<T> : IValidate<T>
        {
            public DefaultValidator(IService<T, int> service)
            {
                this.Service = service;
            }

            public IService<T, int> Service { get; private set; }
        }
    }
}