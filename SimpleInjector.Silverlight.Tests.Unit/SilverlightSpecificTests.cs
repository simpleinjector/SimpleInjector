namespace SimpleInjector.Tests.Unit
{
    using System;
    using System.Collections;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class SilverlightSpecificTests
    {
        private const string ExpectedSandboxFailureExpectedMessage = "Explicitly register the type using " +
            "one of the generic 'Register' overloads or consider making it public.";

        public interface IPublicService
        {
        }

        private interface IInternalService
        {
        }

        [TestMethod]
        public void GetInstance_ResolvingAnConcreteUnregisteredInternalType_Succeeds()
        {
            // Arrange
            var container = new Container();

            // Act
            container.GetInstance<InternalType>();
        }

        [TestMethod]
        public void GetInstance_ResolvingAnInternalType_Succeeds()
        {
            // Arrange
            var container = new Container();

            container.Register<IPublicService, InternalType>();

            // Act
            container.GetInstance<IPublicService>();
        }

        [TestMethod]
        public void GetInstance_ResolvingAnInternalTypeWithRegisteredDependency_Succeeds()
        {
            // Arrange
            var container = new Container();

            container.Register<IPublicService, InternalType>();

            // Act
            container.GetInstance<InternalOuterTypeDependingOnInterface>();
        }

        [TestMethod]
        public void GetInstance_ResolvingAnInternalTypeWithUnregisteredConcreteDependency_FailsWithExpectedExceptionMessage()
        {
            // Arrange
            string expectedMessage = "Explicitly register the type using one of the generic 'Register' " + 
                "overloads or consider making it public.";

            var container = new Container();

            try
            {
                // Act
                container.GetInstance<InternalOuterTypeDependingOnConcrete>();

                // Assert
                Assert.Fail("The call is expected to fail inside a Silverlight sandbox.");
            }
            catch (ActivationException ex)
            {
                AssertThat.StringContains(expectedMessage, ex.Message);
            }
        }

        [TestMethod]
        public void GetInstance_ResolvingAnInternalTypeWithRegisteredConcreteDependency_Succeeds()
        {
            // Arrange
            var container = new Container();

            container.Register<InternalType>(Lifestyle.Singleton);

            // Act
            container.GetInstance<InternalOuterTypeDependingOnConcrete>();
        }

        [TestMethod]
        public void RegisterSingleByType_RegisteringAnInternalType_ThrowsExpectedException()
        {
            // Arrange
            string expectedMessage = ExpectedSandboxFailureExpectedMessage;

            var container = new Container();

            try
            {
                // Act
                container.Register(typeof(IPublicService), typeof(InternalImplOfPublicService), Lifestyle.Singleton);

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
                container.Register(typeof(IInternalService), creator, Lifestyle.Singleton);

                Assert.Fail("The call is expected to fail inside a Silverlight sandbox.");
            }
            catch (ArgumentException ex)
            {
                // Assert
                AssertThat.StringContains(expectedMessage, ex.Message);
            }
        }

        [TestMethod]
        public void RegisterSingleByInstance_RegisteringAnInternalServiceType_FailsWithExpectedException()
        {
            // Arrange
            string expectedMessage = ExpectedSandboxFailureExpectedMessage;

            var container = new Container();

            IInternalService expectedSingleton = new InternalServiceImpl(null);

            container.RegisterInstance(typeof(IInternalService), expectedSingleton);

            try
            {
                // Act
                object actualInstance = container.GetInstance<IInternalService>();
                container.GetInstance(typeof(IInternalService));

                Assert.Fail("The call is expected to fail inside a Silverlight sandbox.");
            }
            catch (Exception ex)
            {
                // Assert
                AssertThat.StringContains(expectedMessage, ex.Message);
            }
        }

        [TestMethod]
        public void GetInstance_ResolvingAnInternalImplementationWithInitializerForPublicAbstraction_Succeeds()
        {
            // Arrange
            var container = new Container();

            container.RegisterInitializer<InternalImplOfPublicService>(impl => { });

            IPublicService expectedSingleton = new InternalImplOfPublicService(null);

            container.RegisterInstance(typeof(IPublicService), expectedSingleton);

            // Act
            container.GetInstance(typeof(IPublicService));
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
                container.RegisterCollection(typeof(IInternalService), instances);

                Assert.Fail("The call is expected to fail inside a Silverlight sandbox.");
            }
            catch (ArgumentException ex)
            {
                // Assert
                AssertThat.StringContains(expectedMessage, ex.Message);
            }
        }

        public sealed class Dependency
        {
        }

        private class InternalType : IPublicService
        {
        }

        private class InternalOuterTypeDependingOnInterface
        {
            public InternalOuterTypeDependingOnInterface(IPublicService internalInstance)
            {
            }
        }

        private class InternalOuterTypeDependingOnConcrete
        {
            public InternalOuterTypeDependingOnConcrete(InternalType internalInstance)
            {
            }
        }

        private sealed class InternalImplOfPublicService : IPublicService
        {
            public InternalImplOfPublicService(Dependency dependency)
            {
            }
        }

        private sealed class InternalServiceImpl : IInternalService
        {
            public InternalServiceImpl(Dependency dependency)
            {
            }
        }
    }
}