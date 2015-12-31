namespace SimpleInjector.Diagnostics.Tests.Unit
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SimpleInjector.Diagnostics;
    using SimpleInjector.Tests.Unit;
        
    [TestClass]
    public class TypeGeneralizerTests
    {
        public interface IQuery<TResult>
        {
        }

        public interface IQueryHandler<TQuery, TResult> where TQuery : IQuery<TResult>
        {
            TResult Handle(TQuery query);
        }

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

        [TestMethod]
        public void MakeTypePartiallyGenericUpToLevel_RequestingLevel0OnTypeWithGenericConstraint_ReturnsGenericTypeDefinition()
        {
            // Arrange
            Type inputType = typeof(IQueryHandler<ConstraintQuery, IEnumerable<int>>);
            int nestingLevel = 0;
            Type expectedType = typeof(IQueryHandler<,>);

            // Act
            Type actualType = TypeGeneralizer.MakeTypePartiallyGenericUpToLevel(inputType, nestingLevel);

            // Assert
            Assert.AreEqual(expectedType, actualType);
        }
        
        [TestMethod]
        public void MakeTypePartiallyGenericUpToLevel_RequestingLevelOneOnTypeWithGenericConstraint_SkipsLevelOne()
        {
            // Arrange
            Type inputType = typeof(IQueryHandler<ConstraintQuery, IEnumerable<int>>);
            int nestingLevel = 1;
            Type expectedType = typeof(IQueryHandler<ConstraintQuery, IEnumerable<int>>);

            // Act
            Type actualType = TypeGeneralizer.MakeTypePartiallyGenericUpToLevel(inputType, nestingLevel);

            // Assert
            Assert.AreEqual(expectedType, actualType, @"
                Since ConstraintQuery implements IQuery<IEnumerable<int>>, it is impossible to
                build a IQueryHandler<ConstraintQuery, IEnumerable<T>> since IEnumerable<T> is
                not an IEnumerable<int>. We should therefore skip that level and return a level
                two type.");
        }

        public class ConstraintQuery : IQuery<IEnumerable<int>>
        {
        }
    }
}