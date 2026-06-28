namespace SimpleInjector.Tests.Unit
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using SimpleInjector.Lifestyles;

    [TestClass]
    public class ContainerScopeTests
    {
        [TestMethod]
        public void GetItem_NoValueSet_ReturnsNull()
        {
            // Arrange
            object key = new();

            var container = ContainerFactory.New();

            // Act
            object item = container.ContainerScope.GetItem(key);

            // Assert
            Assert.IsNull(item);
        }

        [TestMethod]
        public void GetItem_WithValueSet_ReturnsThatItem()
        {
            // Arrange
            object key = new();
            object expectedItem = new();

            var container = ContainerFactory.New();

            container.ContainerScope.SetItem(key, expectedItem);

            // Act
            object actualItem = container.ContainerScope.GetItem(key);

            // Assert
            Assert.AreSame(expectedItem, actualItem);
        }

        [TestMethod]
        public void GetItem_WithValueSetInOneContainer_DoesNotReturnThatItemInAnotherContainer()
        {
            // Arrange
            object key = new();
            object expectedItem = new();

            var container1 = ContainerFactory.New();
            var container2 = ContainerFactory.New();

            container1.ContainerScope.SetItem(key, expectedItem);

            // Act
            object actualItem = container2.ContainerScope.GetItem(key);

            // Assert
            Assert.IsNull(actualItem, "The items dictionary is expected to be container bound. Not static!");
        }

        [TestMethod]
        public void GetItem_WithValueSetTwice_ReturnsLastItem()
        {
            // Arrange
            object key = new();
            object firstItem = new();
            object expectedItem = new();

            var container = ContainerFactory.New();

            container.ContainerScope.SetItem(key, firstItem);
            container.ContainerScope.SetItem(key, expectedItem);

            // Act
            object actualItem = container.ContainerScope.GetItem(key);

            // Assert
            Assert.AreSame(expectedItem, actualItem);
        }

        [TestMethod]
        public void GetItem_WithValueReset_ReturnsNull()
        {
            // Arrange
            object key = new();

            var container = ContainerFactory.New();

            container.ContainerScope.SetItem(key, new object());
            container.ContainerScope.SetItem(key, null);

            // Act
            object item = container.ContainerScope.GetItem(key);

            // Assert
            // This test looks odd, but under the cover the item is removed from the collection when null
            // is supplied to prevent the dictionary from ever increasing, but we have to test this code path.
            Assert.IsNull(item, "When a value is overridden with null, it is expected to return null.");
        }

        [TestMethod]
        public void GetItem_WithNullKey_ThrowsException()
        {
            // Arrange
            var container = ContainerFactory.New();

            // Act
            Action action = () => container.ContainerScope.GetItem(null);

            // Assert
            AssertThat.Throws<ArgumentNullException>(action);
        }

        [TestMethod]
        public void SetItem_WithNullKey_ThrowsException()
        {
            // Arrange
            var container = ContainerFactory.New();

            // Act
            Action action = () => container.ContainerScope.SetItem(null, new object());

            // Assert
            AssertThat.Throws<ArgumentNullException>(action);
        }

        [TestMethod]
        public void GetOrSetItem_WithNullKey_ThrowsException()
        {
            // Arrange
            var container = ContainerFactory.New();

            // Act
            Action action = () => container.ContainerScope.GetOrSetItem<object>(null, (_, __) => null);

            // Assert
            AssertThat.Throws<ArgumentNullException>(action);
        }

        [TestMethod]
        public void GetOrSetItem_WithNullFactory_ThrowsException()
        {
            // Arrange
            var container = ContainerFactory.New();

            // Act
            Action action = () => container.ContainerScope.GetOrSetItem<object>(new object(), null);

            // Assert
            AssertThat.Throws<ArgumentNullException>(action);
        }

        [TestMethod]
        public void GetOrSetItem_WithValueArguments_DoesNotReturnNull()
        {
            // Arrange
            var container = ContainerFactory.New();

            // Act
            var instance = container.ContainerScope.GetOrSetItem(new object(), (_, __) => new object());

            // Assert
            Assert.IsNotNull(instance);
        }

        [TestMethod]
        public void GetOrSetItem_CalledTwiceForSameKey_ReturnsTheSameValue()
        {
            // Arrange
            var key = new object();

            var container = ContainerFactory.New();

            // Act
            var instance1 = container.ContainerScope.GetOrSetItem(key, (_, __) => new object());
            var instance2 = container.ContainerScope.GetOrSetItem(key, (_, __) => new object());

            // Assert
            Assert.AreSame(instance1, instance2);
        }

        [TestMethod]
        public void GetAllInstances_ResolvingACollectionWithMixedLifestylesForFlowingScope_ResolvesInstancesWithExpectedLifestylesForNestedScopes()
        {
            this.GetAllInstances_ResolvingACollectionWithMixedLifestyles_ResolvesInstancesWithExpectedLifestylesForNestedScopes(true);
        }

        [TestMethod]
        public void GetAllInstances_ResolvingACollectionWithMixedLifestylesForNormalScope_ResolvesInstancesWithExpectedLifestylesForNestedScopes()
        {
            this.GetAllInstances_ResolvingACollectionWithMixedLifestyles_ResolvesInstancesWithExpectedLifestylesForNestedScopes(false);
        }

        private void GetAllInstances_ResolvingACollectionWithMixedLifestyles_ResolvesInstancesWithExpectedLifestylesForNestedScopes(
            bool useFlowingScope)
        {
            var container = ContainerFactory.New();

            container.Options.DefaultScopedLifestyle = useFlowingScope
                ? ScopedLifestyle.Flowing
                : new AsyncScopedLifestyle();

            container.Collection.Append<ILogger, ConsoleLogger>(Lifestyle.Transient);
            container.Collection.Append<ILogger, NullLogger>(Lifestyle.Scoped);
            container.Collection.Append<ILogger, Logger<int>>(Lifestyle.Singleton);

            using (var parentScope = CreateScope(useFlowingScope, container))
            {
                IEnumerable<ILogger> parentLoggers = parentScope.GetAllInstances<ILogger>();

                var transientLogger = parentLoggers.First();
                var scopedLogger = parentLoggers.Second();
                var singletonLogger = parentLoggers.Last();

                Assert.AreNotSame(transientLogger, parentLoggers.First());
                Assert.AreSame(scopedLogger, parentLoggers.Second());
                Assert.AreSame(singletonLogger, parentLoggers.Last());

                using (var nestedScope = CreateScope(useFlowingScope, container))
                {
                    IEnumerable<ILogger> nestedLoggers = nestedScope.GetAllInstances<ILogger>();
                    
                    // IMPORTANT: Under flowing scopes, resolving Scoped instances from a stream behaves
                    // different compared to 'normal' scopes.
                    if (useFlowingScope)
                    {
                        // Each flowing scope will get its own IEnumerable<T> instance, and resolving from the
                        // parentLoggers will, therefore, resolve instances from within that scope.
                        Assert.AreNotSame(parentLoggers, nestedLoggers);
                        Assert.AreNotSame(parentLoggers.Second(), nestedLoggers.Second());
                    }
                    else
                    {
                        // With 'normal' scoping, IEnumerable<T> will be singletons and since scopes are
                        // ambient and we're now running inside the nested scope, also the parentLoggers
                        // return the same scoped instance.
                        Assert.AreSame(parentLoggers, nestedLoggers);
                        Assert.AreSame(parentLoggers.Second(), nestedLoggers.Second());
                    }

                    Assert.AreSame(parentLoggers.Last(), nestedLoggers.Last());
                }
            }
        }

        [TestMethod]
        public void GetAllInstances_ResolvingACollectionWithMixedLifestylesForFlowingScope_ResolvesInstancesWithExpectedLifestylesFromForUnrelatedScopes()
        {
            this.GetAllInstances_ResolvingACollectionWithMixedLifestyles_ResolvesInstancesWithExpectedLifestylesFromForUnrelatedScopes(true);
        }

        [TestMethod]
        public void GetAllInstances_ResolvingACollectionWithMixedLifestylesForNormalScope_ResolvesInstancesWithExpectedLifestylesFromForUnrelatedScopes()
        {
            this.GetAllInstances_ResolvingACollectionWithMixedLifestyles_ResolvesInstancesWithExpectedLifestylesFromForUnrelatedScopes(false);
        }

        private void GetAllInstances_ResolvingACollectionWithMixedLifestyles_ResolvesInstancesWithExpectedLifestylesFromForUnrelatedScopes(
            bool useFlowingScope)
        {
            var container = ContainerFactory.New();

            container.Options.DefaultScopedLifestyle = useFlowingScope
                ? ScopedLifestyle.Flowing
                : new AsyncScopedLifestyle();

            container.Collection.Append<ILogger, ConsoleLogger>(Lifestyle.Singleton);
            container.Collection.Append<ILogger, NullLogger>(Lifestyle.Scoped);

            ILogger singletonLogger;
            ILogger scopedLogger;

            using (var parentScope = CreateScope(useFlowingScope, container))
            {
                IEnumerable<ILogger> parentLoggers = parentScope.GetAllInstances<ILogger>();

                singletonLogger = parentLoggers.First();
                scopedLogger = parentLoggers.Second();
            }

            using (var scope = CreateScope(useFlowingScope, container))
            {
                IEnumerable<ILogger> loggers = scope.GetAllInstances<ILogger>();

                Assert.AreSame(singletonLogger, loggers.First());
                Assert.AreNotSame(scopedLogger, loggers.Second());
            }
        }

        private static Scope CreateScope(bool useFlowingScope, Container container)
        {
            return useFlowingScope ? new Scope(container) : AsyncScopedLifestyle.BeginScope(container);
        }
    }
}