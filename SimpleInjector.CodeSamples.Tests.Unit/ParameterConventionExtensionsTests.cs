namespace SimpleInjector.CodeSamples.Tests.Unit
{
    using System;
    using System.Configuration;
    using System.Data.SqlClient;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SimpleInjector.Tests.Unit;

    [TestClass]
    public class ParameterConventionExtensionsTests
    {
        [TestMethod]
        public void ConnectionStringsConvention_RegisteringTypeWithConnectionStringParameter_Succeeds()
        {
            // Arrange
            var container = CreateContainerWithConventions(new ConnectionStringsConvention());

            // Act
            container.Register<TypeWithConnectionStringConstructorArgument>();
        }

        [TestMethod]
        public void ConnectionStringsConvention_ResolvingTypeWithConnectionStringParameter_InjectsExpectedValue()
        {
            // Arrange
            string expectedConnectionString = ConfigurationManager.ConnectionStrings["cs1"].ConnectionString;

            var container = CreateContainerWithConventions(new ConnectionStringsConvention());

            // Act
            var instance = container.GetInstance<TypeWithConnectionStringConstructorArgument>();

            // Assert
            Assert.AreEqual(expectedConnectionString, instance.ConnectionString);
        }

        [TestMethod]
        public void ConnectionStringsConvention_RegisteringTypeWithIntParameterWhichNameEndsWithConnectionString_FailsWithExpectedException()
        {
            // Arrange
            var container = CreateContainerWithConventions(new ConnectionStringsConvention());

            try
            {
                // Act
                container.Register<TypeWithConnectionStringIntConstructorArgument>();

                // Assert
                Assert.Fail("Exception expected.");
            }
            catch (ArgumentException ex)
            {
                AssertThat.StringContains("The constructor of type " +
                    "ParameterConventionExtensionsTests.TypeWithConnectionStringIntConstructorArgument contains " +
                    "parameter 'cs1ConnectionString' of type Int32 which can not be used for constructor " +
                    "injection because it is a value type.", ex.Message);
            }
        }

        [TestMethod]
        public void ConnectionStringsConvention_ResolvingTypeWithIntParameterWhichNameEndsWithConnectionString_FailsWithExpectedException()
        {
            // Arrange
            var container = CreateContainerWithConventions(new ConnectionStringsConvention());

            try
            {
                // Act
                container.GetInstance<TypeWithConnectionStringIntConstructorArgument>();

                // Assert
                Assert.Fail("Exception expected.");
            }
            catch (ActivationException ex)
            {
                AssertThat.StringContains("The constructor of type " +
                    "ParameterConventionExtensionsTests.TypeWithConnectionStringIntConstructorArgument contains " +
                    "parameter 'cs1ConnectionString' of type Int32 which can not be used for constructor " +
                    "injection because it is a value type.", ex.Message);
            }
        }

        [TestMethod]
        public void ConnectionStringsConvention_RegisteringTypeWithNonExistingConnectionStringParameter_FailsWithExpectedException()
        {
            // Arrange
            var container = CreateContainerWithConventions(new ConnectionStringsConvention());

            try
            {
                // Act
                container.Register<TypeWithNonExistingConnectingStringConstructorArgument>();

                // Assert
                Assert.Fail("Exception was expected.");
            }
            catch (ArgumentException ex)
            {
                AssertThat.StringContains(
                    "No connection string with name 'doesNotExist' could be found", ex.Message);
            }
        }

        [TestMethod]
        public void ConnectionStringsConvention_ResolvingTypeWithNonExistingConnectionStringParameter_FailsWithExpectedException()
        {
            // Arrange
            var container = CreateContainerWithConventions(new ConnectionStringsConvention());

            try
            {
                // Act
                container.GetInstance<TypeWithNonExistingConnectingStringConstructorArgument>();

                // Assert
                Assert.Fail("Exception was expected.");
            }
            catch (ActivationException ex)
            {
                AssertThat.StringContains(
                    "No connection string with name 'doesNotExist' could be found", ex.Message);
            }
        }

        [TestMethod]
        public void AppSettingsConvention_RegisteringTypeWithStringAppSettingParameter_Succeeds()
        {
            // Arrange
            var container = CreateContainerWithConventions(new AppSettingsConvention());

            // Act
            container.Register<TypeWithAppSettingsStringConstructorArgument>();
        }

        [TestMethod]
        public void AppSettingsConvention_ResolvingTypeWithStringAppSettingParameter_InjectsExpectedValue()
        {
            // Arrange
            string expectedAppSetting = ConfigurationManager.AppSettings["as1"];

            var container = CreateContainerWithConventions(new AppSettingsConvention());

            // Act
            var instance = container.GetInstance<TypeWithAppSettingsStringConstructorArgument>();

            // Assert
            Assert.AreEqual(expectedAppSetting, instance.AppSetting);
        }

        [TestMethod]
        public void AppSettingsConvention_RegisteringTypeWithGuidAppSettingParameter_Succeeds()
        {
            // Arrange
            var container = CreateContainerWithConventions(new AppSettingsConvention());

            // Act
            container.Register<TypeWithAppSettingsStringConstructorArgument>();
        }

        [TestMethod]
        public void AppSettingsConvention_ResolvingTypeWithGuidAppSettingParameter_InjectsExpectedValue()
        {
            // Arrange
            Guid expectedAppSetting = Guid.Parse(ConfigurationManager.AppSettings["as2"]);

            var container = CreateContainerWithConventions(new AppSettingsConvention());

            // Act
            var instance = container.GetInstance<TypeWithAppSettingsGuidConstructorArgument>();

            // Assert
            Assert.AreEqual(expectedAppSetting, instance.AppSetting);
        }

        [TestMethod]
        public void AppSettingsConvention_RegisteringTypeWithReferenceTypeParameterWhichNameEndsWithAppSetting_Succeeds()
        {
            // Arrange
            var container = CreateContainerWithConventions(new AppSettingsConvention());

            // Act
            // This type depends on an reference type and since reference types are not handled by the
            // AppSettingsConvention, it should fall back on the default behavior, and since reference types
            // can be resolved by the container, the registration is therefore valid.
            container.Register<TypeWithAppSettingConstructorArgumentOfReferenceType>();
        }

        [TestMethod]
        public void AppSettingsConvention_ResolvingTypeWithReferenceTypeParameterWhichNameEndsWithConnectionString_FailsWithExpectedException()
        {
            // Arrange
            var container = CreateContainerWithConventions(new AppSettingsConvention());

            try
            {
                // Act
                container.GetInstance<TypeWithAppSettingConstructorArgumentOfReferenceType>();

                // Assert
                Assert.Fail("Exception expected.");
            }
            catch (ActivationException ex)
            {
                AssertThat.StringContains("The constructor of type " +
                    "ParameterConventionExtensionsTests.TypeWithAppSettingConstructorArgumentOfReferenceType " +
                    "contains the parameter of type IDisposable with name 'as1AppSetting' that is not " +
                    "registered.", ex.Message);
            }
        }

        [TestMethod]
        public void AppSettingsConvention_RegisteringTypeWithNonExistingAppSettingsParameter_FailsWithExpectedException()
        {
            // Arrange
            var container = CreateContainerWithConventions(new AppSettingsConvention());

            try
            {
                // Act
                container.Register<TypeWithNonExistingAppSettingConstructorArgument>();

                // Assert
                Assert.Fail("Exception was expected.");
            }
            catch (ArgumentException ex)
            {
                AssertThat.StringContains("No application setting with key 'doesNotExist' could be found", ex.Message);
            }
        }

        [TestMethod]
        public void AppSettingsConvention_ResolvingTypeWithNonExistingConnectionStringParameter_FailsWithExpectedException()
        {
            // Arrange
            var container = CreateContainerWithConventions(new AppSettingsConvention());

            try
            {
                // Act
                container.GetInstance<TypeWithNonExistingAppSettingConstructorArgument>();

                // Assert
                Assert.Fail("Exception was expected.");
            }
            catch (ActivationException ex)
            {
                AssertThat.StringContains("No application setting with key 'doesNotExist' could be found", ex.Message);
            }
        }

        [TestMethod]
        public void MultipleParameterConventions_RegisterATypeWithMultipleConventionBasedParameters_Succeeds()
        {
            // Arrange
            var container = CreateContainerWithConventions(
                new ConnectionStringsConvention(),
                new AppSettingsConvention());

            // Act
            container.Register<TypeWithBothConnectionStringAndAppSettingsConstructorArguments>();
        }

        [TestMethod]
        public void MultipleParameterConventions_ResolveATypeWithMultipleConventionBasedParameters_InjectsExpectedParameters()
        {
            // Arrange
            string expectedConnectionString = ConfigurationManager.ConnectionStrings["cs1"].ConnectionString;
            string expectedAppSetting = ConfigurationManager.AppSettings["as2"];

            var container = CreateContainerWithConventions(
                new ConnectionStringsConvention(),
                new AppSettingsConvention());

            // Act
            var instance =
                container.GetInstance<TypeWithBothConnectionStringAndAppSettingsConstructorArguments>();

            // Assert
            Assert.AreEqual(expectedConnectionString, instance.ConnectionString);
            Assert.AreEqual(expectedAppSetting, instance.AppSetting);
        }

        [TestMethod]
        public void OptionalParameterConvention_TypeWithOptionalDependencyAndDependencyNotRegistered_InjectsNullIntoTheInstance()
        {
            // Arrange
            var container = new Container();

            AddConventions(container,
                new OptionalParameterConvention(container.Options.ConstructorInjectionBehavior));

            // Act
            var instance = container.GetInstance<TypeWithOptionalDependency<IDisposable>>();

            // Assert
            Assert.IsNull(instance.Dependency);
        }

        [TestMethod]
        public void OptionalParameterConvention_TypeWithOptionalDependencyAndDependencyRegistered_InjectsDependency()
        {
            // Arrange
            var dependency = new SqlConnection();

            var container = new Container();

            AddConventions(container,
                new OptionalParameterConvention(container.Options.ConstructorInjectionBehavior));

            container.RegisterSingle<IDisposable>(dependency);

            // Act
            var instance = container.GetInstance<TypeWithOptionalDependency<IDisposable>>();

            // Assert
            Assert.IsTrue(object.ReferenceEquals(dependency, instance.Dependency));
        }

        [TestMethod]
        public void OptionalParameterConvention_TypeWithRequiredDependencyAndDependencyNotRegistered_ThrowsException()
        {
            // Arrange
            var container = new Container();

            AddConventions(container,
                new OptionalParameterConvention(container.Options.ConstructorInjectionBehavior));

            // Act
            Action action = () => container.GetInstance<TypeWithRequiredDependency<IDisposable>>();

            // Assert
            AssertThat.Throws<ActivationException>(action);
        }

        [TestMethod]
        public void OptionalParameterConvention_TypeWithOptionalIntDependencyWithDefaultValueOfFive_InjectsFive()
        {
            // Arrange
            var container = new Container();

            AddConventions(container,
                new OptionalParameterConvention(container.Options.ConstructorInjectionBehavior));

            // Act
            var instance = container.GetInstance<TypeWithOptionalIntDependencyWithDefaultValueOfFive>();

            // Assert
            Assert.AreEqual(5, instance.Value);
        }

        private static Container CreateContainerWithConventions(params IParameterConvention[] conventions)
        {
            var container = new Container();

            AddConventions(container, conventions);

            return container;
        }

        private static void AddConventions(Container container,
            params IParameterConvention[] conventions)
        {
            foreach (var convention in conventions)
            {
                container.Options.RegisterParameterConvention(convention);
            }
        }

        public class TypeWithConnectionStringConstructorArgument
        {
            // "cs1" is a connection string in the app.config of this test project.
            public TypeWithConnectionStringConstructorArgument(string cs1ConnectionString)
            {
                this.ConnectionString = cs1ConnectionString;
            }

            public string ConnectionString { get; private set; }
        }

        public class TypeWithConnectionStringIntConstructorArgument
        {
            public TypeWithConnectionStringIntConstructorArgument(int cs1ConnectionString)
            {
            }
        }

        public class TypeWithNonExistingConnectingStringConstructorArgument
        {
            public TypeWithNonExistingConnectingStringConstructorArgument(string doesNotExistConnectionString)
            {
            }
        }

        public class TypeWithAppSettingsStringConstructorArgument
        {
            // "as1" is a app setting  in the app.config of this test project.
            public TypeWithAppSettingsStringConstructorArgument(string as1AppSetting)
            {
                this.AppSetting = as1AppSetting;
            }

            public string AppSetting { get; private set; }
        }

        public class TypeWithAppSettingsGuidConstructorArgument
        {
            // "as2" is a app setting in the app.config of this test project.
            public TypeWithAppSettingsGuidConstructorArgument(Guid as2AppSetting)
            {
                this.AppSetting = as2AppSetting;
            }

            public Guid AppSetting { get; private set; }
        }

        public class TypeWithAppSettingConstructorArgumentOfReferenceType
        {
            public TypeWithAppSettingConstructorArgumentOfReferenceType(IDisposable as1AppSetting)
            {
            }
        }

        public class TypeWithNonExistingAppSettingConstructorArgument
        {
            public TypeWithNonExistingAppSettingConstructorArgument(string doesNotExistAppSetting)
            {
            }
        }

        public class TypeWithBothConnectionStringAndAppSettingsConstructorArguments
        {
            // "cs1" is a connection string in the app.config of this test project.
            // "as2" is a app setting in the app.config of this test project.
            public TypeWithBothConnectionStringAndAppSettingsConstructorArguments(string cs1ConnectionString,
                string as2AppSetting)
            {
                this.ConnectionString = cs1ConnectionString;
                this.AppSetting = as2AppSetting;
            }

            public string ConnectionString { get; private set; }

            public string AppSetting { get; private set; }
        }

        public class TypeWithRequiredDependency<T>
        {
            public TypeWithRequiredDependency(T dependency)
            {
                this.Dependency = dependency;
            }

            public T Dependency { get; private set; }
        }

        public class TypeWithOptionalDependency<T> where T : class
        {
            public TypeWithOptionalDependency(T dependency = null)
            {
                this.Dependency = dependency;
            }

            public T Dependency { get; private set; }
        }

        public class TypeWithOptionalIntDependencyWithDefaultValueOfFive
        {
            public TypeWithOptionalIntDependencyWithDefaultValueOfFive(int value = 5)
            {
                this.Value = value;
            }

            public int Value { get; private set; }
        }
    }
}