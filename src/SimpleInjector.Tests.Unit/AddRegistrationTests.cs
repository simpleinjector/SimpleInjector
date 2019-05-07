namespace SimpleInjector.Tests.Unit
{
    using System;
    using System.Collections.Generic;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SimpleInjector.Lifestyles;

    [TestClass]
    public class AddRegistrationTests
    {
        public interface IService1
        {
        }

        public interface IService2
        {
        }

        [TestMethod]
        public void GetInstanceOnTwoKeys_SameSingletonRegistrationForTwoKeys_ReturnsThatSameInstance()
        {
            // Arrange
            var container = ContainerFactory.New();

            var registration = Lifestyle.Singleton.CreateRegistration<Implementation>(container);

            container.AddRegistration(typeof(IService1), registration);
            container.AddRegistration(typeof(IService2), registration);

            // Act
            var instance1 = container.GetInstance<IService1>();
            var instance2 = container.GetInstance<IService2>();

            // Assert
            Assert.IsTrue(object.ReferenceEquals(instance1, instance2));
        }

        [TestMethod]
        public void AddRegistration_RegistrationFromAnotherContainer_FailsWithExpectedException()
        {
            var container = ContainerFactory.New();

            var otherContainer = ContainerFactory.New();

            var registrationFromAnotherContainer =
                Lifestyle.Singleton.CreateRegistration<Implementation>(otherContainer);

            // Act
            Action action =
                () => container.AddRegistration(typeof(IService1), registrationFromAnotherContainer);

            AssertThat.ThrowsWithExceptionMessageContains<ArgumentException>(
                "The supplied Registration belongs to a different Container", action);
        }

        [TestMethod]
        public void AddRegistration_SuppliedWithOpenGenericServiceType_ThrowsExpectedException()
        {
            // Arrange
            Container container = new Container();

            var registration = Lifestyle.Transient.CreateRegistration<StructCommandHandler>(container);

            // Act
            Action action = () => container.AddRegistration(typeof(ICommandHandler<>), registration);

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<ArgumentException>(
                "The supplied type ICommandHandler<TCommand> is an open-generic type.",
                action);
            AssertThat.ThrowsWithParamName("serviceType", action);
        }

        [TestMethod]
        public void AddRegistration_SuppliedWithPartialOpenGenericServiceType_ThrowsExpectedException()
        {
            // Arrange
            Container container = new Container();

            var registration = Lifestyle.Transient.CreateRegistration<StructCommandHandler>(container);

            // Act
            Action action = () => container.AddRegistration(
                typeof(ICommandHandler<>).MakeGenericType(typeof(List<>)),
                registration);

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<ArgumentException>(
                "The supplied type ICommandHandler<List<T>> is an open-generic type.",
                action);
            AssertThat.ThrowsWithParamName("serviceType", action);
        }

        [TestMethod]
        public void ScpopeDispose_RegistrationSetToSuppressDisposal_DisposesThatInstance()
        {
            // Arrange
            var container = new Container();
            container.Options.DefaultScopedLifestyle = new AsyncScopedLifestyle();

            var registration = Lifestyle.Scoped.CreateRegistration<DisposableImplementation>(container);

            registration.SuppressDisposal = true;

            container.AddRegistration<DisposableImplementation>(registration);

            DisposableImplementation instance;

            // Act
            using (AsyncScopedLifestyle.BeginScope(container))
            {
                instance = container.GetInstance<DisposableImplementation>();
            }

            // Assert
            Assert.IsFalse(instance.Disposed);
        }

        [TestMethod]
        public void ScpopeDispose_RegistrationFuncSetToSuppressDisposal_DisposesThatInstance()
        {
            // Arrange
            var container = new Container();
            container.Options.DefaultScopedLifestyle = new AsyncScopedLifestyle();

            var registration = Lifestyle.Scoped.CreateRegistration(
                () => new DisposableImplementation(),
                container);

            registration.SuppressDisposal = true;

            container.AddRegistration<DisposableImplementation>(registration);

            DisposableImplementation instance;

            // Act
            using (AsyncScopedLifestyle.BeginScope(container))
            {
                instance = container.GetInstance<DisposableImplementation>();
            }

            // Assert
            Assert.IsFalse(instance.Disposed);
        }

        [TestMethod]
        public void ContainerDispose_RegistrationSetToSuppressDisposal_DisposesThatInstance()
        {
            // Arrange
            var container = new Container();

            var registration = Lifestyle.Singleton.CreateRegistration<DisposableImplementation>(container);

            registration.SuppressDisposal = true;

            container.AddRegistration<DisposableImplementation>(registration);

            var instance = container.GetInstance<DisposableImplementation>();

            // Act
            container.Dispose();

            // Assert
            Assert.IsFalse(instance.Disposed);
        }

        [TestMethod]
        public void ContainerDispose_RegistrationFuncSetToSuppressDisposal_DisposesThatInstance()
        {
            // Arrange
            var container = new Container();

            var registration = Lifestyle.Singleton.CreateRegistration(
                () => new DisposableImplementation(),
                container);

            registration.SuppressDisposal = true;

            container.AddRegistration<DisposableImplementation>(registration);

            var instance = container.GetInstance<DisposableImplementation>();

            // Act
            container.Dispose();

            // Assert
            Assert.IsFalse(instance.Disposed);
        }

        public class Implementation : IService1, IService2
        {
        }

        public class DisposableImplementation : IService1, IDisposable
        {
            public bool Disposed { get; private set; }

            public void Dispose()
            {
                this.Disposed = true;
            }
        }
    }
}