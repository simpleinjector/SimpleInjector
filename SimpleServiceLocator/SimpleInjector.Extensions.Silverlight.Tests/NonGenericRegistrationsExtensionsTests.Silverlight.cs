namespace SimpleInjector.Extensions.Tests.Unit
{
    using System;
    using System.Collections;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <content>
    /// Silverlight specific tests for the NonGenericRegistrationsExtensions class.
    /// </content>
    public partial class NonGenericRegistrationsExtensionsTests
    {
        private const string ExpectedSandboxFailureExpectedMessage = "Explicitly register the type using " +
            "one of the generic Register overloads or consider making it public.";

        [TestMethod]
        public void RegisterSingleByType_RegisteringAnInternalType_ThrowsExpectedException()
        {
            // Arrange
            string expectedMessage = ExpectedSandboxFailureExpectedMessage;

            var container = new Container();

            try
            {
                // Act
                container.RegisterSingle(typeof(IPublicService), typeof(InternalImplOfPublicService));

                Assert.Fail("The call is expected to fail inside a Silverlight sandbox.");
            }
            catch (ArgumentException ex)
            {
                // Assert
                AssertThat.StringContains(expectedMessage, ex.Message);
            }
        }

        [TestMethod]
        public void RegisterSingleByFunc_RegisteringAnInternalType_ThrowsExpectedException()
        {
            // Arrange
            string expectedMessage = ExpectedSandboxFailureExpectedMessage;

            var container = new Container();

            Func<object> creator = () => new InternalServiceImpl(null);

            try
            {
                // Act
                container.RegisterSingle(typeof(IInternalService), creator);

                Assert.Fail("The call is expected to fail inside a Silverlight sandbox.");
            }
            catch (ArgumentException ex)
            {
                // Assert
                AssertThat.StringContains(expectedMessage, ex.Message);
            }
        }

        [TestMethod]
        public void RegisterSingleByInstance_RegisteringAnInternalType_ThrowsExpectedException()
        {
            // Arrange
            string expectedMessage = ExpectedSandboxFailureExpectedMessage;

            var container = new Container();

            object instance = new InternalServiceImpl(null);

            try
            {
                // Act
                container.RegisterSingle(typeof(IInternalService), instance);

                Assert.Fail("The call is expected to fail inside a Silverlight sandbox.");
            }
            catch (ArgumentException ex)
            {
                // Assert
                AssertThat.StringContains(expectedMessage, ex.Message);
            }
        }

        [TestMethod]
        public void RegisterByType_RegisteringAnInternalType_ThrowsExpectedException()
        {
            // Arrange
            string expectedMessage = ExpectedSandboxFailureExpectedMessage;

            var container = new Container();

            try
            {
                // Act
                container.Register(typeof(IPublicService), typeof(InternalImplOfPublicService));

                Assert.Fail("The call is expected to fail inside a Silverlight sandbox.");
            }
            catch (ArgumentException ex)
            {
                // Assert
                AssertThat.StringContains(expectedMessage, ex.Message);
            }
        }

        [TestMethod]
        public void RegisterByFunc_RegisteringAnInternalType_ThrowsExpectedException()
        {
            // Arrange
            string expectedMessage = ExpectedSandboxFailureExpectedMessage;

            var container = new Container();

            Func<object> creator = () => new InternalServiceImpl(null);

            try
            {
                // Act
                container.Register(typeof(IInternalService), creator);

                Assert.Fail("The call is expected to fail inside a Silverlight sandbox.");
            }
            catch (ArgumentException ex)
            {
                // Assert
                AssertThat.StringContains(expectedMessage, ex.Message);
            }
        }

        [TestMethod]
        public void RegisterAll_RegisteringAnInternalType_ThrowsExpectedException()
        {
            // Arrange
            string expectedMessage = ExpectedSandboxFailureExpectedMessage;

            var container = new Container();

            IEnumerable instances = new[] { new InternalServiceImpl(null) };

            try
            {
                // Act
                container.RegisterAll(typeof(IInternalService), instances);

                Assert.Fail("The call is expected to fail inside a Silverlight sandbox.");
            }
            catch (ArgumentException ex)
            {
                // Assert
                AssertThat.StringContains(expectedMessage, ex.Message);
            }
        }
    }
}