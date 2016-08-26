namespace SimpleInjector.Tests.Unit
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class GetTypesToRegisterTests
    {
        public interface IService<T>
        {
        }

        [TestMethod]
        public void TypesToRegisterOptions_WithDefaultValues_ContainsExpectedValues()
        {
            // Act
            var defaultOptions = new TypesToRegisterOptions();

            // Assert
            Assert.IsFalse(defaultOptions.IncludeDecorators);
            Assert.IsFalse(defaultOptions.IncludeGenericTypeDefinitions);
            Assert.IsTrue(defaultOptions.IncludeComposites);
        }

        [TestMethod]
        public void GetTypesToRegister_WithDefaultOptions_ContainsListOfExpectedTypes()
        {
            // Arrange
            var expectedTypes = new[]
            {
                typeof(NonGeneric1),
                typeof(NonGeneric2),
                typeof(NonGenericComposite),
                typeof(NonGenericWrappingCollection),
            };

            var defaultOptions = new TypesToRegisterOptions();

            // Act
            var actualTypes = GetTypesToRegister(typeof(IService<>), defaultOptions);

            // Assert
            Assert_CollectionsContainExactSameTypes(expectedTypes, actualTypes,
                "Excluded should have been: Everything that is generic or decorator.");
        }

        [TestMethod]
        public void GetTypesToRegister_IncludeDecorators_ContainsListOfExpectedTypes()
        {
            // Arrange
            var expectedTypes = new[]
            {
                typeof(NonGeneric1),
                typeof(NonGeneric2),
                typeof(NonGenericDecorator),
                typeof(NonGenericComposite),
                typeof(NonGenericWrappingCollection),
                typeof(NonGenericCompositeDecorator),
            };

            var options = new TypesToRegisterOptions { IncludeDecorators = true };

            // Act
            var actualTypes = GetTypesToRegister(typeof(IService<>), options);

            // Assert
            Assert_CollectionsContainExactSameTypes(expectedTypes, actualTypes,
                "Excluded should have been: Everything that is generic, so generic decorators as well.");
        }

        [TestMethod]
        public void GetTypesToRegister_IncludeGenericTypeDefinitions_ContainsListOfExpectedTypes()
        {
            // Arrange
            var expectedTypes = new[]
            {
                typeof(NonGeneric1),
                typeof(NonGeneric2),
                typeof(GenericTypeDef<>),
                typeof(NonGenericComposite),
                typeof(GenericComposite<>),
                typeof(NonGenericWrappingCollection),
                typeof(GenericWrappingCollection<>),
            };

            var options = new TypesToRegisterOptions { IncludeGenericTypeDefinitions = true };

            // Act
            var actualTypes = GetTypesToRegister(typeof(IService<>), options);

            // Assert
            Assert_CollectionsContainExactSameTypes(expectedTypes, actualTypes,
                "Excluded should have been: Everything that is a decorator, so generic decorators as well.");
        }
        
        [TestMethod]
        public void GetTypesToRegister_ExcludeComposites_ContainsListOfExpectedTypes()
        {
            // Arrange
            var expectedTypes = new[]
            {
                typeof(NonGeneric1),
                typeof(NonGeneric2),
                typeof(NonGenericWrappingCollection),
            };

            var options = new TypesToRegisterOptions { IncludeComposites = false };

            // Act
            var actualTypes = GetTypesToRegister(typeof(IService<>), options);

            // Assert
            Assert_CollectionsContainExactSameTypes(expectedTypes, actualTypes,
                "Excluded should have been: Everything that is generic, decorator or composite.");
        }

        [TestMethod]
        public void GetTypesToRegister_IncludeDecoratorsCompositesAndGenericTypes_ContainsListOfExpectedTypes()
        {
            // Arrange
            var expectedTypes = new[]
            {
                typeof(NonGeneric1),
                typeof(NonGeneric2),
                typeof(GenericTypeDef<>),
                typeof(NonGenericDecorator),
                typeof(GenericDecorator<>),
                typeof(NonGenericComposite),
                typeof(GenericComposite<>),
                typeof(NonGenericWrappingCollection),
                typeof(GenericWrappingCollection<>),
                typeof(NonGenericCompositeDecorator),
                typeof(GenericCompositeDecorator<>),
            };

            // IncludeComposites is true by default.
            var options = new TypesToRegisterOptions { IncludeDecorators = true, IncludeGenericTypeDefinitions = true };

            // Act
            var actualTypes = GetTypesToRegister(typeof(IService<>), options);

            // Assert
            Assert_CollectionsContainExactSameTypes(expectedTypes, actualTypes,
                "Nothing should be included.");
        }

        [TestMethod]
        public void GetTypesToRegister_IncludeDecoratorsAndGenericTypesButNoComposites_ContainsListOfExpectedTypes()
        {
            // Arrange
            var expectedTypes = new[]
            {
                typeof(NonGeneric1),
                typeof(NonGeneric2),
                typeof(GenericTypeDef<>),
                typeof(NonGenericDecorator),
                typeof(GenericDecorator<>),
                typeof(NonGenericWrappingCollection),
                typeof(GenericWrappingCollection<>),
            };

            // IncludeComposites is true by default.
            var options = new TypesToRegisterOptions
            {
                IncludeDecorators = true,
                IncludeGenericTypeDefinitions = true,
                IncludeComposites = false,
            };

            // Act
            var actualTypes = GetTypesToRegister(typeof(IService<>), options);

            // Assert
            Assert_CollectionsContainExactSameTypes(expectedTypes, actualTypes,
                "Excluded should have been: everything that's a decorator (also decorator-composites).");
        }

        private static Type[] GetTypesToRegister(Type serviceType, TypesToRegisterOptions options)
        {
            var container = new Container();
            var serviceTypeAssembly = new[] { serviceType.GetTypeInfo().Assembly };
            return container.GetTypesToRegister(serviceType, serviceTypeAssembly, options).ToArray();
        }

        private static void Assert_CollectionsContainExactSameTypes(Type[] expected, Type[] actual, string message)
        {
            var missingTypes = expected.Except(actual);

            var invalidlyIncludedTypes = actual.Except(expected);

            Assert.IsFalse(missingTypes.Any() || invalidlyIncludedTypes.Any(), string.Format(
                "{4}. The following {0} types are invalidly included: {1}\n\n" +
                "The following {2} types were missing: {3}",
                invalidlyIncludedTypes.Count(),
                invalidlyIncludedTypes.ToFriendlyNamesText(),
                missingTypes.Count(),
                missingTypes.ToFriendlyNamesText(),
                message));
        }
        
        public abstract class AbstractService : IService<int> { }

        public class NonGeneric1 : IService<int> { }

        public class NonGeneric2 : IService<int> { }

        public class GenericTypeDef<T> : IService<T> { }

        public class NonGenericDecorator : IService<int> { public NonGenericDecorator(IService<int> d) { } }

        public class GenericDecorator<T> : IService<T> { public GenericDecorator(IService<T> d) { } }

        public class NonGenericComposite : IService<int>
        {
            public NonGenericComposite(IEnumerable<IService<int>> services) { }
        }

        public class GenericComposite<T> : IService<T>
        {
            public GenericComposite(IEnumerable<IService<T>> services) { }
        }

        public class NonGenericWrappingCollection : IService<int>
        {
            // This is not a composite
            public NonGenericWrappingCollection(IEnumerable<IService<string>> services) { }
        }

        public class GenericWrappingCollection<T> : IService<T>
        {
            // This is not a composite
            public GenericWrappingCollection(IEnumerable<IService<string>> services) { }
        }

        public class NonGenericCompositeDecorator : IService<int>
        {
            public NonGenericCompositeDecorator(IService<int> d, IEnumerable<IService<int>> services) { }
        }

        public class GenericCompositeDecorator<T> : IService<T>
        {
            public GenericCompositeDecorator(IService<T> d, IEnumerable<IService<T>> services) { }
        }
    }
}