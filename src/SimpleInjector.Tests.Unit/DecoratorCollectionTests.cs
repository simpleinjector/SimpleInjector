namespace SimpleInjector.Tests.Unit
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Linq.Expressions;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SimpleInjector.Advanced;

    /// <summary>
    /// This set of tests test whether individual items of registered collections are correctly decorated.
    /// </summary>
    [TestClass]
    public class DecoratorCollectionTests
    {
        public interface IBase
        {
        }

        public interface IDerive : IBase
        {
        }

        public class DeriveImplementation : IDerive
        {
        }

        [TestMethod]
        public void GetAllInstances_TypeDecorated1_ReturnsCollectionWithDecorators()
        {
            // Arrange
            var container = ContainerFactory.New();

            // Use the RegisterAll(Type, Type[]) overload.
            container.RegisterCollection(typeof(ICommandHandler<RealCommand>), new[] { typeof(RealCommandHandler) });

            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(RealCommandHandlerDecorator));

            // Act
            var handlers = container.GetAllInstances<ICommandHandler<RealCommand>>();

            var handler = handlers.Single();

            // Assert
            AssertThat.IsInstanceOfType(typeof(RealCommandHandlerDecorator), handler);

            AssertThat.IsInstanceOfType(typeof(RealCommandHandler), ((RealCommandHandlerDecorator)handler).Decorated);
        }

        [TestMethod]
        public void GetAllInstances_TypeDecorated2_ReturnsCollectionWithDecorators()
        {
            // Arrange
            var container = ContainerFactory.New();

            var expectedSingletonHandler = new RealCommandHandler();

            // Use the RegisterAll<T>(T[]) overload.
            container.RegisterCollection<ICommandHandler<RealCommand>>(expectedSingletonHandler);

            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(RealCommandHandlerDecorator));

            // Act
            var handlers = container.GetAllInstances<ICommandHandler<RealCommand>>();

            var decorator = handlers.Single();

            // Assert
            AssertThat.IsInstanceOfType(typeof(RealCommandHandlerDecorator), decorator);

            Assert.IsTrue(object.ReferenceEquals(expectedSingletonHandler,
                ((RealCommandHandlerDecorator)decorator).Decorated));
        }

        [TestMethod]
        public void GetAllInstances_TypeDecorated3_ReturnsCollectionWithDecorators()
        {
            // Arrange
            var container = ContainerFactory.New();

            var expectedSingletonHandler = new RealCommandHandler();

            // Use the RegisterAll(Type, IEnumerable) overload.
            container.RegisterCollection(typeof(ICommandHandler<RealCommand>), new[] { expectedSingletonHandler });

            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(RealCommandHandlerDecorator));

            // Act
            var handlers = container.GetAllInstances<ICommandHandler<RealCommand>>();

            var decorator = handlers.Single();

            // Assert
            AssertThat.IsInstanceOfType(typeof(RealCommandHandlerDecorator), decorator);

            Assert.IsTrue(object.ReferenceEquals(expectedSingletonHandler,
                ((RealCommandHandlerDecorator)decorator).Decorated));
        }

        [TestMethod]
        public void GetAllInstances_TypeDecorated4_ReturnsCollectionWithDecorators()
        {
            // Arrange
            var container = ContainerFactory.New();

            var expectedSingletonHandler = new RealCommandHandler();

            // Use the RegisterSingle<IEnumerable<T>> method.
            container.RegisterSingleton<IEnumerable<ICommandHandler<RealCommand>>>(new[] { expectedSingletonHandler });

            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(RealCommandHandlerDecorator));

            // Act
            var handlers = container.GetAllInstances<ICommandHandler<RealCommand>>();

            var decorator = handlers.Single();

            // Assert
            AssertThat.IsInstanceOfType(typeof(RealCommandHandlerDecorator), decorator);

            var decoratedInstance = ((RealCommandHandlerDecorator)decorator).Decorated;

            Assert.IsNotNull(decoratedInstance);

            Assert.IsTrue(object.ReferenceEquals(expectedSingletonHandler, decoratedInstance),
                "Not the same instance. Decorated instance is instance of type: " +
                decoratedInstance.GetType().Name);
        }

        [TestMethod]
        public void GetAllInstances_RegistrationThatAlwaysReturnsANewCollectionAndDecorator_ReturnsTransientInstances()
        {
            // Arrange
            var container = ContainerFactory.New();

            // Use the Register<T>(Func<T>) method. This is a strange (not adviced), but valid way of 
            // registering collections.
            container.Register<IEnumerable<ICommandHandler<RealCommand>>>(
                () => new[] { new RealCommandHandler() });

            container.RegisterDecorator(
                typeof(ICommandHandler<>),
                typeof(RealCommandHandlerDecorator));

            // Act
            var handlers1 = container.GetAllInstances<ICommandHandler<RealCommand>>();
            var handlers2 = container.GetAllInstances<ICommandHandler<RealCommand>>();

            var decorator1 = (RealCommandHandlerDecorator)handlers1.Single();
            var decorator2 = (RealCommandHandlerDecorator)handlers2.Single();

            // Assert
            bool isTransient = !object.ReferenceEquals(decorator1.Decorated, decorator2.Decorated);

            Assert.IsTrue(isTransient,
                "Since the registration returns a new collection with new instances, the decorators are " +
                "expected to be wrapped around those new instances, and not caching the collection that " +
                "is returned first.");
        }

        [TestMethod]
        public void GetAllInstances_RegistrationThatAlwaysReturnsANewCollectionAndSingletonDecorator_ReturnsSingletons()
        {
            // Arrange
            var container = ContainerFactory.New();

            // Use the Register<T>(Func<T>) method. This is a strange (not adviced), but valid way of 
            // registering collections.
            container.Register<IEnumerable<ICommandHandler<RealCommand>>>(
                () => new[] { new RealCommandHandler() });

            container.RegisterDecorator(
                typeof(ICommandHandler<>),
                typeof(RealCommandHandlerDecorator),
                Lifestyle.Singleton);

            // Act
            var handlers1 = container.GetAllInstances<ICommandHandler<RealCommand>>();
            var handlers2 = container.GetAllInstances<ICommandHandler<RealCommand>>();

            var decorator1 = (RealCommandHandlerDecorator)handlers1.Single();
            var decorator2 = (RealCommandHandlerDecorator)handlers2.Single();

            // Assert
            bool isSingleton = object.ReferenceEquals(decorator1, decorator2);

            Assert.IsTrue(isSingleton,
                "Since the decorator is registered as singleton, is should be returned as singleton, no " +
                "matter how the collection is registered (as Register<T>(Func<T>) in this case).");
        }

        [TestMethod]
        public void GetAllInstances_TypeDecoratedWithMultipleDecorators_ReturnsCollectionWithDecorators()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.RegisterCollection(typeof(ICommandHandler<RealCommand>), new[] { typeof(RealCommandHandler) });

            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(TransactionalCommandHandlerDecorator<>));

            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(RealCommandHandlerDecorator));

            // Act
            var handlers = container.GetAllInstances<ICommandHandler<RealCommand>>();

            var handler = handlers.Single();

            // Assert
            AssertThat.IsInstanceOfType(typeof(RealCommandHandlerDecorator), handler);

            AssertThat.IsInstanceOfType(typeof(TransactionalCommandHandlerDecorator<RealCommand>), ((RealCommandHandlerDecorator)handler).Decorated);
        }

        [TestMethod]
        public void GetAllInstances_TypeDecoratedWithTransientDecorator_ReturnsANewInstanceEveryTime()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.RegisterCollection(typeof(ICommandHandler<RealCommand>), new[] { typeof(RealCommandHandler) });

            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(RealCommandHandlerDecorator));

            // Act
            IEnumerable<ICommandHandler<RealCommand>> handlers =
                container.GetAllInstances<ICommandHandler<RealCommand>>();

            var handler1 = handlers.Single();
            var handler2 = handlers.Single();

            // Assert
            AssertThat.IsInstanceOfType(typeof(RealCommandHandlerDecorator), handler1);

            Assert.IsFalse(object.ReferenceEquals(handler1, handler2),
                "Since the decorator is registered as transient, every time the collection is iterated, " +
                "a new instance should be created.");
        }

        [TestMethod]
        public void GetAllInstances_TypeDecoratedWithFuncDecorator1_InjectsADelegateThatCanCreateThatInstance()
        {
            // Arrange
            var container = ContainerFactory.New();

            // Use the RegisterAll(Type, Type[]) overload.
            container.RegisterCollection(typeof(ICommandHandler<RealCommand>), new[] 
            { 
                typeof(RealCommandHandler),
            });

            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(AsyncCommandHandlerProxy<>));

            // Act
            var handlers = container.GetAllInstances<ICommandHandler<RealCommand>>();

            var decorator = (AsyncCommandHandlerProxy<RealCommand>)handlers.Single();

            var handler = decorator.DecorateeFactory();

            // Assert
            AssertThat.IsInstanceOfType(typeof(RealCommandHandler), handler);
        }

        [TestMethod]
        public void GetAllInstances_TypeDecoratedWithFuncDecorator2_InjectsADelegateThatCanCreateThatInstance()
        {
            // Arrange
            var container = ContainerFactory.New();

            var expectedSingletonHandler = new RealCommandHandler();

            // Use the RegisterAll<T>(T[]) overload.
            container.RegisterCollection<ICommandHandler<RealCommand>>(expectedSingletonHandler);

            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(AsyncCommandHandlerProxy<>));

            // Act
            var handlers = container.GetAllInstances<ICommandHandler<RealCommand>>();

            var decorator = (AsyncCommandHandlerProxy<RealCommand>)handlers.Single();

            var handler = decorator.DecorateeFactory();

            // Assert
            AssertThat.IsInstanceOfType(typeof(RealCommandHandler), handler);
        }

        [TestMethod]
        public void GetAllInstances_CollectionManuallyRegisteredAndFuncDecoraterRegistered1_ThrowsExpectedException()
        {
            // Arrange
            var container = ContainerFactory.New();

            var expectedSingletonHandler = new RealCommandHandler();

            // Use the RegisterAll(Type, IEnumerable) overload.
            container.RegisterCollection(typeof(ICommandHandler<RealCommand>), new[] { expectedSingletonHandler });

            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(AsyncCommandHandlerProxy<>));

            try
            {
                // Act
                container.GetAllInstances<ICommandHandler<RealCommand>>();

                // Assert
                Assert.Fail("Exception expected.");
            }
            catch (ActivationException ex)
            {
                Assert_ExceptionContainsInfoAboutManualCollectionRegistrationMixedDecoratorsThatTakeAFunc(ex);
            }
        }

        [TestMethod]
        public void GetAllInstances_CollectionManuallyRegisteredAndFuncDecoraterRegistered2_ThrowsExpectedException()
        {
            // Arrange
            var container = ContainerFactory.New();

            var expectedSingletonHandler = new RealCommandHandler();

            // Use the RegisterSingle<IEnumerable<T>> method
            container.RegisterSingleton<IEnumerable<ICommandHandler<RealCommand>>>(new[] { expectedSingletonHandler });

            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(AsyncCommandHandlerProxy<>));

            try
            {
                // Act
                container.GetAllInstances<ICommandHandler<RealCommand>>();

                // Assert
                Assert.Fail("Exception expected.");
            }
            catch (ActivationException ex)
            {
                Assert_ExceptionContainsInfoAboutManualCollectionRegistrationMixedDecoratorsThatTakeAFunc(ex);
            }
        }

        [TestMethod]
        public void GetAllInstances_CollectionManuallyRegisteredAndFuncDecoraterRegistered4_ThrowsExpectedException()
        {
            // Arrange
            var container = ContainerFactory.New();

            var expectedSingletonHandler = new RealCommandHandler();

            // Use the Register<T>(Func<T>) method. This is a strange (but legal) way of registering a service,
            // but will not work with a Func-Decorator.
            container.Register<IEnumerable<ICommandHandler<RealCommand>>>(() => new[] { expectedSingletonHandler });

            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(AsyncCommandHandlerProxy<>));

            try
            {
                // Act
                container.GetAllInstances<ICommandHandler<RealCommand>>();

                // Assert
                Assert.Fail("Exception expected.");
            }
            catch (ActivationException ex)
            {
                Assert_ExceptionContainsInfoAboutManualCollectionRegistrationMixedDecoratorsThatTakeAFunc(ex);
            }
        }

        [TestMethod]
        public void GetAllInstances_TypeDecoratedWithFuncDecorator_InjectsADelegateThatReturnsATransientInstance()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.RegisterCollection(typeof(ICommandHandler<RealCommand>), new[] 
            { 
                typeof(RealCommandHandler),
            });

            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(AsyncCommandHandlerProxy<>));

            // Act
            var handlers = container.GetAllInstances<ICommandHandler<RealCommand>>();

            var decorator = (AsyncCommandHandlerProxy<RealCommand>)handlers.Single();

            var handler1 = decorator.DecorateeFactory();
            var handler2 = decorator.DecorateeFactory();

            // Assert
            Assert.IsFalse(object.ReferenceEquals(handler1, handler2), "The injected Func<T> should create " +
                "a transient, sine that's how the StubCommandHandler is registered.");
        }

        [TestMethod]
        public void GetAllInstances_DecoratorDecoratedWithFuncDecorator_InjectsADelegateThatCanCreateThatDecorator()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.RegisterCollection(typeof(ICommandHandler<RealCommand>), new[] 
            { 
                typeof(RealCommandHandler),
            });

            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(TransactionalCommandHandlerDecorator<>));
            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(AsyncCommandHandlerProxy<>));

            // Act
            var handlers = container.GetAllInstances<ICommandHandler<RealCommand>>();

            var funcDecorator = (AsyncCommandHandlerProxy<RealCommand>)handlers.Single();

            var decorator = funcDecorator.DecorateeFactory();

            // Assert
            AssertThat.IsInstanceOfType(typeof(TransactionalCommandHandlerDecorator<RealCommand>), decorator);

            var handler = ((TransactionalCommandHandlerDecorator<RealCommand>)decorator).Decorated;

            AssertThat.IsInstanceOfType(typeof(RealCommandHandler), handler);
        }

        [TestMethod]
        public void GetAllInstances_NestedFuncDecorators_GetInjectedAsExpected()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.RegisterCollection(typeof(ICommandHandler<RealCommand>), new[] 
            { 
                typeof(RealCommandHandler),
            });

            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(LifetimeScopeCommandHandlerProxy<>));
            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(AsyncCommandHandlerProxy<>));

            // Act
            var handlers = container.GetAllInstances<ICommandHandler<RealCommand>>();

            var asyncDecorator = (AsyncCommandHandlerProxy<RealCommand>)handlers.Single();

            var scopeDecorator = (LifetimeScopeCommandHandlerProxy<RealCommand>)asyncDecorator.DecorateeFactory();

            var handler = scopeDecorator.DecorateeFactory();

            // Assert
            AssertThat.IsInstanceOfType(typeof(RealCommandHandler), handler);
        }

        [TestMethod]
        public void GetAllInstances_FuncDecoratorDecoratedByANormalDecorator_GetInjectedAsExpected()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.RegisterCollection(typeof(ICommandHandler<RealCommand>), new[] 
            { 
                typeof(RealCommandHandler),
            });

            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(AsyncCommandHandlerProxy<>));
            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(TransactionalCommandHandlerDecorator<>));

            // Act
            var handlers = container.GetAllInstances<ICommandHandler<RealCommand>>();

            var decorator = (TransactionalCommandHandlerDecorator<RealCommand>)handlers.Single();

            // Assert
            AssertThat.IsInstanceOfType(typeof(AsyncCommandHandlerProxy<RealCommand>), decorator.Decorated);
        }

        [TestMethod]
        public void GetAllInstances_TypeRegisteredWithRegisterSingleDecorator_AlwaysReturnsTheSameInstance()
        {
            // Arrange
            var container = ContainerFactory.New();
            
            // Register transient service
            // This is not good practice, since we register a singleton decorator, but just for testing.
            container.RegisterCollection<INonGenericService>(new[] { typeof(RealNonGenericService) });

            container.Register<RealNonGenericService>(Lifestyle.Singleton);

            container.RegisterDecorator(typeof(INonGenericService), typeof(NonGenericServiceDecorator), Lifestyle.Singleton);

            // Act
            IEnumerable<INonGenericService> services = container.GetAllInstances<INonGenericService>();

            var decorator1 = services.Single();
            var decorator2 = services.Single();

            // Assert
            AssertThat.IsInstanceOfType(typeof(NonGenericServiceDecorator), decorator1);

            Assert.IsTrue(object.ReferenceEquals(decorator1, decorator2),
                "Since the decorator is registered as singleton, the enumerable should always return the " +
                "same instance.");
        }

        [TestMethod]
        public void GetAllInstances_TypeRegisteredWithRegisterSingleFuncDecorator_AlwaysReturnsTheSameInstance()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.RegisterCollection(typeof(ICommandHandler<RealCommand>), new[] 
            { 
                typeof(RealCommandHandler),
            });

            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(AsyncCommandHandlerProxy<>),
                Lifestyle.Singleton);

            // Act
            IEnumerable<ICommandHandler<RealCommand>> handlers =
                container.GetAllInstances<ICommandHandler<RealCommand>>();

            var decorator1 = handlers.Single();
            var decorator2 = handlers.Single();

            // Assert
            AssertThat.IsInstanceOfType(typeof(AsyncCommandHandlerProxy<RealCommand>), decorator1);

            Assert.IsTrue(object.ReferenceEquals(decorator1, decorator2),
                "Since the decorator is registered as singleton, the enumerable should always return the " +
                "same instance.");
        }

        [TestMethod]
        public void GetAllInstances_SingleFuncDecoratorDecoratedWithTransientDecorator_WorksAsExpected()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.RegisterCollection(typeof(ICommandHandler<RealCommand>), new[] 
            { 
                typeof(RealCommandHandler),
            });

            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(AsyncCommandHandlerProxy<>), Lifestyle.Singleton);
            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(TransactionalCommandHandlerDecorator<>));

            // Act
            var handlers = container.GetAllInstances<ICommandHandler<RealCommand>>();

            var stubDecorator1 = (TransactionalCommandHandlerDecorator<RealCommand>)handlers.Single();
            var stubDecorator2 = (TransactionalCommandHandlerDecorator<RealCommand>)handlers.Single();

            var asyncDecorator1 = (AsyncCommandHandlerProxy<RealCommand>)stubDecorator1.Decorated;
            var asyncDecorator2 = (AsyncCommandHandlerProxy<RealCommand>)stubDecorator1.Decorated;

            // Assert
            Assert.IsFalse(object.ReferenceEquals(stubDecorator1, stubDecorator2),
                "StubDecorator1 is registered as transient.");

            Assert.IsTrue(object.ReferenceEquals(asyncDecorator1, asyncDecorator2),
                "AsyncCommandHandlerProxy is registered as singleton.");
        }

        [TestMethod]
        public void GetAllInstances_RegisteredCollectionContainerBothTransientAsSingletons_ResolvesTransientsCorrectly()
        {
            // Arrange
            var container = ContainerFactory.New();

            // Register the NullCommandHandler<RealCommand> as singleton.
            // We do this using the RegisterSingleOpenGeneric, but this is not important for this test.
            container.Register(typeof(ICommandHandler<>), typeof(NullCommandHandler<>), Lifestyle.Singleton);

            // Collection that returns both a transient (RealCommandCommandHandler) and singleton.
            container.RegisterCollection<ICommandHandler<RealCommand>>(new[] 
            { 
                typeof(RealCommandHandler),
                typeof(NullCommandHandler<RealCommand>)
            });

            container.RegisterDecorator(
                typeof(ICommandHandler<>),
                typeof(RealCommandHandlerDecorator));

            // Act
            var handlers = container.GetAllInstances<ICommandHandler<RealCommand>>();

            var realHandler1 = ((RealCommandHandlerDecorator)handlers.First()).Decorated;
            var realHandler2 = ((RealCommandHandlerDecorator)handlers.First()).Decorated;

            // Assert
            AssertThat.IsInstanceOfType(typeof(RealCommandHandler), realHandler1);

            bool isTransient = !object.ReferenceEquals(realHandler1, realHandler2);

            Assert.IsTrue(isTransient,
                "The RealCommandCommandHandler is registered as transient and should be injected as transient.");
        }

        [TestMethod]
        public void GetAllInstances_RegisteredCollectionContainerBothTransientAsSingletons_ResolvesSingletonsCorrectly()
        {
            // Arrange
            var container = ContainerFactory.New();

            // Register the NullCommandHandler<RealCommand> as singleton.
            container.Register<NullCommandHandler<RealCommand>>(Lifestyle.Singleton);

            // Collection that returns both a transient (RealCommandCommandHandler) and singleton.
            container.RegisterCollection<ICommandHandler<RealCommand>>(new[]
            {
                typeof(RealCommandHandler),
                typeof(NullCommandHandler<RealCommand>)
            });

            container.RegisterDecorator(
                typeof(ICommandHandler<>),
                typeof(RealCommandHandlerDecorator));

            // Act
            var handlers1 = container.GetAllInstances<ICommandHandler<RealCommand>>().ToArray();
            var handlers2 = container.GetAllInstances<ICommandHandler<RealCommand>>().ToArray();

            var nullHandler1 = ((RealCommandHandlerDecorator)handlers1.Last()).Decorated;
            var nullHandler2 = ((RealCommandHandlerDecorator)handlers2.Last()).Decorated;

            // Assert
            AssertThat.IsInstanceOfType(typeof(NullCommandHandler<RealCommand>), nullHandler1);

            Assert.AreSame(nullHandler1, nullHandler2,
                "The NullCommandHandler is registered as singleton and should be injected as singleton.");
        }

        [TestMethod]
        public void GetAllInstances_CollectionDecoratedWithSingletonDecorator1_WillNotReturnAMutableType()
        {
            // Arrange
            var container = ContainerFactory.New();

            // Use the RegisterAll<T>(Type[]) overload.
            container.RegisterCollection<ICommandHandler<RealCommand>>(new[] { typeof(RealCommandHandler) });

            container.RegisterDecorator(
                typeof(ICommandHandler<>),
                typeof(TransactionalCommandHandlerDecorator<>),
                Lifestyle.Singleton);

            // Act
            var handlers = container.GetAllInstances<ICommandHandler<RealCommand>>();

            // Assert
            // Especially when registering singleton decorators, it is important that the returned collection
            // is not mutable, since changes to this collection could effect the whole application.
            Assert_IsNotAMutableCollection(handlers);
        }

        [TestMethod]
        public void GetAllInstances_CollectionDecoratedWithSingletonDecorator2_WillNotReturnAMutableType()
        {
            // Arrange
            var container = ContainerFactory.New();

            // Use the RegisterAll<T>(T[]) overload.
            container.RegisterCollection<ICommandHandler<RealCommand>>(new[] { new RealCommandHandler() });

            container.RegisterDecorator(
                typeof(ICommandHandler<>),
                typeof(TransactionalCommandHandlerDecorator<>),
                Lifestyle.Singleton);

            // Act
            var handlers = container.GetAllInstances<ICommandHandler<RealCommand>>();

            // Assert
            Assert_IsNotAMutableCollection(handlers);
        }

        [TestMethod]
        public void GetAllInstances_CollectionDecoratedWithSingletonDecorator3_WillNotReturnAMutableType()
        {
            // Arrange
            var container = ContainerFactory.New();

            IEnumerable<ICommandHandler<RealCommand>> handlers = new[] { new RealCommandHandler() };

            // Use the RegisterAll<T>(IEnumerable<T>) overload.
            container.RegisterCollection<ICommandHandler<RealCommand>>(handlers);

            container.RegisterDecorator(
                typeof(ICommandHandler<>),
                typeof(TransactionalCommandHandlerDecorator<>),
                Lifestyle.Singleton);

            // Act
            var actualHandlers = container.GetAllInstances<ICommandHandler<RealCommand>>();

            // Assert
            Assert_IsNotAMutableCollection(actualHandlers);
        }

        [TestMethod]
        public void GetAllInstances_CollectionDecoratedWithSingletonDecorator4_WillNotReturnAMutableType()
        {
            // Arrange
            var container = ContainerFactory.New();

            // Use the Register<T>(Func<T>) overload.
            container.Register<IEnumerable<ICommandHandler<RealCommand>>>(
                () => new[] { new RealCommandHandler() });

            container.RegisterDecorator(
                typeof(ICommandHandler<>),
                typeof(TransactionalCommandHandlerDecorator<>),
                Lifestyle.Singleton);

            // Act
            var handlers = container.GetAllInstances<ICommandHandler<RealCommand>>();

            // Assert
            Assert_IsNotAMutableCollection(handlers);
        }

        [TestMethod]
        public void GetAllInstances_DecoratorRegisteredWithPredicate_DecoratesInstancesThatShouldBeDecorated()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.RegisterCollection<ICommandHandler<RealCommand>>(new[] 
            { 
                typeof(RealCommandHandler),
                typeof(NullCommandHandler<RealCommand>)
            });

            container.RegisterDecorator(
                typeof(ICommandHandler<>),
                typeof(TransactionalCommandHandlerDecorator<>),
                c => c.ImplementationType == typeof(RealCommandHandler));

            // Act
            var handlers = container.GetAllInstances<ICommandHandler<RealCommand>>();

            var handler = handlers.First();

            // Assert
            bool realCommandHandlerIsDecorated =
                handler.GetType() == typeof(TransactionalCommandHandlerDecorator<RealCommand>);

            Assert.IsTrue(realCommandHandlerIsDecorated);
        }

        [TestMethod]
        public void GetAllInstances_DecoratorRegisteredWithPredicate_DoesNotDecoratesInstancesThatShouldNotBeDecorated()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.RegisterCollection<ICommandHandler<RealCommand>>(new[] 
            {
                typeof(RealCommandHandler),
                typeof(NullCommandHandler<RealCommand>)
            });

            container.RegisterDecorator(
                typeof(ICommandHandler<>),
                typeof(TransactionalCommandHandlerDecorator<>),
                c => c.ImplementationType == typeof(RealCommandHandler));

            // Act
            var handlers = container.GetAllInstances<ICommandHandler<RealCommand>>();

            var handler = handlers.Last();

            // Assert
            AssertThat.IsInstanceOfType(typeof(NullCommandHandler<RealCommand>), handler);

            bool nullCommandHandlerIsDecorated =
                handler.GetType() == typeof(TransactionalCommandHandlerDecorator<RealCommand>);

            Assert.IsFalse(nullCommandHandlerIsDecorated);
        }

        [TestMethod]
        public void GetRelationships_OnAPartiallyDecoratedCollection_ReturnsTheExpectedRelationships()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.RegisterCollection<ICommandHandler<RealCommand>>(new[] 
            {
                typeof(RealCommandHandler),
                typeof(DefaultCommandHandler<RealCommand>),
                typeof(NullCommandHandler<RealCommand>)
            });

            container.RegisterDecorator(
                typeof(ICommandHandler<>),
                typeof(TransactionalCommandHandlerDecorator<>),
                c => c.ImplementationType != typeof(NullCommandHandler<RealCommand>));

            // Act
            container.Verify();

            var relationships = container.GetRegistration(typeof(IEnumerable<ICommandHandler<RealCommand>>))
                .GetRelationships();

            // Assert
            Assert.AreEqual(2, relationships.Length);

            var real = relationships[0];

            AssertThat.AreEqual(typeof(TransactionalCommandHandlerDecorator<RealCommand>), real.ImplementationType);
            AssertThat.AreEqual(typeof(ICommandHandler<RealCommand>), real.Dependency.ServiceType);
            AssertThat.AreEqual(typeof(RealCommandHandler), real.Dependency.ImplementationType);

            var @default = relationships[1];

            AssertThat.AreEqual(typeof(TransactionalCommandHandlerDecorator<RealCommand>), @default.ImplementationType);
            AssertThat.AreEqual(typeof(ICommandHandler<RealCommand>), @default.Dependency.ServiceType);
            AssertThat.AreEqual(typeof(DefaultCommandHandler<RealCommand>), @default.Dependency.ImplementationType);
        }

        [TestMethod]
        public void GetAllInstances_DecoratorRegisteredWithPredicate_DecoratesAllInstancesThatShouldBeDecorated()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.RegisterCollection<ICommandHandler<RealCommand>>(new[] 
            {
                typeof(RealCommandHandler),
                typeof(DefaultCommandHandler<RealCommand>),
                typeof(NullCommandHandler<RealCommand>)
            });

            container.RegisterDecorator(
                typeof(ICommandHandler<>),
                typeof(TransactionalCommandHandlerDecorator<>), context =>
                {
                    var name = context.ImplementationType.Name;

                    return name.StartsWith("Default") || name.StartsWith("Null");
                });

            // Act
            var handlers = container.GetAllInstances<ICommandHandler<RealCommand>>().ToArray();

            Assert.AreEqual(3, handlers.Length, "Not the correct number of handlers was returned.");

            var realHandler = handlers[0];
            var defaultHandler = handlers[1];
            var nullHandler = handlers[2];

            // Assert
            AssertThat.IsInstanceOfType(typeof(RealCommandHandler), realHandler, "The RealCommandCommandHandler was expected not to be decorated.");
            AssertThat.IsInstanceOfType(typeof(TransactionalCommandHandlerDecorator<RealCommand>), defaultHandler, "The DefaultCommandHandler was expected to be decorated.");
            AssertThat.IsInstanceOfType(typeof(TransactionalCommandHandlerDecorator<RealCommand>), nullHandler, "The NullCommandHandler was expected to be decorated.");
        }

        [TestMethod]
        public void GetAllInstances_MultipleDecoratorsRegisteredWithPredicate_DecoratesInstancesThatShouldBeDecorated()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.RegisterCollection<ICommandHandler<RealCommand>>(new[] 
            {
                typeof(RealCommandHandler),
                typeof(NullCommandHandler<RealCommand>)
            });

            container.RegisterDecorator(
                typeof(ICommandHandler<>),
                typeof(TransactionalCommandHandlerDecorator<>),
                c => c.ImplementationType == typeof(RealCommandHandler));

            container.RegisterDecorator(
                typeof(ICommandHandler<>),
                typeof(AsyncCommandHandlerProxy<>),
                Lifestyle.Singleton,
                c => c.ImplementationType == typeof(RealCommandHandler));

            // Act
            var handlers = container.GetAllInstances<ICommandHandler<RealCommand>>();

            var handler = handlers.First();

            // Assert
            bool realCommandHandlerIsDecorated =
                handler.GetType() == typeof(AsyncCommandHandlerProxy<RealCommand>);

            Assert.IsTrue(realCommandHandlerIsDecorated);
        }

        [TestMethod]
        public void GetAllInstances_MultipleDecoratorsRegisteredWithPredicate2_DecoratesInstancesThatShouldBeDecorated()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.RegisterCollection<ICommandHandler<RealCommand>>(new[] 
            {
                typeof(RealCommandHandler),
                typeof(NullCommandHandler<RealCommand>)
            });

            container.RegisterDecorator(
                typeof(ICommandHandler<>),
                typeof(TransactionalCommandHandlerDecorator<>),
                c => c.ImplementationType == typeof(RealCommandHandler));

            container.RegisterDecorator(
                typeof(ICommandHandler<>),
                typeof(AsyncCommandHandlerProxy<>),
                Lifestyle.Singleton,
                c => c.ImplementationType == typeof(NullCommandHandler<RealCommand>));

            // Act
            var realHandler1 = container.GetAllInstances<ICommandHandler<RealCommand>>().First();
            var realHandler2 = container.GetAllInstances<ICommandHandler<RealCommand>>().First();

            var nullHandler1 = container.GetAllInstances<ICommandHandler<RealCommand>>().Last();
            var nullHandler2 = container.GetAllInstances<ICommandHandler<RealCommand>>().Last();

            // Assert
            AssertThat.IsInstanceOfType(typeof(TransactionalCommandHandlerDecorator<RealCommand>), realHandler1, "RealCommandCommandHandler hasn't been decorated properly.");

            AssertThat.IsInstanceOfType(typeof(AsyncCommandHandlerProxy<RealCommand>), nullHandler1, "NullCommandHandler hasn't been decorated properly.");

            Assert.IsFalse(object.ReferenceEquals(realHandler1, realHandler2),
                "TransactionalCommandHandlerDecorator is registered as transient and should therefore be transient.");
            Assert.IsTrue(object.ReferenceEquals(nullHandler1, nullHandler2),
                "AsyncCommandHandlerProxy is registered as singleton and should therefore be singleton.");
        }

        [TestMethod]
        public void GetAllInstances_PredicateDecorator_PredicateGetsSuppliedWithExpectedAppliedDecoratorsCollection()
        {
            // Arrange
            int predicateCallCount = 0;

            var container = ContainerFactory.New();

            container.RegisterCollection<ICommandHandler<RealCommand>>(new[] 
            {
                typeof(RealCommandHandler),
                typeof(NullCommandHandler<RealCommand>)
            });

            container.RegisterDecorator(
                typeof(ICommandHandler<>),
                typeof(TransactionalCommandHandlerDecorator<>));

            container.RegisterDecorator(
                typeof(ICommandHandler<>),
                typeof(AsyncCommandHandlerProxy<>),
                Lifestyle.Singleton, context =>
                {
                    predicateCallCount++;

                    // Assert
                    Assert.AreEqual(typeof(TransactionalCommandHandlerDecorator<RealCommand>),
                        context.AppliedDecorators.Single());

                    return true;
                });

            // Act
            container.GetAllInstances<ICommandHandler<RealCommand>>().ToArray();

            // Assert
            Assert.AreEqual(2, predicateCallCount, "The predicate is expected to be called once per handler.");
        }

        [TestMethod]
        public void GetAllInstances_PredicateDecorator_AppliedDecoratorsIsEmpyWhenNoDecoratorsHaveBeenAppliedYet()
        {
            // Arrange
            int predicateCallCount = 0;

            var container = ContainerFactory.New();

            container.RegisterCollection<ICommandHandler<RealCommand>>(new[]
            {
                typeof(RealCommandHandler),
                typeof(NullCommandHandler<RealCommand>)
            });

            container.RegisterDecorator(
                typeof(ICommandHandler<>),
                typeof(TransactionalCommandHandlerDecorator<>),
                context => context.ImplementationType == typeof(RealCommandHandler));

            container.RegisterDecorator(
                typeof(ICommandHandler<>),
                typeof(LifetimeScopeCommandHandlerProxy<>),
                Lifestyle.Singleton,
                context => context.ImplementationType == typeof(RealCommandHandler));

            container.RegisterDecorator(
                typeof(ICommandHandler<>),
                typeof(AsyncCommandHandlerProxy<>),
                Lifestyle.Singleton, context =>
                {
                    if (context.ImplementationType == typeof(NullCommandHandler<RealCommand>))
                    {
                        predicateCallCount++;

                        Assert.AreEqual(0, context.AppliedDecorators.Count,
                            "No decorators have been applied to the NullCommandHandler and the " +
                            "AppliedDecorators collection is expected to be empty.");
                    }

                    return true;
                });

            // Act
            container.GetAllInstances<ICommandHandler<RealCommand>>().ToArray();

            // Assert
            Assert.AreEqual(1, predicateCallCount, "The predicate is expected to be called just once.");
        }

        [TestMethod]
        public void GetAllInstances_InstancesRegisteredWithRegisterAllParamsTAndDecorated_InjectsSingletons()
        {
            // Arrange
            var container = ContainerFactory.New();

            // Uses the RegisterAll<T>(params T[]) that explicitly registers a collection of singletons.
            container.RegisterCollection<ICommandHandler<RealCommand>>(
                new RealCommandHandler(),
                new NullCommandHandler<RealCommand>());

            container.RegisterDecorator(
                typeof(ICommandHandler<>),
                typeof(TransactionalCommandHandlerDecorator<>));

            // Act
            var decorator = container.GetAllInstances<ICommandHandler<RealCommand>>().First();

            // Assert
            AssertThat.IsInstanceOfType(typeof(TransactionalCommandHandlerDecorator<RealCommand>), decorator);
        }

        [TestMethod]
        public void GetAllInstances_InstancesRegisteredWithRegisterAllParamsTAndDecorated_SuppliesTheCorrectPredicateContextForEachElement()
        {
            // Arrange
            var container = ContainerFactory.New();

            var predicateContexts = new List<DecoratorPredicateContext>();

            // Uses the RegisterAll<T>(params T[]) that explicitly registers a collection of singletons.
            container.RegisterCollection<ICommandHandler<RealCommand>>(
                new RealCommandHandler(),
                new NullCommandHandler<RealCommand>());

            container.RegisterDecorator(
                typeof(ICommandHandler<>),
                typeof(TransactionalCommandHandlerDecorator<>), context =>
                {
                    predicateContexts.Add(context);

                    return false;
                });

            // Act
            container.GetAllInstances<ICommandHandler<RealCommand>>().ToArray();
            container.GetAllInstances<ICommandHandler<RealCommand>>().ToArray();
            container.GetAllInstances<ICommandHandler<RealCommand>>().ToArray();
            container.GetAllInstances<ICommandHandler<RealCommand>>().ToArray();
            container.GetAllInstances<ICommandHandler<RealCommand>>().ToArray();

            // Assert
            Assert.AreEqual(2, predicateContexts.Count, "Since the registration is made with an array of " +
                "singletons, the decorator system should have enough information to call the predicate " +
                "once for each element in the collection (the collection contains two elements).");

            DecoratorPredicateContext realContext = predicateContexts[0];

            AssertThat.AreEqual(typeof(ICommandHandler<RealCommand>), realContext.ServiceType);
            AssertThat.AreEqual(typeof(RealCommandHandler), realContext.ImplementationType);
            AssertThat.IsInstanceOfType(typeof(ConstantExpression), realContext.Expression);
            Assert.AreEqual(0, realContext.AppliedDecorators.Count);

            DecoratorPredicateContext nullContext = predicateContexts[1];

            Assert.AreEqual(nullContext.ServiceType, typeof(ICommandHandler<RealCommand>));
            Assert.AreEqual(nullContext.ImplementationType, typeof(NullCommandHandler<RealCommand>));
            AssertThat.IsInstanceOfType(typeof(ConstantExpression), nullContext.Expression);
            Assert.AreEqual(0, nullContext.AppliedDecorators.Count);
        }

        [TestMethod]
        public void GetAllInstances_InstancesRegisteredWithRegisterAllEnumerableAndDecorated_CallsThePredicateJustOnceForTheWholeCollection()
        {
            // Arrange
            var container = ContainerFactory.New();

            var predicateContexts = new List<DecoratorPredicateContext>();

            IEnumerable<ICommandHandler<RealCommand>> dynamicList = new List<ICommandHandler<RealCommand>>
            {
                new RealCommandHandler(),
                new NullCommandHandler<RealCommand>()
            };

            // Uses the RegisterAll<T>(IEnumerable<T>) that registers a dynamic list.
            container.RegisterCollection<ICommandHandler<RealCommand>>(dynamicList);

            container.RegisterDecorator(
                typeof(ICommandHandler<>),
                typeof(TransactionalCommandHandlerDecorator<>), context =>
                {
                    predicateContexts.Add(context);

                    return false;
                });

            // Act
            container.GetAllInstances<ICommandHandler<RealCommand>>().ToArray();
            container.GetAllInstances<ICommandHandler<RealCommand>>().ToArray();
            container.GetAllInstances<ICommandHandler<RealCommand>>().ToArray();
            container.GetAllInstances<ICommandHandler<RealCommand>>().ToArray();
            container.GetAllInstances<ICommandHandler<RealCommand>>().ToArray();

            // Assert
            Assert.AreEqual(1, predicateContexts.Count, "The predicate should therefore be called " +
                "just once when collections are registered using RegisterAll(IEnumerable).");

            DecoratorPredicateContext collectionContext = predicateContexts.Single();

            AssertThat.AreEqual(typeof(ICommandHandler<RealCommand>), collectionContext.ServiceType);
            AssertThat.AreEqual(typeof(ICommandHandler<RealCommand>), collectionContext.ImplementationType,
                "Since there is no information about the elements of the collection (and they can change) " +
                "there is no information about the implementation type, and the service type should be applied.");

            Assert.AreEqual(0, collectionContext.AppliedDecorators.Count);
        }

        [TestMethod]
        public void GetAllInstances_InstancesRegisteredWithRegisterAllEnumerableAndDecoratedWithMultipleDecorators_SuppliesThePreviouslyAppliedDecoratorsToThePredicate()
        {
            // Arrange
            var container = ContainerFactory.New();

            int predicateCallCount = 0;

            IEnumerable<ICommandHandler<RealCommand>> dynamicList = new List<ICommandHandler<RealCommand>>
            {
                new RealCommandHandler(),
                new NullCommandHandler<RealCommand>()
            };

            // Uses the RegisterAll<T>(IEnumerable<T>) that registers a dynamic list.
            container.RegisterCollection<ICommandHandler<RealCommand>>(dynamicList);

            container.RegisterDecorator(
                typeof(ICommandHandler<>),
                typeof(TransactionalCommandHandlerDecorator<>));

            container.RegisterDecorator(
                typeof(ICommandHandler<>),
                typeof(RealCommandHandlerDecorator), context =>
                {
                    predicateCallCount++;

                    // Assert
                    Assert.AreEqual(1, context.AppliedDecorators.Count);
                    Assert.AreEqual(context.AppliedDecorators.Single(),
                        typeof(TransactionalCommandHandlerDecorator<RealCommand>));

                    return false;
                });

            // Act
            container.GetAllInstances<ICommandHandler<RealCommand>>().ToArray();
            container.GetAllInstances<ICommandHandler<RealCommand>>().ToArray();
            container.GetAllInstances<ICommandHandler<RealCommand>>().ToArray();
            container.GetAllInstances<ICommandHandler<RealCommand>>().ToArray();
            container.GetAllInstances<ICommandHandler<RealCommand>>().ToArray();

            // Assert
            Assert.AreEqual(1, predicateCallCount);
        }

        [TestMethod]
        public void GetAllInstances_InstancesRegisteredWithRegisterAllEnumerableAndDecoratedWithMultipleDecorators_DoesNotSupplyThePreviousDecoratorWhenItWasNotApplied()
        {
            // Arrange
            var container = ContainerFactory.New();

            int predicateCallCount = 0;

            IEnumerable<ICommandHandler<RealCommand>> dynamicList = new List<ICommandHandler<RealCommand>>
            {
                new RealCommandHandler(),
                new NullCommandHandler<RealCommand>()
            };

            // Uses the RegisterAll<T>(IEnumerable<T>) that registers a dynamic list.
            container.RegisterCollection<ICommandHandler<RealCommand>>(dynamicList);

            container.RegisterDecorator(
                typeof(ICommandHandler<>),
                typeof(TransactionalCommandHandlerDecorator<>), c => false);

            container.RegisterDecorator(
                typeof(ICommandHandler<>),
                typeof(RealCommandHandlerDecorator), context =>
                {
                    predicateCallCount++;

                    // Assert
                    Assert.AreEqual(0, context.AppliedDecorators.Count,
                        "No decorators expected since, the previous decorator is not applied.");

                    return false;
                });

            // Act
            container.GetAllInstances<ICommandHandler<RealCommand>>().ToArray();

            // Assert
            Assert.AreEqual(1, predicateCallCount);
        }

        [TestMethod]
        public void GetInstance_BothServiceAndCollectionOfServicesRegistered_RegistrationsDontShareTheirPredicateContext()
        {
            // Arrange
            var container = ContainerFactory.New();

            int predicateCallCount = 0;

            IEnumerable<ICommandHandler<RealCommand>> dynamicList = new List<ICommandHandler<RealCommand>>
            {
                new RealCommandHandler(),
                new NullCommandHandler<RealCommand>()
            };

            container.Register<ICommandHandler<RealCommand>, RealCommandHandler>();

            // Uses the RegisterAll<T>(IEnumerable<T>) that registers a dynamic list.
            container.RegisterCollection<ICommandHandler<RealCommand>>(dynamicList);

            container.RegisterDecorator(
                typeof(ICommandHandler<>),
                typeof(TransactionalCommandHandlerDecorator<>));

            container.RegisterDecorator(
                typeof(ICommandHandler<>),
                typeof(RealCommandHandlerDecorator), context =>
                {
                    predicateCallCount++;

                    // Assert
                    Assert.AreEqual(1, context.AppliedDecorators.Count,
                        "One decorator was expected to be applied. Applied decorators: " +
                        context.AppliedDecorators.ToFriendlyNamesText());

                    Assert.AreEqual(context.AppliedDecorators.Single(),
                        typeof(TransactionalCommandHandlerDecorator<RealCommand>));

                    return false;
                });

            // Act
            container.GetInstance<ICommandHandler<RealCommand>>();

            // Assert
            Assert.AreEqual(1, predicateCallCount);

            // Act
            container.GetAllInstances<ICommandHandler<RealCommand>>();

            // Assert
            Assert.AreEqual(2, predicateCallCount);
        }

        [TestMethod]
        public void GetAllInstances_DecoratorRegisteredTwiceAsSingleton_WrapsTheDecorateeTwice()
        {
            // Arrange
            var container = ContainerFactory.New();

            IEnumerable<ICommandHandler<RealCommand>> dynamicList = new List<ICommandHandler<RealCommand>>
            {
                new RealCommandHandler(),
                new NullCommandHandler<RealCommand>()
            };

            // Uses the RegisterAll<T>(IEnumerable<T>) that registers a dynamic list.
            container.RegisterCollection<ICommandHandler<RealCommand>>(dynamicList);

            // Register the same decorator twice. 
            container.RegisterDecorator(
                typeof(ICommandHandler<>),
                typeof(TransactionalCommandHandlerDecorator<>),
                Lifestyle.Singleton);

            container.RegisterDecorator(
                typeof(ICommandHandler<>),
                typeof(TransactionalCommandHandlerDecorator<>),
                Lifestyle.Singleton);

            // Act
            var decorator1 = (TransactionalCommandHandlerDecorator<RealCommand>)
                container.GetAllInstances<ICommandHandler<RealCommand>>().First();

            var decorator2 = decorator1.Decorated;

            // Assert
            AssertThat.IsInstanceOfType(typeof(TransactionalCommandHandlerDecorator<RealCommand>), decorator2, "Since the decorator is registered twice, it should wrap the decoratee twice.");

            var decoratee = ((TransactionalCommandHandlerDecorator<RealCommand>)decorator2).Decorated;

            Assert.AreEqual(typeof(RealCommandHandler).ToFriendlyName(), decoratee.GetType().ToFriendlyName());
        }

        [TestMethod]
        public void RegisterAll_ContainerUncontrolledSingletons_InitializesThoseSingletonsOnce()
        {
            // Arrange
            var containerUncontrolledSingletonHandlers = new ICommandHandler<RealCommand>[]
            {
                new StubCommandHandler(), new RealCommandHandler()
            };

            var actualInitializedHandlers = new List<ICommandHandler<RealCommand>>();

            var container = ContainerFactory.New();

            container.RegisterCollection<ICommandHandler<RealCommand>>(containerUncontrolledSingletonHandlers);

            container.RegisterInitializer<ICommandHandler<RealCommand>>(handler =>
            {
                actualInitializedHandlers.Add(handler);
            });

            // Act
            container.GetAllInstances<ICommandHandler<RealCommand>>().ToArray();
            container.GetAllInstances<ICommandHandler<RealCommand>>().ToArray();

            // Assert
            Assert.AreEqual(2, actualInitializedHandlers.Count, "The handlers are expected to be initialized.");
            Assert.IsFalse(actualInitializedHandlers.Except(containerUncontrolledSingletonHandlers).Any(),
                "Both handlers are expected to be initialized.");
        }

        [TestMethod]
        public void GetRegistration_ContainerControlledCollectionWithDecorator_ContainsExpectedListOfRelationships()
        {
            // Arrange
            var expectedRelationship = new RelationshipInfo
            {
                ImplementationType = typeof(RealCommandHandlerDecorator),
                Lifestyle = Lifestyle.Transient,
                Dependency = new DependencyInfo(typeof(ICommandHandler<RealCommand>), Lifestyle.Transient)
            };

            var container = ContainerFactory.New();

            container.RegisterCollection<ICommandHandler<RealCommand>>(new[]
            {
                typeof(StubCommandHandler),
                typeof(RealCommandHandler)
            });

            // RealCommandHandlerDecorator only takes a dependency on ICommandHandler<RealCommand>
            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(RealCommandHandlerDecorator));

            container.Verify();

            var producer = container.GetRegistration(typeof(IEnumerable<ICommandHandler<RealCommand>>));

            // Act
            var relationships = producer.GetRelationships();

            // Assert
            Assert.AreEqual(2, relationships.Length);
            Assert.AreEqual(2, relationships.Count(actual => expectedRelationship.Equals(actual)));
        }

        [TestMethod]
        public void GetRegistration_ContainerUncontrolledCollectionWithDecorator_ContainsExpectedListOfRelationships()
        {
            // Arrange
            var expectedRelationship = new RelationshipInfo
            {
                ImplementationType = typeof(RealCommandHandlerDecorator),
                Lifestyle = Lifestyle.Transient,
                Dependency = new DependencyInfo(typeof(ICommandHandler<RealCommand>), Lifestyle.Unknown)
            };

            var container = ContainerFactory.New();

            IEnumerable<ICommandHandler<RealCommand>> containerUncontrolledCollection =
                new ICommandHandler<RealCommand>[] { new StubCommandHandler(), new RealCommandHandler() };

            container.RegisterCollection<ICommandHandler<RealCommand>>(containerUncontrolledCollection);

            // RealCommandHandlerDecorator only takes a dependency on ICommandHandler<RealCommand>
            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(RealCommandHandlerDecorator));

            // Verify() ensures that all Relationships are built.
            container.Verify();

            var r = container.GetCurrentRegistrations();

            // Act
            var registration = container.GetRegistration(typeof(IEnumerable<ICommandHandler<RealCommand>>));
            var relationships = registration.GetRelationships();

            // Assert
            Assert.AreEqual(1, relationships.Length);
            Assert.IsTrue(expectedRelationship.Equals(relationships[0]),
                "Actual relationship: " + RelationshipInfo.ToString(relationships[0]));
        }

        [TestMethod]
        public void GetInstance_ContainerControlledCollectionWithDecorator_DecoratorGoesThroughCompletePipeLineIncludingExpressionBuilding()
        {
            // Arrange
            var typesBuilding = new List<Type>();

            var container = ContainerFactory.New();

            var typesToRegister = new[] { typeof(StubCommandHandler), typeof(RealCommandHandler) };

            container.RegisterCollection<ICommandHandler<RealCommand>>(typesToRegister);

            // RealCommandHandlerDecorator only takes a dependency on ICommandHandler<RealCommand>
            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(RealCommandHandlerDecorator));

            container.ExpressionBuilding += (s, e) =>
            {
                if (e.Expression is NewExpression)
                {
                    typesBuilding.Add(((NewExpression)e.Expression).Constructor.DeclaringType);
                }
            };

            // Act
            container.GetAllInstances<ICommandHandler<RealCommand>>().ToArray();

            // Assert
            var decorators = typesBuilding.Where(type => type == typeof(RealCommandHandlerDecorator)).ToArray();

            Assert.AreEqual(typesToRegister.Length, decorators.Length,
                "The decorator is expected to go through the complete pipeline, including " +
                "ExpressionBuilding. Since the collection is container controlled the ExpressionBuilding " +
                "should be called for each type in the collection.");
        }

        [TestMethod]
        public void GetInstance_ContainerUncontrolledCollectionWithDecorator_DecoratorGoesThroughCompletePipeLineIncludingExpressionBuilding()
        {
            // Arrange
            var typesBuilding = new List<Type>();

            var container = ContainerFactory.New();

            IEnumerable<ICommandHandler<RealCommand>> containerUncontrolledCollection =
                new ICommandHandler<RealCommand>[] { new StubCommandHandler(), new RealCommandHandler() };

            container.RegisterCollection<ICommandHandler<RealCommand>>(containerUncontrolledCollection);

            // RealCommandHandlerDecorator only takes a dependency on ICommandHandler<RealCommand>
            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(RealCommandHandlerDecorator));

            container.ExpressionBuilding += (s, e) =>
            {
                if (e.Expression is NewExpression)
                {
                    typesBuilding.Add(((NewExpression)e.Expression).Constructor.DeclaringType);
                }
            };

            // Act
            container.GetAllInstances<ICommandHandler<RealCommand>>().ToArray();

            // Assert
            var decorators = typesBuilding.Where(type => type == typeof(RealCommandHandlerDecorator)).ToArray();

            Assert.AreEqual(1, decorators.Length,
                "The decorator is expected to go through the complete pipeline, including " +
                "ExpressionBuilding. Since the collection is container uncontrolled the ExpressionBuilding " +
                "should be called once for the complete collection.");
        }

        [TestMethod]
        public void GetAllInstances_DecoratingContainerControlledCollectionWithHybridLifestyle_AppliesLifestyleCorrectly()
        {
            // Arrange
            var container = ContainerFactory.New();

            var typesToRegister = new[] { typeof(StubCommandHandler), typeof(RealCommandHandler) };

            container.RegisterCollection(typeof(ICommandHandler<RealCommand>),
                from type in typesToRegister
                select Lifestyle.Singleton.CreateRegistration(type, container));

            var hybridLifestyle = Lifestyle.CreateHybrid(() => false, Lifestyle.Transient, Lifestyle.Singleton);

            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(RealCommandHandlerDecorator),
                hybridLifestyle);

            // Act
            var instance1 = container.GetAllInstances<ICommandHandler<RealCommand>>().First();
            var instance2 = container.GetAllInstances<ICommandHandler<RealCommand>>().First();

            // Assert
            AssertThat.IsInstanceOfType(typeof(RealCommandHandlerDecorator), instance1);
            Assert.IsTrue(object.ReferenceEquals(instance1, instance2));
        }

        [TestMethod]
        public void GetAllInstances_DecoratingContainerUncontrolledCollectionWithLifestyleOtherThanTransientAndSingleton_ThrowsExpectedException()
        {
            // Arrange
            var container = ContainerFactory.New();

            IEnumerable<ICommandHandler<RealCommand>> containerUncontrolledCollection =
                new ICommandHandler<RealCommand>[] { new StubCommandHandler(), new RealCommandHandler() };

            container.RegisterCollection<ICommandHandler<RealCommand>>(containerUncontrolledCollection);

            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(RealCommandHandlerDecorator),
                Lifestyle.CreateHybrid(() => true, Lifestyle.Singleton, Lifestyle.Singleton));

            try
            {
                // Act
                container.GetAllInstances<ICommandHandler<RealCommand>>();

                // Assert
                Assert.Fail("Exception expected.");
            }
            catch (Exception ex)
            {
                AssertThat.ExceptionMessageContains(@"
                    You are trying to apply the RealCommandHandlerDecorator decorator with the 
                    'Hybrid Singleton / Singleton' lifestyle to a collection of type 
                    ICommandHandler<RealCommand>, but the registered collection is not controlled by the
                    container."
                    .TrimInside(), ex);

                AssertThat.ExceptionMessageContains(@"
                    Since the number of returned items might change on each call, the decorator with this 
                    lifestyle cannot be applied to the collection. Instead, register the decorator with the 
                    Transient lifestyle, or use one of the RegisterCollection overloads that takes a collection of 
                    System.Type types."
                    .TrimInside(), ex);
            }
        }

        [TestMethod]
        public void GetRelationships_AddingRelationshipDuringBuildingOnDecoratorTypeForUncontrolledCollection_ContainsAddedRelationship()
        {
            // Arrange
            KnownRelationship expectedRelationship = GetValidRelationship();

            var container = ContainerFactory.New();

            IEnumerable<ICommandHandler<RealCommand>> containerUncontrolledCollection =
                new ICommandHandler<RealCommand>[] { new StubCommandHandler(), new RealCommandHandler() };

            container.RegisterCollection<ICommandHandler<RealCommand>>(containerUncontrolledCollection);

            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(RealCommandHandlerDecorator));

            container.ExpressionBuilding += (s, e) =>
            {
                if (e.KnownImplementationType == typeof(RealCommandHandlerDecorator))
                {
                    e.KnownRelationships.Add(expectedRelationship);
                }
            };

            container.Verify(VerificationOption.VerifyOnly);

            // Act
            var commandHandlerCollectionRegistration =
                container.GetRegistration(typeof(IEnumerable<ICommandHandler<RealCommand>>));

            var relationships = commandHandlerCollectionRegistration.GetRelationships();

            // Assert
            Assert.AreEqual(1, relationships.Count(relationship => relationship == expectedRelationship),
                "Any known relationships added to the decorator during the ExpressionBuilding event " +
                "should be added to the registration of the service type.");
        }

        [TestMethod]
        public void GetRelationships_AddingRelationshipDuringBuildingOnDecoratorTypeForControlledCollection_ContainsAddedRelationship()
        {
            // Arrange
            KnownRelationship expectedRelationship = GetValidRelationship();

            var container = ContainerFactory.New();

            container.RegisterCollection<ICommandHandler<RealCommand>>(new[]
            {
                typeof(StubCommandHandler),
                typeof(RealCommandHandler)
            });

            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(RealCommandHandlerDecorator));

            container.ExpressionBuilding += (s, e) =>
            {
                if (e.KnownImplementationType == typeof(RealCommandHandlerDecorator))
                {
                    e.KnownRelationships.Add(expectedRelationship);
                }
            };

            container.Verify(VerificationOption.VerifyOnly);

            // Act
            var commandHandlerCollectionRegistration =
                container.GetRegistration(typeof(IEnumerable<ICommandHandler<RealCommand>>));

            var relationships = commandHandlerCollectionRegistration.GetRelationships();

            // Assert
            Assert.AreEqual(1, relationships.Count(relationship => relationship == expectedRelationship),
                "Any known relationships added to the decorator during the ExpressionBuilding event " +
                "should be added to the registration of the service type. Current: " +
                relationships.Select(r => r.ImplementationType).ToFriendlyNamesText());
        }

        [TestMethod]
        public void GetAllInstances_DecoratingEmptyCollectionWithLifestyleOtherThanTransientAndSingleton_Succeeds()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.RegisterCollection(typeof(ICommandHandler<>), Type.EmptyTypes);

            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(RealCommandHandlerDecorator),
                Lifestyle.CreateHybrid(() => true, Lifestyle.Singleton, Lifestyle.Singleton));

            // Act
            container.GetAllInstances<ICommandHandler<RealCommand>>().ToArray();
        }

        [TestMethod]
        public void GetAllInstances_CollectionRegistrationWithTypeReferingBackToAnotherContainerRegistration_AllowsApplyingDecoratorsOnBothLevels()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Register<IDerive, DeriveImplementation>();
            container.RegisterDecorator(typeof(IDerive), typeof(DeriveDecorator));
            container.RegisterCollection<IBase>(new[] { typeof(IDerive) });
            container.RegisterDecorator(typeof(IBase), typeof(BaseDecorator));

            // Act
            var decorator = (BaseDecorator)container.GetAllInstances<IBase>().Single();

            // Assert
            AssertThat.IsInstanceOfType(typeof(DeriveDecorator), decorator.Decoratee, "Since the collection element points back into a container's registration, we would expect " +
                "the type to be decorated with that decorator as well.");
        }

        [TestMethod]
        public void GetAllInstances_CollectionRegistrationWithTypeReferingBackToAnotherContainerRegistrationForSameBaseType_WillNotApplyDecoratorTwice()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Register<IBase, DeriveImplementation>();
            container.RegisterCollection<IBase>(new[] { typeof(IBase) });
            container.RegisterDecorator(typeof(IBase), typeof(BaseDecorator));

            // Act
            var decorator = (BaseDecorator)container.GetAllInstances<IBase>().Single();

            // Assert
            AssertThat.IsInstanceOfType(typeof(DeriveImplementation), decorator.Decoratee, "Since the collection element points back into a container's registration of the same type, " +
                "we'd expect the decorator to be applied just once.");
        }

        [TestMethod]
        public void GetAllInstances_ControlledCollectionDecoratedWithFactoryDefinedDecorator_WrapsTheExpectedDecorators()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.RegisterCollection<IPlugin>(new[] { typeof(PluginImpl), typeof(PluginImpl2) });

            container.RegisterDecorator(typeof(IPlugin),
                decoratorTypeFactory: c => typeof(PluginDecorator<>).MakeGenericType(c.ImplementationType),
                lifestyle: Lifestyle.Transient,
                predicate: c => true);

            // Act
            var plugins = container.GetAllInstances<IPlugin>().ToArray();

            // Assert
            AssertThat.IsInstanceOfType(typeof(PluginDecorator<PluginImpl>), plugins[0]);
            AssertThat.IsInstanceOfType(typeof(PluginDecorator<PluginImpl2>), plugins[1]);
        }

        [TestMethod]
        public void GetAllInstances_UncontrolledCollectionDecoratedWithFactoryDefinedDecorator_WrapsTheExpectedDecorators()
        {
            // Arrange
            var container = ContainerFactory.New();

            IEnumerable<IPlugin> uncontrolledCollection = new IPlugin[] { new PluginImpl(), new PluginImpl2() };

            container.RegisterCollection<IPlugin>(uncontrolledCollection);

            // For an uncontrolled collection the ImplementationType will equal the ServiceType, since the
            // system knows nothing about the actual used instances.
            container.RegisterDecorator(typeof(IPlugin),
                decoratorTypeFactory: c => typeof(PluginDecorator<>).MakeGenericType(c.ImplementationType),
                lifestyle: Lifestyle.Transient,
                predicate: c => true);

            // Act
            var plugins = container.GetAllInstances<IPlugin>().ToArray();

            // Assert
            AssertThat.AreEqual(typeof(PluginDecorator<IPlugin>), plugins[0].GetType());
            AssertThat.AreEqual(typeof(PluginDecorator<IPlugin>), plugins[1].GetType());
        }

        [TestMethod]
        public void GetAllInstances_UncontrolledRegisterDecoratorWithDecoratorReturningOpenGenericType_WrapsTheServiceWithTheClosedDecorator()
        {
            // Arrange
            var container = new Container();

            IEnumerable<ICommandHandler<RealCommand>> uncontrolledCollection = new[] { new RealCommandHandler() };

            container.RegisterCollection<ICommandHandler<RealCommand>>(uncontrolledCollection);

            var parameters = RegisterDecoratorFactoryParameters.CreateValid();

            parameters.ServiceType = typeof(ICommandHandler<>);
            parameters.DecoratorTypeFactory = context => typeof(TransactionHandlerDecorator<>);

            container.RegisterDecorator(parameters);

            // Act
            var handler = container.GetAllInstances<ICommandHandler<RealCommand>>().Single();

            // Assert
            AssertThat.IsInstanceOfType(typeof(TransactionHandlerDecorator<RealCommand>), handler);
        }

        [TestMethod]
        public void GetAllInstances_UncontrolledRegisterDecoratorWithPredicateReturningFalse_DoesNotWrapTheServiceWithTheDecorator()
        {
            // Arrange
            var container = new Container();

            IEnumerable<ICommandHandler<RealCommand>> uncontrolledCollection = new[] { new RealCommandHandler() };

            container.RegisterCollection<ICommandHandler<RealCommand>>(uncontrolledCollection);

            var parameters = RegisterDecoratorFactoryParameters.CreateValid();

            parameters.Predicate = context => false;
            parameters.ServiceType = typeof(ICommandHandler<>);
            parameters.DecoratorTypeFactory = context => typeof(TransactionHandlerDecorator<>);

            container.RegisterDecorator(parameters);

            // Act
            var handler = container.GetAllInstances<ICommandHandler<RealCommand>>().Single();

            // Assert
            AssertThat.IsInstanceOfType(typeof(RealCommandHandler), handler);
        }

        [TestMethod]
        public void GetAllInstances_UncontrolledRegisterDecoratorWithPredicateReturningFalse_DoesNotCallTheFactory()
        {
            // Arrange
            bool decoratorTypeFactoryCalled = false;

            var container = new Container();

            IEnumerable<ICommandHandler<RealCommand>> uncontrolledCollection = new[] { new RealCommandHandler() };

            container.RegisterCollection<ICommandHandler<RealCommand>>(uncontrolledCollection);

            var parameters = RegisterDecoratorFactoryParameters.CreateValid();

            parameters.Predicate = context => false;
            parameters.ServiceType = typeof(ICommandHandler<>);

            parameters.DecoratorTypeFactory = context =>
            {
                decoratorTypeFactoryCalled = true;
                return typeof(TransactionHandlerDecorator<>);
            };

            container.RegisterDecorator(parameters);

            // Act
            var handler = container.GetAllInstances<ICommandHandler<RealCommand>>().Single();

            // Assert
            Assert.IsFalse(decoratorTypeFactoryCalled, @"
                The factory should not be called if the predicate returns false. This prevents the user from 
                having to do specific handling when the decorator type can't be constructed because of generic 
                type constraints.");
        }

        [TestMethod]
        public void GetAllInstances_UncontrolledRegisterDecoratorWithFactoryReturningTypeBasedOnImplementationType_WrapsTheServiceWithTheExpectedDecorator()
        {
            // Arrange
            var container = new Container();

            IEnumerable<INonGenericService> uncontrolledCollection = new[] { new RealNonGenericService() };

            container.RegisterCollection<INonGenericService>(uncontrolledCollection);

            var parameters = RegisterDecoratorFactoryParameters.CreateValid();

            parameters.ServiceType = typeof(INonGenericService);
            parameters.DecoratorTypeFactory =
                context => typeof(NonGenericServiceDecorator<>).MakeGenericType(context.ImplementationType);

            container.RegisterDecorator(parameters);

            // Act
            var service = container.GetAllInstances<INonGenericService>().Single();

            // Assert
            // With uncontrolled collections, the container has no knowledge about the actual elements of
            // the collection (since those elements can change on every iteration), so the ImplementationType
            // will in this case simply be the same as the ServiceType: INonGenericService.
            AssertThat.IsInstanceOfType(typeof(NonGenericServiceDecorator<INonGenericService>), service);
        }

        [TestMethod]
        public void GetAllInstances_UncontrolledRegisterDecoratorReturningAnOpenGenericType_AppliesThatTypeOnlyWhenTypeConstraintsAreMet()
        {
            // Arrange
            var container = new Container();

            // SpecialCommand implements ISpecialCommand, but RealCommand does not.
            IEnumerable<ICommandHandler<SpecialCommand>> uncontrolledCollection1 =
                new[] { new NullCommandHandler<SpecialCommand>() };

            container.RegisterCollection<ICommandHandler<SpecialCommand>>(uncontrolledCollection1);

            IEnumerable<ICommandHandler<RealCommand>> uncontrolledCollection2 =
                new[] { new NullCommandHandler<RealCommand>() };

            container.RegisterCollection<ICommandHandler<RealCommand>>(uncontrolledCollection2);

            var parameters = RegisterDecoratorFactoryParameters.CreateValid();

            parameters.ServiceType = typeof(ICommandHandler<>);

            // SpecialCommandHandlerDecorator has a "where T : ISpecialCommand" constraint.
            parameters.DecoratorTypeFactory = context => typeof(SpecialCommandHandlerDecorator<>);

            container.RegisterDecorator(parameters);

            // Act
            var handler1 = container.GetAllInstances<ICommandHandler<SpecialCommand>>().Single();
            var handler2 = container.GetAllInstances<ICommandHandler<RealCommand>>().Single();

            // Assert
            AssertThat.IsInstanceOfType(typeof(SpecialCommandHandlerDecorator<SpecialCommand>), handler1);
            AssertThat.IsInstanceOfType(typeof(NullCommandHandler<RealCommand>), handler2);
        }

        [TestMethod]
        public void GetAllInstances_UncontrolledRegisterDecoratorWithFactoryReturningAPartialOpenGenericType_WorksLikeACharm()
        {
            // Arrange
            var container = new Container();

            IEnumerable<ICommandHandler<RealCommand>> uncontrolledCollection = new[] { new RealCommandHandler() };

            container.RegisterCollection<ICommandHandler<RealCommand>>(uncontrolledCollection);

            var parameters = RegisterDecoratorFactoryParameters.CreateValid();

            parameters.ServiceType = typeof(ICommandHandler<>);

            // Here we make a partial open-generic type by filling in the TUnresolved.
            parameters.DecoratorTypeFactory = context =>
                typeof(CommandHandlerDecoratorWithUnresolvableArgument<,>)
                    .MakePartialOpenGenericType(
                        secondArgument: context.ImplementationType);

            container.RegisterDecorator(parameters);

            // Act
            var handler = container.GetAllInstances<ICommandHandler<RealCommand>>().Single();

            // Assert
            AssertThat.AreEqual(
                typeof(CommandHandlerDecoratorWithUnresolvableArgument<RealCommand, ICommandHandler<RealCommand>>),
                actualType: handler.GetType());
        }

        [TestMethod]
        public void GetAllInstances_UncontrolledWithClosedGenericServiceAndOpenGenericDecoratorReturnedByFactory_ReturnsDecoratedFactory()
        {
            // Arrange
            var container = new Container();

            IEnumerable<ICommandHandler<RealCommand>> uncontrolledCollection = new[] { new RealCommandHandler() };

            container.RegisterCollection<ICommandHandler<RealCommand>>(uncontrolledCollection);

            var parameters = RegisterDecoratorFactoryParameters.CreateValid();

            parameters.ServiceType = typeof(ICommandHandler<RealCommand>);
            parameters.DecoratorTypeFactory = context => typeof(TransactionHandlerDecorator<>);

            container.RegisterDecorator(parameters);

            // Act
            // Registering an closed generic service with an open generic decorator isn't supported by the
            // 'normal' RegisterDecorator methods. This is a limitation in the underlying system. The system
            // can't easily verify whether the open-generic decorator is assignable from the closed-generic
            // service.
            // The factory-supplying version doesn't have this limitation, since the factory is only called
            // at resolve-time, which means there are no open-generic types to check. Everything is closed.
            // So long story short: the following call will (or should) succeed.
            var handler = container.GetAllInstances<ICommandHandler<RealCommand>>().Single();

            // Assert
            AssertThat.IsInstanceOfType(typeof(TransactionHandlerDecorator<RealCommand>), handler);
        }

        [TestMethod]
        public void GetAllInstances_ContainerControlledDecoratorDependingOnDecoratorPredicateContext_ContainsTheExpectedContext()
        {
            // Arrange
            var container = new Container();

            container.RegisterCollection<ICommandHandler<RealCommand>>(new[] { typeof(RealCommandHandler) });

            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(TransactionHandlerDecorator<>));
            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(ContextualHandlerDecorator<>));

            // Act
            var decorator = (ContextualHandlerDecorator<RealCommand>)
                container.GetAllInstances<ICommandHandler<RealCommand>>().Single();

            DecoratorContext context = decorator.Context;

            // Assert
            Assert.AreSame(typeof(ICommandHandler<RealCommand>), context.ServiceType);
            Assert.AreSame(typeof(RealCommandHandler), context.ImplementationType);
            Assert.AreSame(typeof(TransactionHandlerDecorator<RealCommand>), context.AppliedDecorators.Single());
            Assert.AreEqual("new TransactionHandlerDecorator`1(new RealCommandHandler())",
                context.Expression.ToString());
            Assert.AreEqual("ServiceType = ICommandHandler<RealCommand>, ImplementationType = RealCommandHandler",
                context.DebuggerDisplay);
        }

        [TestMethod]
        public void GetAllInstances_ContainerUncontrolledDecoratorDependingOnDecoratorPredicateContext_ContainsTheExpectedContext()
        {
            // Arrange
            var container = new Container();

            IEnumerable<ICommandHandler<RealCommand>> uncontrolledCollection = new[] { new RealCommandHandler() };

            container.RegisterCollection<ICommandHandler<RealCommand>>(uncontrolledCollection);

            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(TransactionHandlerDecorator<>));
            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(ContextualHandlerDecorator<>));

            // Act
            var decorator = (ContextualHandlerDecorator<RealCommand>)
                container.GetAllInstances<ICommandHandler<RealCommand>>().Single();

            DecoratorContext context = decorator.Context;

            // Assert
            Assert.AreSame(typeof(TransactionHandlerDecorator<RealCommand>), context.AppliedDecorators.Single());

            // NOTE: Since this is an container uncontrolled collection, the container does not know the
            // exact type of the command handler, only its interface type is available.
            AssertThat.AreEqual(typeof(ICommandHandler<RealCommand>), context.ServiceType);
            AssertThat.AreEqual(typeof(ICommandHandler<RealCommand>), context.ImplementationType);
        }

        private static KnownRelationship GetValidRelationship()
        {
            var container = new Container();

            var dummyRegistration = container.GetRegistration(typeof(Container));

            return new KnownRelationship(typeof(object), Lifestyle.Transient, dummyRegistration);
        }

        private static void
            Assert_ExceptionContainsInfoAboutManualCollectionRegistrationMixedDecoratorsThatTakeAFunc(
            ActivationException ex)
        {
            AssertThat.StringContains(string.Format(
                "impossible for the container to generate a {0} for injection into the {1} decorator",
                    typeof(Func<ICommandHandler<RealCommand>>).ToFriendlyName(),
                    typeof(AsyncCommandHandlerProxy<>).ToFriendlyName()),
                ex.Message);

            AssertThat.StringContains(
                "the registration hasn't been made using one of the RegisterCollection overloads that take " +
                "a list of System.Type",
                ex.Message);

            AssertThat.StringContains(
                "switch to one of the other RegisterCollection overloads, or don't use a decorator that " +
                "depends on a Func<T>",
                ex.Message);
        }

        private static void Assert_IsNotAMutableCollection(object collection)
        {
            Type type = collection.GetType();

            Type genericTypeDefinition = type.IsGenericType() ? type.GetGenericTypeDefinition() : null;

            if (genericTypeDefinition == typeof(ReadOnlyCollection<>))
            {
                return;
            }

            if (genericTypeDefinition == typeof(ICollection<>) || type == typeof(IList) || type.IsArray)
            {
                Assert.Fail("The {0} is a mutable type", type.Name);
            }
        }

        public class RealNonGenericService : INonGenericService
        {
            public void DoSomething()
            {
            }
        }

        public class NonGenericServiceDecorator : INonGenericService
        {
            public NonGenericServiceDecorator(INonGenericService decorated)
            {
                this.DecoratedService = decorated;
            }

            public INonGenericService DecoratedService { get; }

            public void DoSomething()
            {
                this.DecoratedService.DoSomething();
            }
        }

        public class BaseDecorator : IBase
        {
            public readonly IBase Decoratee;

            public BaseDecorator(IBase decoratee)
            {
                this.Decoratee = decoratee;
            }
        }

        public class DeriveDecorator : IDerive
        {
            public readonly IDerive Decoratee;

            public DeriveDecorator(IDerive decoratee)
            {
                this.Decoratee = decoratee;
            }
        }
    }
}