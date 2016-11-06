#pragma warning disable 0618
namespace SimpleInjector.Integration.Web.Tests.Unit
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Web;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SimpleInjector.Tests.Unit;

    [TestClass]
    public class WebRequestLifestyleTests
    {
        [TestMethod]
        public void WhenCurrentRequestEnds_WithValidArgumentsInHttpContext_Succeeds()
        {
            // Arrange
            var validContainer = new Container();
            Action validAction = () => { };

            using (new HttpContextScope())
            {
                // Act
                WebRequestLifestyle.WhenCurrentRequestEnds(validContainer, validAction);
            }
        }

        [TestMethod]
        public void WhenCurrentRequestEnds_WithNullContainer_ThrowsExpectedException()
        {
            // Arrange
            Container invalidContainer = null;

            using (new HttpContextScope())
            {
                // Act
                Action action = () => WebRequestLifestyle.WhenCurrentRequestEnds(invalidContainer, () => { });

                // Assert
                AssertThat.ThrowsWithParamName<ArgumentNullException>("container", action);
            }
        }

        [TestMethod]
        public void WhenCurrentRequestEnds_WithNullAction_ThrowsExpectedException()
        {
            // Arrange
            Action invalidAction = null;

            using (new HttpContextScope())
            {
                // Act
                Action action = () => WebRequestLifestyle.WhenCurrentRequestEnds(new Container(), invalidAction);

                // Assert
                AssertThat.ThrowsWithParamName<ArgumentNullException>("action", action);
            }
        }

        [TestMethod]
        public void WhenCurrentRequestEnds_OutsideTheContextOfAHttpRequest_ThrowsExpectedException()
        {
            // Act
            Action action = () => WebRequestLifestyle.WhenCurrentRequestEnds(new Container(), () => { });

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<InvalidOperationException>(
                "This method can only be called within the context of an active Web Request.", action);
        }
        
        [TestMethod]
        public void Verify_WhenCurrentRequestEndsCalledDuringVerificationOutsideAnHttpContext_Succeeds()
        {
            // Arrange
            bool initializerCalled = false;

            var container = new Container();

            container.Register<DisposableCommand>();

            container.RegisterInitializer<DisposableCommand>(command =>
            {
                WebRequestLifestyle.WhenCurrentRequestEnds(container, () => command.Dispose());
                initializerCalled = true;
            });

            // Act
            container.Verify(VerificationOption.VerifyOnly);

            // Arrange
            Assert.IsTrue(initializerCalled);
        }

        [TestMethod]
        public void RegisterForDisposal_WithValidArgumentsInHttpContext_Succeeds()
        {
            // Arrange
            ScopedLifestyle lifestyle = new WebRequestLifestyle();

            var validContainer = new Container();
            IDisposable validInstance = new DisposableCommand();

            using (new HttpContextScope())
            {
                // Act
                lifestyle.RegisterForDisposal(validContainer, validInstance);
            }
        }

        [TestMethod]
        public void RegisterForDisposal_WithNullContainer_ThrowsExpectedException()
        {
            // Arrange
            ScopedLifestyle lifestyle = new WebRequestLifestyle();

            Container invalidContainer = null;

            using (new HttpContextScope())
            {
                // Act
                Action action = () => lifestyle.RegisterForDisposal(invalidContainer, new DisposableCommand());

                // Assert
                AssertThat.ThrowsWithParamName<ArgumentNullException>("container", action);
            }
        }

        [TestMethod]
        public void RegisterForDisposal_WithNullAction_ThrowsExpectedException()
        {
            // Arrange
            ScopedLifestyle lifestyle = new WebRequestLifestyle();

            IDisposable invalidInstance = null;

            using (new HttpContextScope())
            {
                // Act
                Action action = () => lifestyle.RegisterForDisposal(new Container(), invalidInstance);

                // Assert
                AssertThat.ThrowsWithParamName<ArgumentNullException>("disposable", action);
            }
        }

        [TestMethod]
        public void RegisterForDisposal_OutsideTheContextOfAHttpRequest_ThrowsExpectedException()
        {
            // Arrange
            ScopedLifestyle lifestyle = new WebRequestLifestyle();

            var validContainer = new Container();
            var validInstance = new DisposableCommand();

            // Act
            Action action = () => lifestyle.RegisterForDisposal(validContainer, validInstance);

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<InvalidOperationException>(
                "This method can only be called within the context of an active Web Request.", action);
        }

        [TestMethod]
        public void Verify_RegisterForDisposalCalledDuringVerificationOutsideAnHttpContext_Succeeds()
        {
            // Arrange
            ScopedLifestyle lifestyle = new WebRequestLifestyle();

            bool initializerCalled = false;

            var container = new Container();

            container.Register<DisposableCommand>();

            container.RegisterInitializer<DisposableCommand>(command =>
            {
                lifestyle.RegisterForDisposal(container, command);
                initializerCalled = true;
            });

            // Act
            container.Verify(VerificationOption.VerifyOnly);

            // Arrange
            Assert.IsTrue(initializerCalled);
        }

        [TestMethod]
        public void WhenScopeEnds_WithValidArgumentsInHttpContext_Succeeds()
        {
            // Arrange
            ScopedLifestyle lifestyle = new WebRequestLifestyle();

            var validContainer = new Container();
            Action validAction = () => { };

            using (new HttpContextScope())
            {
                // Act
                lifestyle.WhenScopeEnds(validContainer, validAction);
            }
        }

        [TestMethod]
        public void WhenScopeEnds_WithNullContainer_ThrowsExpectedException()
        {
            // Arrange
            ScopedLifestyle lifestyle = new WebRequestLifestyle();

            Container invalidContainer = null;

            using (new HttpContextScope())
            {
                // Act
                Action action = () => lifestyle.WhenScopeEnds(invalidContainer, () => { });

                // Assert
                AssertThat.ThrowsWithParamName<ArgumentNullException>("container", action);
            }
        }

        [TestMethod]
        public void WhenScopeEnds_WithNullAction_ThrowsExpectedException()
        {
            // Arrange
            ScopedLifestyle lifestyle = new WebRequestLifestyle();

            Action invalidAction = null;
            
            using (new HttpContextScope())
            {
                // Act
                Action action = () => lifestyle.WhenScopeEnds(new Container(), invalidAction);

                // Assert
                AssertThat.ThrowsWithParamName<ArgumentNullException>("action", action);
            }
        }

        [TestMethod]
        public void WhenScopeEnds_OutsideTheContextOfAHttpRequest_ThrowsExpectedException()
        {
            // Arrange
            ScopedLifestyle lifestyle = new WebRequestLifestyle();

            // Act
            Action action = () => lifestyle.WhenScopeEnds(new Container(), () => { });

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<InvalidOperationException>(
                "This method can only be called within the context of an active Web Request.", action);
        }

        [TestMethod]
        public void SimpleInjectorHttpModuleDispose_Always_Succeeds()
        {
            // Arrange
            IHttpModule module = new SimpleInjectorHttpModule();

            // Act
            module.Dispose();
        }
    }
}