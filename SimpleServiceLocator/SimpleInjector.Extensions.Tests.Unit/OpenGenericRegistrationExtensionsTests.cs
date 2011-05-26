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
            void Validate(T instance);
        }

        private interface IDoStuff<T>
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
            container.RegisterOpenGeneric(typeof(IDoStuff<>), typeof(DefaultStuff<>));
            container.RegisterOpenGeneric(typeof(IService<,>), typeof(ServiceImpl<,>));

            // Act
            var validator = container.GetInstance<IDoStuff<string>>();

            // Assert
            Assert.IsInstanceOfType(validator, typeof(DefaultStuff<string>));
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
            container.RegisterSingleOpenGeneric(typeof(IDoStuff<>), typeof(DefaultStuff<>));
            container.RegisterSingleOpenGeneric(typeof(IService<,>), typeof(ServiceImpl<,>));

            // Act
            var validator = container.GetInstance<IDoStuff<string>>();

            // Assert
            Assert.IsInstanceOfType(validator, typeof(DefaultStuff<string>));
            Assert.IsInstanceOfType(validator.Service, typeof(ServiceImpl<string, int>));
        }

        [TestMethod]
        public void GetInstance_CalledOnMultipleClosedImplementationsOfTypeRegisteredWithRegisterSingleOpenGeneric_Succeeds()
        {
            // Arrange
            var container = new Container();

            container.RegisterSingleOpenGeneric(typeof(IValidate<>), typeof(NullValidator<>));

            // Act
            container.GetInstance<IValidate<int>>();
            container.GetInstance<IValidate<double>>();
        }

        private sealed class ServiceImpl<TA, TB> : IService<TA, TB>
        {
        }

        private sealed class DefaultStuff<T> : IDoStuff<T>
        {
            public DefaultStuff(IService<T, int> service)
            {
                this.Service = service;
            }

            public IService<T, int> Service { get; private set; }
        }

        private sealed class NullValidator<T> : IValidate<T>
        {
            public void Validate(T instance)
            {
                // Do nothing.
            }
        }
    }
}