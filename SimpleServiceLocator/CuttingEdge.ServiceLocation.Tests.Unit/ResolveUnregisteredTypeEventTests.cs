using System;

using Microsoft.Practices.ServiceLocation;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CuttingEdge.ServiceLocation.Tests.Unit
{
    [TestClass]
    public class ResolveUnregisteredTypeEventTests
    {
        [TestMethod]
        public void GetInstance_WithEventRegistered_RegistersExpectedDelegate()
        {
            // Arrange
            var expectedInstance = new Katana();

            var container = new SimpleServiceLocator();

            container.ResolveUnregisteredType += (s, e) =>
            {
                if (e.UnregisteredServiceType == typeof(IWeapon))
                {
                    Func<object> weaponCreator = () => expectedInstance;

                    e.Register(weaponCreator);
                }
            };

            // Act
            var actualInstance = container.GetInstance<IWeapon>();

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

            var container = new SimpleServiceLocator();

            container.ResolveUnregisteredType += (s, e) => e.Register(() => new Katana());
            container.ResolveUnregisteredType += (s, e) => e.Register(() => new Tanto());

            try
            {
                // Act
                container.GetInstance<IWeapon>();

                // Assert
                Assert.Fail("Exception expected.");
            }
            catch (ActivationException ex)
            {
                Assert.IsTrue(ex.Message.Contains(expectedMessage), "Actual message: " + expectedMessage);                
            }
        }

        [TestMethod]
        public void GetInstance_WithUnregisteredType_InvokesEvent()
        {
            // Arrange
            bool eventCalled = false;

            var container = new SimpleServiceLocator();

            container.ResolveUnregisteredType += (s, e) => { eventCalled = true; };

            try
            {
                // Act
                container.GetInstance<IWeapon>();

                // Assert
                Assert.Fail("Exception was expected.");
            }
            catch
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

            var container = new SimpleServiceLocator();

            container.RegisterSingle<IWeapon>(new Katana());

            container.ResolveUnregisteredType += (s, e) => { eventCalled = true; };

            // Act
            // Samurai is a concrete type with IWeapon as constructor dependency.
            container.GetInstance<Samurai>();

            // Assert
            Assert.IsTrue(eventCalled, "The container should first try unregistered type resolution before " +
                "trying to create a concrete type.");
        }

        [TestMethod]
        public void GetInstance_UnregisteredConcreteTypeWithUnregistedDependencies_ThrowsExpectedException()
        {
            // Arrange
            string expectedMessage = 
                "No registration for type CuttingEdge.ServiceLocation.Tests.Unit.Samurai could be found " + 
                "and an implicit registration could not be made. The constructor of the type contains the " +
                "parameter of type CuttingEdge.ServiceLocation.Tests.Unit.IWeapon that is not registered. " +
                "Please ensure IWeapon is registered in the container, change the constructor of the type " +
                "or register the type Samurai directly.";

            // We don't register the required IWeapon dependency.
            var container = new SimpleServiceLocator();

            try
            {
                // Act
                // Samurai is a concrete class with a constructor with a single argument of type IWeapon.
                container.GetInstance<Samurai>();

                // Assert
                Assert.Fail("Exception expected.");
            }
            catch (Exception ex)
            {
                Assert.IsTrue(ex.Message.Contains(expectedMessage), "Expected message not found. Actual: " +
                    ex.Message);
            }
        }

        [TestMethod]
        public void GetInstance_WithConcreteUncreatableType_ResolvesTypeUsingEvent()
        {
            // Arrange
            var expectedInstance = new Samurai(new Katana());
            bool eventCalled = false;

            var container = new SimpleServiceLocator();

            container.ResolveUnregisteredType += (s, e) =>
            {
                e.Register(() => expectedInstance);
                eventCalled = true;
            };

            // Act
            // IWeapon wasn't registered. Therefore, the concrete Samurai type can not be created without
            // unregistered type resolution.
            var actualInstance = container.GetInstance<Samurai>();

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
            var container = new SimpleServiceLocator();

            container.RegisterSingle<IWeapon>(new Katana());

            // The first use of the container locks the container.
            container.GetInstance<IWeapon>();

            // Act
            container.ResolveUnregisteredType += (s, e) => { };
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException),
            "Removal of an event after the container is locked is illegal.")]
        public void RemoveResolveUnregisteredType_AfterContainerHasBeenLocked_ThrowsAnException()
        {
            // Arrange
            var container = new SimpleServiceLocator();

            container.RegisterSingle<IWeapon>(new Katana());

            // The first use of the container locks the container.
            container.GetInstance<IWeapon>();

            // Act
            container.ResolveUnregisteredType -= (s, e) => { };
        }

        [TestMethod]
        public void RemoveResolveUnregisteredType_BeforeContainerHasBeenLocked_Succeeds()
        {
            // Arrange
            var container = new SimpleServiceLocator();

            container.RegisterSingle<IWeapon>(new Katana());

            var tester = new HandleTest();

            container.ResolveUnregisteredType += tester.Handle;

            // Act
            container.ResolveUnregisteredType -= tester.Handle;

            try
            {
                container.GetInstance<IDisposable>();
            }
            catch
            {
                // We expect an exception
            }

            // Assert
            Assert.IsFalse(tester.HandlerCalled, "The delegate was not removed correctly.");
        }

        [TestMethod]
        public void GetInstance_CalledMultipleTimesEventRegistered_WillCallEventJustOnce()
        {
            // Arrange
            const int ExpectedEventCallCount = 1;
            int actualEventCallCount = 0;
            
            var container = new SimpleServiceLocator();

            container.ResolveUnregisteredType += (s, e) =>
            {
                actualEventCallCount++;
                e.Register(() => new Katana());
            };

            // Act
            container.GetInstance<IWeapon>();
            container.GetInstance<IWeapon>();

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

            var container = new SimpleServiceLocator();

            container.ResolveUnregisteredType += (s, e) => delegate1Called = true;
            container.ResolveUnregisteredType += (s, e) => delegate2Called = true;

            try
            {
                // Act
                container.GetInstance<IWeapon>();

                // Assert
                Assert.Fail("Exception was expected, because weapon could not be resolved.");
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
            var container = new SimpleServiceLocator();

            container.ResolveUnregisteredType += (s, e) =>
            {
                e.Register(() => { throw new Exception(); });
            };

            try
            {
                // Act
                container.GetInstance<IWeapon>();

                // Assert
                Assert.Fail("Exception was expected.");
            }
            catch (Exception ex)
            {
                const string AssertMessage = "Exception message was not descriptive. Actual message: ";

                Assert.IsTrue(ex.Message.Contains("ResolveUnregisteredType"), AssertMessage + ex.Message);
                Assert.IsTrue(ex.Message.Contains("registered a delegate that threw an exception"),
                    AssertMessage + ex.Message);
            }
        }

        [TestMethod]
        public void GetInstance_EventRegisteredThatThrowsException_ThrowsExceptionWithExpectedInnerException()
        {
            // Arrange
            var expectedInnerException = new NullReferenceException();

            var container = new SimpleServiceLocator();

            container.ResolveUnregisteredType += (s, e) =>
            {
                e.Register(() => { throw expectedInnerException; });
            };

            try
            {
                // Act
                container.GetInstance<IWeapon>();

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
            var container = new SimpleServiceLocator();

            container.ResolveUnregisteredType += (s, e) =>
            {
                e.Register(() => null);
            };

            try
            {
                // Act
                container.GetInstance<IWeapon>();

                // Assert
                Assert.Fail("Exception was expected.");
            }
            catch (Exception ex)
            {
                const string AssertMessage = "Exception message was not descriptive. Actual message: ";

                Assert.IsTrue(ex.Message.Contains("ResolveUnregisteredType"), AssertMessage + ex.Message);
                Assert.IsTrue(ex.Message.Contains("registered a delegate that returned a null reference"),
                    AssertMessage + ex.Message);
            }
        }

        [TestMethod]
        public void GetInstance_EventRegisteredThatReturnsANonAssignableType_ThrowsExceptionWithExpectedMessage()
        {
            // Arrange
            var container = new SimpleServiceLocator();

            container.ResolveUnregisteredType += (s, e) =>
            {
                e.Register(() => new Samurai(null));
            };

            try
            {
                // Act
                container.GetInstance<IWeapon>();

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