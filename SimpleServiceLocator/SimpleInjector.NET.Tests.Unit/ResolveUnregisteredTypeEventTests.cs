namespace SimpleInjector.Tests.Unit
{
    using System;
    using System.Collections.Generic;
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

            var container = new Container();

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

            var container = new Container();

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

            var container = new Container();

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

            var container = new Container();

            container.RegisterSingle<IUserRepository>(new SqlUserRepository());

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
            string expectedMessage = 
                "No registration for type RealUserService could be found " + 
                "and an implicit registration could not be made. The constructor of the type contains the " +
                "parameter of type IUserRepository that is not registered. " +
                "Please ensure IUserRepository is registered in the container, " + 
                "or change the constructor of RealUserService.";

            // We don't register the required IUserRepository dependency.
            var container = new Container();

            try
            {
                // Act
                // RealUserService is a concrete class with a constructor with a single argument of type 
                // IUserRepository.
                container.GetInstance<RealUserService>();

                // Assert
                Assert.Fail("Exception expected.");
            }
            catch (Exception ex)
            {
                AssertThat.StringContains(expectedMessage, ex.Message);
            }
        }

        [TestMethod]
        public void GetInstance_WithConcreteUncreatableType_ResolvesTypeUsingEvent()
        {
            // Arrange
            var expectedInstance = new RealUserService(new SqlUserRepository());
            bool eventCalled = false;

            var container = new Container();

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
        [ExpectedException(typeof(InvalidOperationException),
            "Registration of an event after the container is locked is illegal.")]
        public void AddResolveUnregisteredType_AfterContainerHasBeenLocked_ThrowsAnException()
        {
            // Arrange
            var container = new Container();

            container.RegisterSingle<IUserRepository>(new SqlUserRepository());

            // The first use of the container locks the container.
            container.GetInstance<IUserRepository>();

            // Act
            container.ResolveUnregisteredType += (s, e) => { };
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException),
            "Removal of an event after the container is locked is illegal.")]
        public void RemoveResolveUnregisteredType_AfterContainerHasBeenLocked_ThrowsAnException()
        {
            // Arrange
            var container = new Container();

            container.RegisterSingle<IUserRepository>(new SqlUserRepository());

            // The first use of the container locks the container.
            container.GetInstance<IUserRepository>();

            // Act
            container.ResolveUnregisteredType -= (s, e) => { };
        }

        [TestMethod]
        public void RemoveResolveUnregisteredType_BeforeContainerHasBeenLocked_Succeeds()
        {
            // Arrange
            bool handlerCalled = false;

            var container = new Container();

            EventHandler<UnregisteredTypeEventArgs> handler = (sender, e) =>
            {
                handlerCalled = true;
            };

            container.ResolveUnregisteredType += handler;

            container.RegisterSingle<IUserRepository>(new SqlUserRepository());
            
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
            
            var container = new Container();

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

            var container = new Container();

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
            var container = new Container();

            container.ResolveUnregisteredType += (s, e) =>
            {
                e.Register(() => { throw new Exception(); });
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
                const string AssertMessage = "Exception message was not descriptive.";

                AssertThat.StringContains("ResolveUnregisteredType", ex.Message, AssertMessage);
                AssertThat.StringContains("registered a delegate that threw an exception", ex.Message, 
                    AssertMessage);
            }
        }

        [TestMethod]
        public void GetInstance_EventRegisteredForNonRootTypeThatThrowsException_ThrowsAnDescriptiveException()
        {
            // Arrange
            var container = new Container();

            container.Register<UserServiceBase, RealUserService>();

            container.ResolveUnregisteredType += (s, e) =>
            {
                e.Register(() => { throw new Exception(); });
            };

            try
            {
                // Act
                container.GetInstance<UserServiceBase>();

                // Assert
                Assert.Fail("Exception was expected.");
            }
            catch (Exception ex)
            {
                const string AssertMessage = "Exception message was not descriptive.";

                AssertThat.StringContains("ResolveUnregisteredType", ex.Message, AssertMessage);
                AssertThat.StringContains("registered a delegate that threw an exception", ex.Message,
                    AssertMessage);
            }
        }

        [TestMethod]
        public void GetInstance_EventRegisteredWithInvalidExpression_ThrowsAnDescriptiveException()
        {
            // Arrange
            var container = new Container();

            container.ResolveUnregisteredType += (s, e) =>
            {
                var invalidExpression = Expression.GreaterThan(Expression.Constant(1), Expression.Constant(1));

                e.Register(invalidExpression);
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
                const string AssertMessage = "Exception message was not descriptive.";

                AssertThat.StringContains("Error occurred while trying to build a delegate for type", 
                    ex.Message, AssertMessage);
                AssertThat.StringContains("UnregisteredTypeEventArgs.Register(Expression)", ex.Message,
                    AssertMessage);
                AssertThat.StringContains("Expression of type 'System.Boolean' cannot be used for return type", 
                    ex.Message, AssertMessage);
            }
        }

        [TestMethod]
        public void GetInstance_EventRegisteredThatThrowsException_ThrowsExceptionWithExpectedInnerException()
        {
            // Arrange
            var expectedInnerException = new NullReferenceException();

            var container = new Container();

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
                Assert.IsNotNull(ex.InnerException, "No inner exception was supplied.");
                Assert.AreEqual(expectedInnerException, ex.InnerException,
                    "The supplied inner exception was not the expected inner exception");
            }
        }

        [TestMethod]
        public void GetInstance_EventRegisteredThatReturnsNull_ThrowsExceptionWithExpectedMessage()
        {
            // Arrange
            var container = new Container();

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

                AssertThat.StringContains("ResolveUnregisteredType", ex.Message, AssertMessage);
                AssertThat.StringContains("registered a delegate that returned a null reference", ex.Message, 
                    AssertMessage);
            }
        }

        [TestMethod]
        public void GetInstance_EventRegisteredThatReturnsANonAssignableType_ThrowsExceptionWithExpectedMessage()
        {
            // Arrange
            var container = new Container();

            container.ResolveUnregisteredType += (s, e) =>
            {
                if (e.UnregisteredServiceType == typeof(IUserRepository))
                {
                    // RealUserService does not implement IUserRepository
                    e.Register(() => new RealUserService(null));
                }
            };

            try
            {
                // Act
                container.GetInstance<RealUserService>();

                // Assert
                Assert.Fail("Exception was expected.");
            }
            catch (Exception ex)
            {
                const string AssertMessage = "Exception message was not descriptive enough. Actual message: ";

                Assert.IsTrue(ex.Message.Contains("ResolveUnregisteredType"), AssertMessage + ex.Message);
                Assert.IsTrue(ex.Message.Contains("registered a delegate that created an instance of type "),
                    AssertMessage + ex.Message);
                Assert.IsTrue(ex.Message.Contains("that can not be cast to the specified service type"),
                    AssertMessage + ex.Message);
            }
        }

        [TestMethod]
        public void GetAllInstances_OnUnregisteredType_TriggersUnregisteredTypeResolution()
        {
            // Arrange
            bool resolveUnregisteredTypeWasTriggered = false;

            var container = new Container();
            
            container.ResolveUnregisteredType += (s, e) =>
            {
                if (e.UnregisteredServiceType == typeof(IEnumerable<Exception>))
                {
                    resolveUnregisteredTypeWasTriggered = true;
                }
            };

            // Act
            container.GetAllInstances<Exception>();

            // Assert
            Assert.IsTrue(resolveUnregisteredTypeWasTriggered);
        }

        [TestMethod]
        public void GetAllInstancesByType_OnUnregisteredType_TriggersUnregisteredTypeResolution()
        {
            // Arrange
            bool resolveUnregisteredTypeWasTriggered = false;

            var container = new Container();

            container.ResolveUnregisteredType += (s, e) =>
            {
                if (e.UnregisteredServiceType == typeof(IEnumerable<Exception>))
                {
                    resolveUnregisteredTypeWasTriggered = true;
                }
            };

            // Act
            container.GetAllInstances(typeof(Exception));

            // Assert
            Assert.IsTrue(resolveUnregisteredTypeWasTriggered);
        }

        [TestMethod]
        public void GetInstance_OnUnregisteredCollection_TriggersUnregisteredTypeResolution()
        {
            // Arrange
            bool resolveUnregisteredTypeWasTriggered = false;

            var container = new Container();

            container.ResolveUnregisteredType += (s, e) =>
            {
                if (e.UnregisteredServiceType == typeof(IEnumerable<Exception>))
                {
                    resolveUnregisteredTypeWasTriggered = true;
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

            var container = new Container();

            container.ResolveUnregisteredType += (s, e) =>
            {
                if (e.UnregisteredServiceType == typeof(IEnumerable<Exception>))
                {
                    resolveUnregisteredTypeWasTriggered = true;
                }
            };

            // Act
            container.GetInstance<Wrapper<Exception>>();

            // Assert
            Assert.IsTrue(resolveUnregisteredTypeWasTriggered);
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