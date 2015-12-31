namespace SimpleInjector.Tests.Unit
{
    using System;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class AmbiguousTypesTests
    {
        [TestMethod]
        public void RegisterFunc_SuppliedWithAmbiguousTypeString_ThrowsExpectedException()
        {
            // Arrange
            var container = ContainerFactory.New();

            // Act
            Assert_RegistrationFailsWithExpectedAmbiguousMessage("String", () =>
            {
                container.Register<string>(() => "some value");
            });
        }

        [TestMethod]
        public void RegisterFunc_SuppliedWithAmbiguousTypeType_ThrowsExpectedException()
        {
            // Arrange
            var container = ContainerFactory.New();

            // Act
            Assert_RegistrationFailsWithExpectedAmbiguousMessage("Type", () =>
            {
                container.Register<Type>(() => typeof(int));
            });
        }

        [TestMethod]
        public void RegisterSingleFunc_SuppliedWithAmbiguousType_ThrowsExpectedException()
        {
            // Arrange
            var container = ContainerFactory.New();

            // Act
            Assert_RegistrationFailsWithExpectedAmbiguousMessage("String", () =>
            {
                container.Register<string>(() => "some value", Lifestyle.Singleton);
            });
        }

        [TestMethod]
        public void RegisterSingleValue_SuppliedWithAmbiguousType_ThrowsExpectedException()
        {
            // Arrange
            var container = ContainerFactory.New();

            // Act
            Assert_RegistrationFailsWithExpectedAmbiguousMessage("String", () =>
            {
                container.RegisterSingleton<string>("some value");
            });
        }

        [TestMethod]
        public void RegisterFunc_SuppliedWithAmbiguousType_ThrowsExceptionWithExpectedParamName()
        {
            // Arrange
            var container = ContainerFactory.New();
            
            // Assert
            Assert_RegistrationFailsWithExpectedParamName("TService", () =>
            {
                // Act
                container.Register<string>(() => "some value");
            });
        }

        [TestMethod]
        public void RegisterSingleFunc_SuppliedWithAmbiguousType_ThrowsExceptionWithExpectedParamName()
        {
            // Arrange
            var container = ContainerFactory.New();

            // Assert
            Assert_RegistrationFailsWithExpectedParamName("TService", () =>
            {
                // Act
                container.Register<string>(() => "some value", Lifestyle.Singleton);
            });
        }

        [TestMethod]
        public void RegisterSingleValue_SuppliedWithAmbiguousType_ThrowsExceptionWithExpectedParamName()
        {
            // Arrange
            var container = ContainerFactory.New();

            // Assert
            Assert_RegistrationFailsWithExpectedParamName("TService", () =>
            {
                // Act
                container.RegisterSingleton<string>("some value");
            });
        }

        private static void Assert_RegistrationFailsWithExpectedParamName(string paramName, Action action)
        {
            try
            {
                // Act
                action();

                // Assert
                Assert.Fail("Exception expected.");
            }
            catch (ArgumentException ex)
            {
                AssertThat.ExceptionContainsParamName("TService", ex);
            }
        }

        private static void Assert_RegistrationFailsWithExpectedAmbiguousMessage(string typeName, Action action)
        {
            try
            {
                // Act
                action();

                // Assert
                Assert.Fail("Exception expected.");
            }
            catch (ArgumentException ex)
            {
                string message = @"
                    You are trying to register " + typeName + @" as a service type, but registering this type
                    is not allowed to be registered because the type is ambiguous";

                AssertThat.ExceptionMessageContains(message.TrimInside(), ex);
            }
        }
    }
}