#pragma warning disable 0618
namespace SimpleInjector.Tests.Unit
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class ResolveUnregisteredTypeEventTests
    {
        [TestMethod]
        public void GetInstance_WithEventRegistered_RegistersExpectedDelegate()
        {
            // Arrange
            var expectedInstance = new SqlUserRepository();

            var container = ContainerFactory.New();

            container.ResolveUnregisteredType += (s, e) =>
            {
                if (e.UnregisteredServiceType == typeof(IUserRepository))
                {
                    Func<object> repositoryCreator = () => expectedInstance;

                    e.Register(repositoryCreator);
                }
            };

            // Act
            var actualInstance = container.GetInstance<IUserRepository>();

            // Assert
            Assert.AreEqual(expectedInstance, actualInstance,
                "The container did not return the expected instance.");
        }

        [TestMethod]
        public void GetInstance_MultipleDelegatesHookedUpToEvent_FailsWhenBothDelegatesRegisterSameServiceType()
        {
            // Arrange
            string expectedMessage = "Multiple observers of the ResolveUnregisteredType event are " +
                "registering a delegate for the same service type";

            var container = ContainerFactory.New();

            container.ResolveUnregisteredType += (s, e) => e.Register(() => new SqlUserRepository());
            container.ResolveUnregisteredType += (s, e) => e.Register(() => new InMemoryUserRepository());

            try
            {
                // Act
                container.GetInstance<IUserRepository>();

                // Assert
                Assert.Fail("Exception expected.");
            }
            catch (ActivationException ex)
            {
                AssertThat.StringContains(expectedMessage, ex.Message);
            }
        }

        [TestMethod]
        public void GetInstance_WithUnregisteredType_InvokesEvent()
        {
            // Arrange
            bool eventCalled = false;

            var container = ContainerFactory.New();

            container.ResolveUnregisteredType += (s, e) => { eventCalled = true; };

            try
            {
                // Act
                container.GetInstance<IUserRepository>();

                // Assert
                Assert.Fail("Exception was expected.");
            }
            catch (ActivationException)
            {
                Assert.IsTrue(eventCalled, "Before throwing an exception, the container must try to resolve " +
                    "a missing type by calling the ResolveUnregisteredType event.");
            }
        }

        [TestMethod]
        public void GetInstance_WithConcreteType_InvokesEventsBeforeTryingToCreateTheConcreteType()
        {
            // Arrange
            bool eventCalled = false;

            var container = ContainerFactory.New();

            container.RegisterSingleton<IUserRepository>(new SqlUserRepository());

            container.ResolveUnregisteredType += (s, e) => { eventCalled = true; };

            // Act
            // RealUserService is a concrete type with IUserRepository as constructor dependency.
            container.GetInstance<RealUserService>();

            // Assert
            Assert.IsTrue(eventCalled, "The container should first try unregistered type resolution before " +
                "trying to create a concrete type.");
        }

        [TestMethod]
        public void GetInstance_UnregisteredConcreteTypeWithUnregistedDependencies_ThrowsExpectedException()
        {
            // Arrange
            string expectedMessage = @"
                No registration for type RealUserService could be found and an implicit registration could not 
                be made. The constructor of type RealUserService contains the parameter with name 'repository'
                and type IUserRepository that is not registered. Please ensure IUserRepository 
                is registered, or change the constructor of RealUserService."
                .TrimInside();

            // We don't register the required IUserRepository dependency.
            var container = ContainerFactory.New();

            // Act
            // RealUserService is a concrete class with a constructor with a single argument of type 
            // IUserRepository.
            Action action = () => container.GetInstance<RealUserService>();

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<Exception>(expectedMessage, action);
        }

        [TestMethod]
        public void GetInstance_WithConcreteUncreatableType_ResolvesTypeUsingEvent()
        {
            // Arrange
            var expectedInstance = new RealUserService(new SqlUserRepository());
            bool eventCalled = false;

            var container = ContainerFactory.New();

            container.ResolveUnregisteredType += (s, e) =>
            {
                e.Register(() => expectedInstance);
                eventCalled = true;
            };

            // Act
            // IUserRepository wasn't registered. Therefore, the concrete RealUserService type can not be 
            // created without unregistered type resolution.
            var actualInstance = container.GetInstance<RealUserService>();

            // Assert
            Assert.IsTrue(eventCalled, "The type is expected be resolved by the registered delegate.");
            Assert.AreEqual(expectedInstance, actualInstance);
        }

        [TestMethod]
        public void AddResolveUnregisteredType_AfterContainerHasBeenLocked_ThrowsAnException()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.RegisterSingleton<IUserRepository>(new SqlUserRepository());

            // The first use of the container locks the container.
            container.GetInstance<IUserRepository>();

            // Act
            Action action = () => container.ResolveUnregisteredType += (s, e) => { };

            // Assert
            AssertThat.Throws<InvalidOperationException>(action,
                "Registration of an event after the container is locked is illegal.");
        }

        [TestMethod]
        public void RemoveResolveUnregisteredType_AfterContainerHasBeenLocked_ThrowsAnException()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.RegisterSingleton<IUserRepository>(new SqlUserRepository());

            // The first use of the container locks the container.
            container.GetInstance<IUserRepository>();

            // Act
            Action action = () => container.ResolveUnregisteredType -= (s, e) => { };

            // Assert
            AssertThat.Throws<InvalidOperationException>(action,
                "Removal of an event after the container is locked is illegal.");
        }

        [TestMethod]
        public void RemoveResolveUnregisteredType_BeforeContainerHasBeenLocked_Succeeds()
        {
            // Arrange
            bool handlerCalled = false;

            var container = ContainerFactory.New();

            EventHandler<UnregisteredTypeEventArgs> handler = (sender, e) =>
            {
                handlerCalled = true;
            };

            container.ResolveUnregisteredType += handler;

            container.RegisterSingleton<IUserRepository>(new SqlUserRepository());

            // Act
            container.ResolveUnregisteredType -= handler;

            try
            {
                // Call an unregistered type to trigger the ResolveUnregisteredType event.
                container.GetInstance<IDisposable>();
            }
            catch
            {
                // We expect an exception
            }

            // Assert
            Assert.IsFalse(handlerCalled, "The delegate was not removed correctly.");
        }

        [TestMethod]
        public void GetInstance_CalledMultipleTimesEventRegistered_WillCallEventJustOnce()
        {
            // Arrange
            const int ExpectedEventCallCount = 1;
            int actualEventCallCount = 0;

            var container = ContainerFactory.New();

            container.ResolveUnregisteredType += (s, e) =>
            {
                actualEventCallCount++;
                e.Register(() => new SqlUserRepository());
            };

            // Act
            container.GetInstance<IUserRepository>();
            container.GetInstance<IUserRepository>();

            // Assert
            Assert.AreEqual(ExpectedEventCallCount, actualEventCallCount, "The event was only expected to " +
                "be called once, because the container is expected to store the delegate in its local " +
                "dictionary (for performance).");
        }

        [TestMethod]
        public void GetInstance_MultipleDelegatesHookedUpToEvent_CallsAllDelegates()
        {
            // Arrange
            bool delegate1Called = false;
            bool delegate2Called = false;

            var container = ContainerFactory.New();

            container.ResolveUnregisteredType += (s, e) => delegate1Called = true;
            container.ResolveUnregisteredType += (s, e) => delegate2Called = true;

            try
            {
                // Act
                container.GetInstance<IUserRepository>();

                // Assert
                Assert.Fail("Exception was expected, because the type could not be resolved.");
            }
            catch
            {
                Assert.IsTrue(delegate1Called, "Both delegates are expected to be called.");
                Assert.IsTrue(delegate2Called, "Both delegates are expected to be called.");
            }
        }

        [TestMethod]
        public void GetInstance_EventRegisteredThatThrowsException_ThrowsAnDescriptiveException()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.ResolveUnregisteredType += (s, e) =>
            {
                e.Register(() => { throw new Exception(); });
            };
            
            // Act
            Action action = () => container.GetInstance<IUserRepository>();

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<Exception>(
                "The delegate that was registered for service type IUserRepository using the " +
                "UnregisteredTypeEventArgs.Register(Func<object>) method threw an exception",
                action,
                "Exception message was not descriptive.");
        }

        [TestMethod]
        public void GetInstance_EventRegisteredForNonRootTypeThatThrowsException_ThrowsAnDescriptiveException()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Register<UserServiceBase, RealUserService>();

            container.ResolveUnregisteredType += (s, e) =>
            {
                e.Register(() => { throw new Exception(); });
            };

            // Act
            Action action = () => container.GetInstance<UserServiceBase>();

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<Exception>(
                "The delegate that was registered for service type IUserRepository using the " +
                "UnregisteredTypeEventArgs.Register(Func<object>) method threw an exception",
                action,
                "Exception message was not descriptive.");
        }

        [TestMethod]
        public void GetInstance_EventRegisteredWithInvalidExpression_ThrowsAnDescriptiveException()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.ResolveUnregisteredType += (s, e) =>
            {
                var invalidExpression = Expression.GreaterThan(Expression.Constant(1), Expression.Constant(1));

                e.Register(invalidExpression);
            };

            // Act
            Action action = () => container.GetInstance<IUserRepository>();

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<ArgumentException>(
                "The supplied expression of type Boolean does not implement IUserRepository",
                action);
        }

        [TestMethod]
        public void GetInstance_EventRegisteredThatThrowsException_ThrowsExceptionWithExpectedInnerException()
        {
            // Arrange
            var expectedInnerException = new NullReferenceException();

            var container = ContainerFactory.New();

            container.ResolveUnregisteredType += (s, e) =>
            {
                e.Register(() => { throw expectedInnerException; });
            };

            try
            {
                // Act
                container.GetInstance<IUserRepository>();

                // Assert
                Assert.Fail("Exception was expected.");
            }
            catch (ActivationException ex)
            {
                var innerExceptions = ex.InnerException.GetExceptionChain();

                Assert.IsNotNull(ex.InnerException, "No inner exception was supplied.");
                Assert.IsTrue(innerExceptions.Contains(expectedInnerException),
                    "The supplied inner exception was not the expected inner exception. " +
                    "Actual exception: " + ex.Message);
            }
        }

        [TestMethod]
        public void GetInstance_EventRegisteredThatReturnsNull_ThrowsExceptionWithExpectedMessage()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.ResolveUnregisteredType += (s, e) =>
            {
                e.Register(() => null);
            };

            try
            {
                // Act
                container.GetInstance<IUserRepository>();

                // Assert
                Assert.Fail("Exception was expected.");
            }
            catch (Exception ex)
            {
                const string AssertMessage = "Exception message was not descriptive. Actual message: ";

                AssertThat.ExceptionMessageContains(
                    "registered delegate for type IUserRepository returned null", ex, AssertMessage);
            }
        }

        [TestMethod]
        public void GetInstance_EventRegisteredThatReturnsANonAssignableType_ThrowsExceptionWithExpectedMessage()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.ResolveUnregisteredType += (s, e) =>
            {
                if (e.UnregisteredServiceType == typeof(IUserRepository))
                {
                    // RealUserService does not implement IUserRepository
                    e.Register(() => new RealUserService(null));
                }
            };

            // Act
            Action action = () => container.GetInstance<RealUserService>();

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<ActivationException>(@"
                The delegate that was registered for service type IUserRepository using the 
                UnregisteredTypeEventArgs.Register(Func<object>) method returned an object that couldn't
                be casted to IUserRepository. 
                Unable to cast object of type 'SimpleInjector.Tests.Unit.RealUserService' 
                to type 'SimpleInjector.Tests.Unit.IUserRepository'.".TrimInside(),
                action,
                "Exception message was not descriptive enough.");
        }

        [TestMethod]
        public void GetInstance_EventRegisteredThrowsAnException_ThrowsExceptionWithExpectedMessage()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.ResolveUnregisteredType += (s, e) =>
            {
                if (e.UnregisteredServiceType == typeof(IUserRepository))
                {
                    e.Register(() =>
                    {
                        throw new InvalidOperationException("Some stupid exception.");
                    });
                }
            };

            // Act
            Action action = () => container.GetInstance<RealUserService>();

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<ActivationException>(@"
                The delegate that was registered for service type IUserRepository using the 
                UnregisteredTypeEventArgs.Register(Func<object>) method threw an exception. 
                Some stupid exception.".TrimInside(),
                action,
                "Exception message was not descriptive enough.");
        }

        [TestMethod]
        public void GetAllInstances_OnUnregisteredType_TriggersUnregisteredTypeResolution()
        {
            // Arrange
            bool resolveUnregisteredTypeWasTriggered = false;

            var container = ContainerFactory.New();

            container.ResolveUnregisteredType += (s, e) =>
            {
                if (e.UnregisteredServiceType == typeof(IEnumerable<Exception>))
                {
                    resolveUnregisteredTypeWasTriggered = true;
                }
            };

            // Act
            Action action = () => container.GetAllInstances<Exception>();

            // Assert
            AssertThat.Throws<ActivationException>(action);
            Assert.IsTrue(resolveUnregisteredTypeWasTriggered);
        }

        [TestMethod]
        public void GetAllInstancesByType_OnUnregisteredType_TriggersUnregisteredTypeResolution()
        {
            Dictionary<Type, object> o = new Dictionary<Type, object>(40);
            o[typeof(object)] = 3;

            // Arrange
            bool resolveUnregisteredTypeWasTriggered = false;

            var container = ContainerFactory.New();

            container.ResolveUnregisteredType += (s, e) =>
            {
                if (e.UnregisteredServiceType == typeof(IEnumerable<Exception>))
                {
                    resolveUnregisteredTypeWasTriggered = true;
                }
            };

            // Act
            Action action = () => container.GetAllInstances(typeof(Exception));

            // Assert
            AssertThat.Throws<ActivationException>(action);
            Assert.IsTrue(resolveUnregisteredTypeWasTriggered);
        }

        [TestMethod]
        public void GetInstance_OnUnregisteredCollection_TriggersUnregisteredTypeResolution()
        {
            // Arrange
            bool resolveUnregisteredTypeWasTriggered = false;

            var container = ContainerFactory.New();

            container.ResolveUnregisteredType += (s, e) =>
            {
                if (e.UnregisteredServiceType == typeof(IEnumerable<Exception>))
                {
                    resolveUnregisteredTypeWasTriggered = true;
                    e.Register(() => Enumerable.Empty<Exception>());
                }
            };

            // Act
            container.GetInstance<IEnumerable<Exception>>();

            // Assert
            Assert.IsTrue(resolveUnregisteredTypeWasTriggered);
        }

        [TestMethod]
        public void GetInstance_OnInstanceDependingOnAnUnregisteredCollection_TriggersUnregisteredTypeResolution()
        {
            // Arrange
            bool resolveUnregisteredTypeWasTriggered = false;

            var container = ContainerFactory.New();

            container.ResolveUnregisteredType += (s, e) =>
            {
                if (e.UnregisteredServiceType == typeof(IEnumerable<Exception>))
                {
                    resolveUnregisteredTypeWasTriggered = true;

                    e.Register(() => Enumerable.Empty<Exception>());
                }
            };

            // Act
            container.GetInstance<ServiceDependingOn<IEnumerable<Exception>>>();

            // Assert
            Assert.IsTrue(resolveUnregisteredTypeWasTriggered);
        }

        // This test verifies the bug reported in work item 19847.
        [TestMethod]
        public void GetInstance_DependingOnAnArrayOfGenericElementsThatIsUnregistered_ThrowsAnExceptionThatCorrectlyFormatsThatArray()
        {
            // Arrange
            var container = ContainerFactory.New();

            // Act
            // CompositeService<T> depends on IGeneric<T>[] (array)
            Action action = () => container.GetInstance<CompositeService<int>>();

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<ActivationException>("Nullable<Int32>[]", action);
        }

        [TestMethod]
        public void ResolveUnregisteredType_Always_IsExpectedToBeCached()
        {
            // Arrange
            int callCount = 0;

            var container = ContainerFactory.New();

            container.Register<ComponentDependingOn<IUserRepository>>();
            container.RegisterSingleton<ServiceDependingOn<IUserRepository>>();

            var r = Lifestyle.Singleton.CreateRegistration<SqlUserRepository>(container);

            container.ResolveUnregisteredType += (s, e) =>
            {
                if (e.UnregisteredServiceType == typeof(IUserRepository))
                {
                    callCount++;

                    e.Register(r);
                }
            };

            // Act
            container.Verify();

            // Assert
            Assert.AreEqual(1, callCount, "The result of ResolveUnregisteredType is expected to be cached.");
        }

        public class CompositeService<T> where T : struct
        {
            public CompositeService(Nullable<T>[] dependencies)
            {
            }
        }

        private sealed class Wrapper<T>
        {
            public Wrapper(IEnumerable<T> wrappedCollection)
            {
            }
        }

        private sealed class HandleTest
        {
            public bool HandlerCalled { get; private set; }

            public void Handle(object sender, UnregisteredTypeEventArgs e)
            {
                this.HandlerCalled = true;
            }
        }
    }
}
#pragma warning restore 0618