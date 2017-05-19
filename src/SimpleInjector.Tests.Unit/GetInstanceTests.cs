namespace SimpleInjector.Tests.Unit
{
    using System;
    using System.Collections.Generic;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class GetInstanceTests
    {
        [TestMethod]
        public void GetInstanceByType_CalledOnUnregisteredInvalidServiceType_ThrowsActivationException()
        {
            // Arrange
            var container = ContainerFactory.New();

            // Act
            Action action = () => container.GetInstance(typeof(ServiceWithUnregisteredDependencies));

            // Assert
            AssertThat.Throws<ActivationException>(action);
        }

        [TestMethod]
        public void GetInstanceByType_CalledOnRegisteredButInvalidServiceType_ThrowsActivationException()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Register<ServiceWithUnregisteredDependencies>();

            // Act
            Action action = () => container.GetInstance(typeof(ServiceWithUnregisteredDependencies));

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<ActivationException>(@"
                The constructor of type ServiceWithUnregisteredDependencies contains the parameter with name
                'a' and type IDisposable that is not registered."
                .TrimInside(),
                action);
        }

        [TestMethod]
        public void GetInstanceByType_CalledOnUnregisteredConcreteButInvalidServiceType_ThrowsActivationException()
        {
            // Arrange
            var container = ContainerFactory.New();

            // Act
            Action action = () => container.GetInstance(typeof(ServiceWithUnregisteredDependencies));

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<ActivationException>(@"
                The constructor of type ServiceWithUnregisteredDependencies contains the parameter 
                with name 'a' and type IDisposable that is not registered."
                .TrimInside(),
                action);
        }

        [TestMethod]
        public void GetInstanceGeneric_CalledOnUnregisteredInvalidServiceType_ThrowsActivationException()
        {
            // Arrange
            var container = ContainerFactory.New();

            // Act
            Action action = () => container.GetInstance<ServiceWithUnregisteredDependencies>();

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<ActivationException>(@"
                The constructor of type ServiceWithUnregisteredDependencies contains the parameter 
                with name 'a' and type IDisposable that is not registered."
                .TrimInside(),
                action);
        }

        [TestMethod]
        public void GetInstanceGeneric_CalledOnRegisteredInvalidServiceType_ThrowsActivationException()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Register<ServiceWithUnregisteredDependencies>();

            // Act
            Action action = () => container.GetInstance<ServiceWithUnregisteredDependencies>();

            // Assert
            AssertThat.Throws<ActivationException>(action);
        }

        [TestMethod]
        public void GetInstance_OnObjectWhileUnregistered_ThrowsActivationException()
        {
            // Arrange
            var container = ContainerFactory.New();

            // Act
            Action action = () => container.GetInstance<object>();

            // Assert
            AssertThat.Throws<ActivationException>(action);
        }

        [TestMethod]
        public void GetInstanceType_DeeplyNestedGenericTypeWithInternalConstructor_ThrowsExceptionWithProperFriendlyTypeName()
        {
            // Arrange
            var container = ContainerFactory.New();

            // Act
            Action action =
                () => container.GetInstance(typeof(SomeGenericNastyness<>.ReadOnlyDictionary<,>.KeyCollection));

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<ArgumentException>(
                "GetInstanceTests.SomeGenericNastyness<TBla>.ReadOnlyDictionary<TKey, TValue>.KeyCollection",
                action);
        }

        [TestMethod]
        public void GetInstance_WithOpenGenericType_ThrowsExpectedException()
        {
            // Arrange
            var container = ContainerFactory.New();

            // Lazy<Func<TResult>>
            var nastyOpenGenericType = typeof(Lazy<>);

            // Act
            Action action = () => container.GetInstance(nastyOpenGenericType);

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<ArgumentException>(@"
                The request for type Lazy<T> is invalid because it is an open generic type: it is only 
                possible to instantiate instances of closed generic types. A generic type is closed if all of 
                its type parameters have been substituted with types that are recognized by the compiler."
                .TrimInside(),
                action);
        }

        [TestMethod]
        public void GetAllInstances_WithOpenGenericEnumerableType_ThrowsExpectedException()
        {
            // Arrange
            var container = ContainerFactory.New();

            // Act
            Action action = () => container.GetAllInstances(typeof(IEnumerable<>));

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<ArgumentException>(
                "The request for type IEnumerable<IEnumerable<T>> is invalid", action);
        }

        [TestMethod]
        public void GetInstance_RequestingAnPartiallyOpenGenericType_ThrowsExpectedException()
        {
            // Arrange
            var container = ContainerFactory.New();

            // Act
            Action action = () => container.GetInstance(typeof(ICollection<>).MakeGenericType(typeof(List<>)));

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<ArgumentException>(
                "The request for type ICollection<List<T>> is invalid",
                action);
        }

        [TestMethod]
        public void GetInstance_RegisteredConcreteTypeWithMissingDependency_DoesNotThrowMessageAboutMissingRegistration()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Register<FakeUserService>();

            // Act
            Action action = () => container.GetInstance<FakeUserService>();

            // Assert
            AssertThat.ThrowsWithExceptionMessageDoesNotContain<ActivationException>(
                "No registration for type FakeUserService could be found", action);
        }

        [TestMethod]
        public void GetInstance_RegisteredAbstractionWithImplementationWithMissingDependency_DoesNotThrowMessageAboutMissingRegistration()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Register<UserServiceBase, FakeUserService>();

            // Act
            Action action = () => container.GetInstance<UserServiceBase>();

            // Assert
            AssertThat.ThrowsWithExceptionMessageDoesNotContain<ActivationException>(
                "No registration for type FakeUserService could be found", action);
        }

        [TestMethod]
        public void GetInstance_NonRootTypeRegistrationWithMissingDependency_DoesNotThrowMessageAboutMissingRegistration()
        {
            // Arrange
            var container = ContainerFactory.New();

            // FakeUserService depends on IUserRepository
            container.Register<UserServiceBase, FakeUserService>();

            // SomeUserRepository depends on IPlugin, but that isn't registered
            container.Register<IUserRepository, PluginDependantUserRepository>();

            // Act
            Action action = () => container.GetInstance<UserServiceBase>();

            // Assert
            AssertThat.ThrowsWithExceptionMessageDoesNotContain<ActivationException>(
                "No registration for type PluginDependantUserRepository could be found", action);
        }

        [TestMethod]
        public void GetInstance_UnregisteredConcreteTypeWithMissingDependency_ThrowsExceptionAboutMissingRegistration()
        {
            // Arrange
            var container = ContainerFactory.New();

            // Act
            // FakeUserService depends on IUserRepository but this abstraction is not registered.
            Action action = () => container.GetInstance<FakeUserService>();

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<ActivationException>(@"
                No registration for type FakeUserService could be found and an implicit registration 
                could not be made."
                .TrimInside(),
                action);
        }

        [TestMethod]
        public void GetInstance_ResolvingAnUnregisteredDelegate_ThrowsExpectedException()
        {
            // Arrange
            var container = new Container();

            // Act
            Action action = () => container.GetInstance<Func<object>>();

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<ActivationException>(
                "No registration for type Func<Object> could be found.",
                action);
        }

        [TestMethod]
        public void GetInstance_ResolvingConcreteTypeWithMissingDependencyOnEmptyContainer_ThrowsMessageWarningAboutEmptyContainer()
        {
            // Arrange
            var container = new Container();

            // Act
            Action action = () => container.GetInstance<ServiceWithDependency<ILogger>>();

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<ActivationException>(
                "Please note that the container instance you are resolving from contains no registrations.",
                action);
        }

        [TestMethod]
        public void GetInstance_ResolvingConcreteTypeWithMissingDependencyOnNoneEmptyContainer_ThrowsMessageWarningWITHOUTAboutEmptyContainer()
        {
            // Arrange
            var container = new Container();

            container.Register<UserController>();

            // Act
            Action action = () => container.GetInstance<ServiceWithDependency<ILogger>>();

            // Assert
            AssertThat.ThrowsWithExceptionMessageDoesNotContain<ActivationException>(
                "Please note that the container instance you are resolving from contains no registrations.",
                action);
        }

        [TestMethod]
        public void GetInstance_ResolvingAbstractTypeWithMissingDependencyOnEmptyContainer_ThrowsMessageWarningAboutEmptyContainer()
        {
            // Arrange
            var container = new Container();

            // Act
            Action action = () => container.GetInstance<ILogger>();

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<ActivationException>(
                "Please note that the container instance you are resolving from contains no registrations.",
                action);
        }

        [TestMethod]
        public void GetInstance_ResolvingConcreteTypeAfterGettingTheRegistrationWithMissingDependencyOnEmptyContainer_ThrowsMessageWarningAboutEmptyContainer()
        {
            // Arrange
            var container = new Container();

            container.GetRegistration(typeof(ServiceWithDependency<ILogger>));

            // Act
            Action action = () => container.GetInstance<ServiceWithDependency<ILogger>>();

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<ActivationException>(
                "Please note that the container instance you are resolving from contains no registrations.",
                action);
        }

        [TestMethod]
        public void GetInstance_ResolvingAbstractTypeAfterGettingTheRegistrationWithMissingDependencyOnEmptyContainer_ThrowsMessageWarningAboutEmptyContainer()
        {
            // Arrange
            var container = new Container();

            container.GetRegistration(typeof(ILogger));

            // Act
            Action action = () => container.GetInstance<ILogger>();

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<ActivationException>(
                "Please note that the container instance you are resolving from contains no registrations.",
                action);
        }

        [TestMethod]
        public void GetInstance_NoRegistrationExistsButRegistrationForCollectionDoes_ThrowsExceptionReferingToThatCollectionRegistration()
        {
            // Arrange
            var container = new Container();

            container.RegisterCollection(typeof(ILogger), new[] { typeof(NullLogger) });

            // Act
            Action action = () => container.GetInstance<ILogger>();

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<ActivationException>(@"
                No registration for type ILogger could be found. 
                There is, however, a registration for IEnumerable<ILogger>;
                Did you mean to call GetAllInstances<ILogger>() or depend on IEnumerable<ILogger>?"
                .TrimInside(),
                action);
        }

        [TestMethod]
        public void GetInstance_ResolvingATypeDependingOnAnUnregisteredTypeWhileACollectionRegistrationExists_ThrowsExceptionReferingToThatRegistration()
        {
            // Arrange
            var container = new Container();

            container.RegisterCollection(typeof(ILogger), new[] { typeof(NullLogger) });

            // Act
            Action action = () => container.GetInstance<ComponentDependingOn<ILogger>>();

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<ActivationException>(@"
                There is, however, a registration for IEnumerable<ILogger>; 
                Did you mean to depend on IEnumerable<ILogger>?
                If you meant to depend on ILogger, 
                use should use one of the Register overloads instead of using RegisterCollection"
                .TrimInside(),
                action);
        }

        [TestMethod]
        public void GetInstance_NoRegistrationFExistsAndNoCollectionRegistrationEither_ExceptionMessageDoesNotReferToOtherRegistrations()
        {
            // Arrange
            var container = new Container();

            // Act
            Action action = () => container.GetInstance<ILogger>();

            // Assert
            AssertThat.ThrowsWithExceptionMessageDoesNotContain<ActivationException>(
                "GetInstance<ILogger>()",
                action);

            AssertThat.ThrowsWithExceptionMessageDoesNotContain<ActivationException>(
                "GetAllInstances<ILogger>()",
                action);
        }

        [TestMethod]
        public void GetAllInstances_NoRegistrationForCollectionButRegistrationForSingleElementExists_ThrowsExceptionReferingToThatRegistration()
        {
            // Arrange
            var container = new Container();

            container.Register<ILogger, NullLogger>();

            // Act
            Action action = () => container.GetAllInstances<ILogger>();

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<ActivationException>(@"
                No registration for type IEnumerable<ILogger> could be found.
                There is, however, a registration for ILogger; 
                Did you mean to call GetInstance<ILogger>() or depend on ILogger?"
                .TrimInside(),
                action);
        }

        [TestMethod]
        public void GetInstance_ResolvingATypeDependingOnAnUnregisteredCollectionWhileASingleRegistrationExists_ThrowsExceptionReferingToThatRegistration()
        {
            // Arrange
            var container = new Container();

            container.Register<ILogger, NullLogger>();

            // Act
            Action action = () => container.GetInstance<ComponentDependingOn<IEnumerable<ILogger>>>();

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<ActivationException>(
                "There is, however, a registration for ILogger; Did you mean to depend on ILogger?",
                action);
        }

        [TestMethod]
        public void GetAllInstances_NoRegistrationForCollectionAndNoSingleRegistrationEither_ExceptionMessageDoesNotReferToOtherRegistrations()
        {
            // Arrange
            var container = new Container();

            // Act
            Action action = () => container.GetAllInstances<ILogger>();

            // Assert
            AssertThat.ThrowsWithExceptionMessageDoesNotContain<ActivationException>(
                "Did you mean to call GetInstance<ILogger>()",
                action);

            AssertThat.ThrowsWithExceptionMessageDoesNotContain<ActivationException>(
                "GetAllInstances<ILogger>()",
                action);
        }

        [TestMethod]
        public void GetInstance_OnMissingTypeWithNoLookalike_DoesNotReportLookalikes()
        {
            // Arrange
            var container = new Container();

            // Act
            Action action = () => container.GetInstance<IDuplicate>();

            // Assert
            AssertThat.ThrowsWithExceptionMessageDoesNotContain<ActivationException>(
                "Note that there exists a registration for a different type",
                action);
        }

        [TestMethod]
        public void GetInstance_OnMissingConstructorDependencyWithNoLookalike_DoesNotReportLookalikes()
        {
            // Arrange
            var container = new Container();

            // Act
            Action action = () => container.GetInstance<ServiceDependingOn<IDuplicate>>();

            // Assert
            AssertThat.ThrowsWithExceptionMessageDoesNotContain<ActivationException>(
                "Note that there exists a registration for a different type",
                action);
        }

        [TestMethod]
        public void GetInstance_OnMissingTypeWithLookalikeAsExternalProducer_DoesNotReportLookalikes()
        {
            // Arrange
            var container = new Container();

            var externalProducer = Lifestyle.Transient.CreateProducer<IDuplicate, Duplicate>(container);

            // Act
            Action action = () => container.GetInstance<ServiceDependingOn<IDuplicate>>();

            // Assert
            // This exception is not expected to be thrown, because external producers are not registered
            // in the container and can not be resolved from the container. Reporting them would be confusing.
            AssertThat.ThrowsWithExceptionMessageDoesNotContain<ActivationException>(
                "Note that there exists a registration for a different type",
                action);

            GC.KeepAlive(externalProducer);
        }

        [TestMethod]
        public void GetInstance_ConsumerDependingOnConditionalRegistrationsThatDoNotGetInjected_DoesNotReportLookalikes()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.RegisterConditional(typeof(ILogger), typeof(NullLogger), Lifestyle.Singleton, c => false);
            container.RegisterConditional(typeof(ILogger), typeof(ConsoleLogger), Lifestyle.Singleton, c => false);

            // Act
            Action action = () => container.GetInstance<ServiceWithDependency<ILogger>>();

            // Assert
            AssertThat.ThrowsWithExceptionMessageDoesNotContain<ActivationException>(
                "Note that there exists a registration for a different type",
                action);
        }

        [TestMethod]
        public void GetInstance_OnMissingTypeWithExistingLookalike_WarnsAboutThisLookalike()
        {
            // Arrange
            var container = new Container();

            container.Register<IDuplicate, Duplicate>();

            // Act
            Action action = () => container.GetInstance<SimpleInjector.Tests.Unit.Duplicates.IDuplicate>();

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<ActivationException>(@"
                Note that there exists a registration for a different type 
                SimpleInjector.Tests.Unit.IDuplicate while the requested type is 
                SimpleInjector.Tests.Unit.Duplicates.IDuplicate."
                .TrimInside(),
                action);
        }

        [TestMethod]
        public void GetInstance_MissingConstructorDependencyWithExistingLookalike_WarnsAboutThisLookalike()
        {
            // Arrange
            var container = new Container();

            container.Register<IDuplicate, Duplicate>();

            // Act
            Action action = () => container.GetInstance<ServiceDependingOn<SimpleInjector.Tests.Unit.Duplicates.IDuplicate>>();

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<ActivationException>(@"
                Note that there exists a registration for a different type
                SimpleInjector.Tests.Unit.IDuplicate"
                .TrimInside(),
                action);
        }

        [TestMethod]
        public void GetInstance_OnMissingConstructorDependencyWithExistingOpenGenericLookalike_WarnsAboutThisLookalike()
        {
            // Arrange
            var container = new Container();

            container.Register(typeof(IDuplicate<>), typeof(Duplicate<>));

            // Act
            Action action = () => 
                container.GetInstance<ServiceDependingOn<SimpleInjector.Tests.Unit.Duplicates.IDuplicate<object>>>();

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<ActivationException>(@"
                Note that there exists a registration for a different type
                SimpleInjector.Tests.Unit.IDuplicate<T>"
                .TrimInside(),
                action);
        }

        //// Seems like there are tests missing, but all other cases are already covered by other test classes.

        public class SomeGenericNastyness<TBla>
        {
            public class ReadOnlyDictionary<TKey, TValue>
            {
                public sealed class KeyCollection
                {
                    internal KeyCollection(ICollection<TKey> collection)
                    {
                    }
                }
            }
        }
    }
}