namespace SimpleInjector.Tests.Unit.Diagnostics
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using SimpleInjector.Diagnostics;
        
#if DEBUG
    [TestClass]
    public class TypeGeneralizerTests
    {
        [TestMethod]
        public void MakeTypePartiallyGenericUpToLevel_NonGenericType_ReturnsTheSuppliedType()
        {
            // Arrange
            Type suppliedType = typeof(IDisposable);
            Type expectedType = typeof(IDisposable);

            // Act
            Type actualType = TypeGeneralizer.MakeTypePartiallyGenericUpToLevel(suppliedType, 0);

            // Assert
            Assert.AreEqual(expectedType, actualType);
        }

        [TestMethod]
        public void MakeTypePartiallyGenericUpToLevel_GenericTypeWithLevelZero_ReturnsAGenericTypeDefinition()
        {
            // Arrange
            Type suppliedType = typeof(IEnumerable<int>);
            Type expectedType = typeof(IEnumerable<>);

            // Act
            Type actualType = TypeGeneralizer.MakeTypePartiallyGenericUpToLevel(suppliedType, 0);

            // Assert
            Assert.AreEqual(expectedType.ToFriendlyName(), actualType.ToFriendlyName());
        }

        [TestMethod]
        public void MakeTypePartiallyGenericUpToLevel_OneLevelDeepGenericTypeRequestingLevel1_ReturnsThatType()
        {
            // Arrange
            Type suppliedType = typeof(IEnumerable<int>);
            Type expectedType = typeof(IEnumerable<int>);

            // Act
            Type actualType = TypeGeneralizer.MakeTypePartiallyGenericUpToLevel(suppliedType, 1);

            // Assert
            Assert.AreEqual(expectedType.ToFriendlyName(), actualType.ToFriendlyName());
        }

        [TestMethod]
        public void MakeTypePartiallyGenericUpToLevel_FourLevelsDeepGenericTypeRequestingLevel0_ReturnsGenericTypeDefinition()
        {
            // Arrange
            Type suppliedType = typeof(IEnumerable<HashSet<List<Collection<double>>>>);
            Type expectedType = typeof(IEnumerable<>);

            // Act
            Type actualType = TypeGeneralizer.MakeTypePartiallyGenericUpToLevel(suppliedType, 0);

            // Assert
            Assert.AreEqual(expectedType.ToFriendlyName(), actualType.ToFriendlyName());
        }

        [TestMethod]
        public void MakeTypePartiallyGenericUpToLevel_FourLevelsDeepGenericTypeRequestingLevel1_ReturnsPartlyGenericTypeDefinition()
        {
            // Arrange
            Type suppliedType = typeof(IEnumerable<HashSet<List<Collection<double>>>>);
            string expectedType = "IEnumerable<HashSet<T>>";

            // Act
            Type actualType = TypeGeneralizer.MakeTypePartiallyGenericUpToLevel(suppliedType, 1);

            // Assert
            Assert.AreEqual(expectedType, actualType.ToFriendlyName());
        }

        [TestMethod]
        public void MakeTypePartiallyGenericUpToLevel_FourLevelsDeepGenericTypeRequestingLevel2_ReturnsPartlyGenericTypeDefinition()
        {
            // Arrange
            Type suppliedType = typeof(IEnumerable<HashSet<List<Collection<double>>>>);
            string expectedType = "IEnumerable<HashSet<List<T>>>";

            // Act
            Type actualType = TypeGeneralizer.MakeTypePartiallyGenericUpToLevel(suppliedType, 2);

            // Assert
            Assert.AreEqual(expectedType, actualType.ToFriendlyName());
        }

        [TestMethod]
        public void MakeTypePartiallyGenericUpToLevel_FourLevelsDeepGenericTypeRequestingLevel3_ReturnsPartlyGenericTypeDefinition()
        {
            // Arrange
            Type suppliedType = typeof(IEnumerable<HashSet<List<Collection<double>>>>);
            string expectedType = "IEnumerable<HashSet<List<Collection<T>>>>";

            // Act
            Type actualType = TypeGeneralizer.MakeTypePartiallyGenericUpToLevel(suppliedType, 3);

            // Assert
            Assert.AreEqual(expectedType, actualType.ToFriendlyName());
        }

        [TestMethod]
        public void MakeTypePartiallyGenericUpToLevel_FourLevelsDeepGenericTypeWithMultipleBranchesRequestingLevel3_ReturnsPartlyGenericTypeDefinition()
        {
            // Arrange
            Type suppliedType = typeof(IEnumerable<HashSet<List<Tuple<double, int>>>>);
            string expectedType = "IEnumerable<HashSet<List<Tuple<T1, T2>>>>";

            // Act
            Type actualType = TypeGeneralizer.MakeTypePartiallyGenericUpToLevel(suppliedType, 3);

            // Assert
            Assert.AreEqual(expectedType, actualType.ToFriendlyName());
        }

        [TestMethod]
        public void MakeTypePartiallyGenericUpToLevel_FourLevelsDeepGenericTypeRequestingLevel4_ReturnsThatType()
        {
            // Arrange
            Type suppliedType = typeof(IEnumerable<HashSet<List<Collection<double>>>>);
            Type expectedType = typeof(IEnumerable<HashSet<List<Collection<double>>>>);

            // Act
            Type actualType = TypeGeneralizer.MakeTypePartiallyGenericUpToLevel(suppliedType, 4);

            // Assert
            Assert.AreEqual(expectedType, actualType);
        }
    }
#endif
}