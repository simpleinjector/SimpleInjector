namespace SimpleInjector.CodeSamples.Tests.Unit
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SimpleInjector.Tests.Unit;

    [TestClass]
    public class ParameterConventionExtensionsTests
    {
        private static readonly Dictionary<string, string> ConnectionStrings =
           new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
           {
               ["cs1"] = "Some Connection String 1",
               ["cs2"] = "Some Connection String 2",
           };

        private static readonly Dictionary<string, string> AppSettings =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["as1"] = "Some App Settings value 1",
                ["as2"] = "8573842a-769c-4302-a85d-88d141fab3e5",
            };

        public static string GetAppSetting(string key) => AppSettings.ContainsKey(key) ? AppSettings[key] : null;
        public static string GetConnectionString(string name) =>
            ConnectionStrings.ContainsKey(name) ? ConnectionStrings[name] : null;

        [TestMethod]
        public void ConnectionStringsConvention_RegisteringTypeWithConnectionStringParameter_Succeeds()
        {
            // Arrange
            var container = CreateContainerWithConventions(new ConnectionStringsConvention(GetConnectionString));

            // Act
            container.Register<TypeWithConnectionStringConstructorArgument>();
        }

        [TestMethod]
        public void ConnectionStringsConvention_ResolvingTypeWithConnectionStringParameter_InjectsExpectedValue()
        {
            // Arrange
            string expectedConnectionString = GetConnectionString("cs1");

            var container = CreateContainerWithConventions(new ConnectionStringsConvention(GetConnectionString));

            // Act
            var instance = container.GetInstance<TypeWithConnectionStringConstructorArgument>();

            // Assert
            Assert.AreEqual(expectedConnectionString, instance.Cs1ConnectionString);
        }
        
        [TestMethod]
        public void ConnectionStringsConvention_ResolvingTypeWithConnectionStringProperty_InjectsExpectedValue()
        {
            // Arrange
            string expectedConnectionString = GetConnectionString("cs1");

            var container = CreateContainerWithConventions(new ConnectionStringsConvention(GetConnectionString));

            container.Options.EnablePropertyAutoWiring();

            container.Register<TypeWithConnectionStringProperty>();
            container.AutoWireProperty<TypeWithConnectionStringProperty>(t => t.Cs1ConnectionString);

            // Act
            var instance = container.GetInstance<TypeWithConnectionStringProperty>();

            // Assert
            Assert.AreEqual(expectedConnectionString, instance.Cs1ConnectionString);
        }
        
        [TestMethod]
        public void ConnectionStringsConvention_RegisteringTypeWithIntParameterWhichNameEndsWithConnectionString_FailsWithExpectedException()
        {
            // Arrange
            var container = CreateContainerWithConventions(new ConnectionStringsConvention(GetConnectionString));

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
            var container = CreateContainerWithConventions(new ConnectionStringsConvention(GetConnectionString));

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
            var container = CreateContainerWithConventions(new ConnectionStringsConvention(GetConnectionString));

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
            var container = CreateContainerWithConventions(new ConnectionStringsConvention(GetConnectionString));

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
            // example: new AppSettingsConvention(key => ConfigurationManager.AppSettings[key]);
            var container = CreateContainerWithConventions(new AppSettingsConvention(GetAppSetting));

            // Act
            container.Register<TypeWithAppSettingsStringConstructorArgument>();
        }

        [TestMethod]
        public void AppSettingsConvention_ResolvingTypeWithStringAppSettingParameter_InjectsExpectedValue()
        {
            // Arrange
            string expectedAppSetting = AppSettings["as1"];

            var container = CreateContainerWithConventions(new AppSettingsConvention(GetAppSetting));

            // Act
            var instance = container.GetInstance<TypeWithAppSettingsStringConstructorArgument>();

            // Assert
            Assert.AreEqual(expectedAppSetting, instance.AppSetting);
        }

        [TestMethod]
        public void AppSettingsConvention_RegisteringTypeWithGuidAppSettingParameter_Succeeds()
        {
            // Arrange
            var container = CreateContainerWithConventions(new AppSettingsConvention(GetAppSetting));

            // Act
            container.Register<TypeWithAppSettingsStringConstructorArgument>();
        }

        [TestMethod]
        public void AppSettingsConvention_ResolvingTypeWithGuidAppSettingParameter_InjectsExpectedValue()
        {
            // Arrange
            Guid expectedAppSetting = Guid.Parse(AppSettings["as2"]);

            var container = CreateContainerWithConventions(new AppSettingsConvention(GetAppSetting));

            // Act
            var instance = container.GetInstance<TypeWithAppSettingsGuidConstructorArgument>();

            // Assert
            Assert.AreEqual(expectedAppSetting, instance.AppSetting);
        }

        [TestMethod]
        public void AppSettingsConvention_RegisteringTypeWithReferenceTypeParameterWhichNameEndsWithAppSetting_Succeeds()
        {
            // Arrange
            var container = CreateContainerWithConventions(new AppSettingsConvention(GetAppSetting));

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
            var container = CreateContainerWithConventions(new AppSettingsConvention(GetAppSetting));

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
                    "contains the parameter with name 'as1AppSetting' and type IDisposable that is not " +
                    "registered.", ex.Message);
            }
        }

        [TestMethod]
        public void AppSettingsConvention_RegisteringTypeWithNonExistingAppSettingsParameter_FailsWithExpectedException()
        {
            // Arrange
            var container = CreateContainerWithConventions(new AppSettingsConvention(GetAppSetting));

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
            var container = CreateContainerWithConventions(new AppSettingsConvention(GetAppSetting));

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
                new ConnectionStringsConvention(GetConnectionString),
                new AppSettingsConvention(GetAppSetting));

            // Act
            container.Register<TypeWithBothConnectionStringAndAppSettingsConstructorArguments>();
        }

        [TestMethod]
        public void MultipleParameterConventions_ResolveATypeWithMultipleConventionBasedParameters_InjectsExpectedParameters()
        {
            // Arrange
            string expectedConnectionString = ConnectionStrings["cs1"];
            string expectedAppSetting = AppSettings["as2"];

            var container = CreateContainerWithConventions(
                new ConnectionStringsConvention(GetConnectionString),
                new AppSettingsConvention(GetAppSetting));

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
                new OptionalParameterConvention(container.Options.DependencyInjectionBehavior));

            // Act
            var instance = container.GetInstance<TypeWithOptionalDependency<IDisposable>>();

            // Assert
            Assert.IsNull(instance.Dependency);
        }

        [TestMethod]
        public void OptionalParameterConvention_TypeWithOptionalDependencyAndDependencyRegistered_InjectsDependency()
        {
            // Arrange
            var dependency = new NullLogger();

            var container = new Container();

            AddConventions(container,
                new OptionalParameterConvention(container.Options.DependencyInjectionBehavior));

            container.RegisterSingleton<ILogger>(dependency);

            // Act
            var instance = container.GetInstance<TypeWithOptionalDependency<ILogger>>();

            // Assert
            Assert.IsTrue(object.ReferenceEquals(dependency, instance.Dependency));
        }

        [TestMethod]
        public void OptionalParameterConvention_TypeWithRequiredDependencyAndDependencyNotRegistered_ThrowsException()
        {
            // Arrange
            var container = new Container();

            AddConventions(container,
                new OptionalParameterConvention(container.Options.DependencyInjectionBehavior));

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
                new OptionalParameterConvention(container.Options.DependencyInjectionBehavior));

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
                this.Cs1ConnectionString = cs1ConnectionString;
            }

            public string Cs1ConnectionString { get; }
        }

        public class TypeWithConnectionStringProperty
        {
            public string Cs1ConnectionString { get; set; }
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

            public string AppSetting { get; }
        }

        public class TypeWithAppSettingsGuidConstructorArgument
        {
            // "as2" is a app setting in the app.config of this test project.
            public TypeWithAppSettingsGuidConstructorArgument(Guid as2AppSetting)
            {
                this.AppSetting = as2AppSetting;
            }

            public Guid AppSetting { get; }
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

            public string ConnectionString { get; }

            public string AppSetting { get; }
        }

        public class TypeWithRequiredDependency<T>
        {
            public TypeWithRequiredDependency(T dependency)
            {
                this.Dependency = dependency;
            }

            public T Dependency { get; }
        }

        public class TypeWithOptionalDependency<T> where T : class
        {
            public TypeWithOptionalDependency(T dependency = null)
            {
                this.Dependency = dependency;
            }

            public T Dependency { get; }
        }

        public class TypeWithOptionalIntDependencyWithDefaultValueOfFive
        {
            public TypeWithOptionalIntDependencyWithDefaultValueOfFive(int value = 5)
            {
                this.Value = value;
            }

            public int Value { get; }
        }
    }
}