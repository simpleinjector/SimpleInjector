using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SimpleInjector.Tests.Unit
{
    [TestClass]
    public class SilverlightSpecificTests
    {
        public interface IInterface
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

            container.Register<IInterface, InternalType>();

            // Act
            container.GetInstance<IInterface>();
        }

        [TestMethod]
        public void GetInstance_ResolvingAnInternalTypeWithRegisteredDependency_Succeeds()
        {
            // Arrange
            var container = new Container();

            container.Register<IInterface, InternalType>();

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

            container.RegisterSingle<InternalType>();

            // Act
            container.GetInstance<InternalOuterTypeDependingOnConcrete>();
        }

        private class InternalType : IInterface
        {
        }

        private class InternalOuterTypeDependingOnInterface
        {
            public InternalOuterTypeDependingOnInterface(IInterface internalInstance)
            {
            }
        }

        private class InternalOuterTypeDependingOnConcrete
        {
            public InternalOuterTypeDependingOnConcrete(InternalType internalInstance)
            {
            }
        }
    }
}