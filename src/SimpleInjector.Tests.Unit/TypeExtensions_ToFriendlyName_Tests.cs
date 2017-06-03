namespace SimpleInjector.Core.Tests.Unit
{
    using System;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SimpleInjector.Core.Tests.Unit.Helpers;

    [TestClass]
    public class TypeExtensions_ToFriendlyName_Tests
    {
        [TestMethod]
        public void NonGenericTypeContainingBackTick_CreatesCorrectName()
        {
            // Arrange
            // NSubstitute creates non-generic proxy types by postfixing the type name with "Proxy". This 
            // could result in something as follows:
            string expectedName = "IQueryHandler`2Proxy";

            Type type = DynamicTypeBuilder.BuildType(expectedName);

            // Act
            string actualName = type.ToFriendlyName();

            // Assert
            Assert.AreEqual(expected: expectedName, actual: actualName);
        }

        [TestMethod]
        public void GenericTypeContainingBackTick_CreatesCorrectName()
        {
            // Arrange
            string expectedName = "Generic`Type<TKey, TResult>";

            Type type = DynamicTypeBuilder.BuildType("Generic`Type`2", "TKey", "TResult");

            // Act
            string actualName = type.ToFriendlyName();

            // Assert
            Assert.AreEqual(expected: expectedName, actual: actualName);
        }

        [TestMethod]
        public void GenericTypeWithIncorrectArgumentCountInName_CreatesCorrectName()
        {
            // Arrange
            string expectedName = "Generic<TKey, TResult>";

            Type type = DynamicTypeBuilder.BuildType("Generic`2", "TKey", "TResult");

            // Act
            string actualName = type.ToFriendlyName();

            // Assert
            Assert.AreEqual(expected: expectedName, actual: actualName);
        }

        [TestMethod]
        public void GenericTypeWithoutBacktickAndArgumentNumber_CreatesCorrectName()
        {
            // Arrange
            string expectedName = "Generic<TKey, TResult>";

            Type type = DynamicTypeBuilder.BuildType("Generic", "TKey", "TResult");

            // Act
            string actualName = type.ToFriendlyName();

            // Assert
            Assert.AreEqual(expected: expectedName, actual: actualName);
        }
        
        [TestMethod]
        public void NestedClassWithoutTypeArgumentsInsideGenericType_ReturnsExpectedName()
        {
            // Arrange
            Type type = typeof(GenericNastyness1<int>.Dictionary<object, string>.KeyCollection);

            string expectedName = "GenericNastyness1<TBla>.Dictionary<TKey, TValue>.KeyCollection";

            // Act
            string actualName = type.ToFriendlyName();

            // Assert
            Assert.AreEqual(expected: expectedName, actual: actualName);
        }

        [TestMethod]
        public void NestedClassWithTypeArgumentInsideGenericType_ReturnsExpectedName()
        {
            // Arrange
            Type type = typeof(GenericNastyness1<int>.Dictionary<object, string>.GenericKeyCollection<byte>);

            string expectedName =
                "GenericNastyness1<TBla>.Dictionary<TKey, TValue>.GenericKeyCollection<Byte>";

            // Act
            string actualName = type.ToFriendlyName();

            // Assert
            Assert.AreEqual(expected: expectedName, actual: actualName);
        }
    }

#pragma warning disable RCS1102 // Mark class as static.
    public class GenericNastyness1<TBla>
    {
        public class Dictionary<TKey, TValue>
        {
            public class KeyCollection
            {
            }

            public class GenericKeyCollection<TItem>
            {
            }
        }
    }
#pragma warning restore RCS1102 // Mark class as static.
}