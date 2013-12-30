namespace SimpleInjector.Tests.Unit
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class ScopedLifestyleTests
    {
        [TestMethod]
        public void DisposeInstances_WithNullArgument_ThrowsExpectedException()
        {
            // Act
            Action action = () => ScopedLifestyle.DisposeInstances(null);

            // Assert
            AssertThat.ThrowsWithParamName<ArgumentNullException>("disposables", action);
        }

        [TestMethod]
        public void DisposeInstances_ListWithOneItem_DisposesItem()
        {
            // Arrange
            var disposable = new DisposableObject();

            var disposables = new List<IDisposable> { disposable };
            
            // Act
            ScopedLifestyle.DisposeInstances(disposables);

            // Assert
            Assert.AreEqual(1, disposable.DisposeCount);
        }
        
        [TestMethod]
        public void DisposeInstances_ListWithMultipleItems_DisposesAllItems()
        {
            // Arrange
            var disposables = new List<DisposableObject> 
            { 
                new DisposableObject(),
                new DisposableObject(),
                new DisposableObject()
            };

            // Act
            ScopedLifestyle.DisposeInstances(disposables.Cast<IDisposable>().ToList());

            // Assert
            Assert.IsTrue(disposables.All(d => d.DisposeCount == 1));
        }

        [TestMethod]
        public void DisposeInstances_ListWithMultipleItems_DisposesAllItemsInReversedOrder()
        {
            // Arrange
            var disposedItems = new List<DisposableObject>();

            var disposables = new List<DisposableObject> 
            { 
                new DisposableObject(disposedItems.Add),
                new DisposableObject(disposedItems.Add),
                new DisposableObject(disposedItems.Add)
            };

            // Act
            ScopedLifestyle.DisposeInstances(disposables.Cast<IDisposable>().ToList());

            disposedItems.Reverse();

            // Assert
            Assert.IsTrue(disposedItems.SequenceEqual(disposables));
        }

        [TestMethod]
        public void DisposeInstances_WithMultipleItemsThatThrow_StillDisposesAllItems()
        {
            // Arrange
            var disposables = new List<DisposableObject> 
            { 
                new DisposableObject(new Exception()),
                new DisposableObject(new Exception()),
                new DisposableObject(new Exception())
            };

            try
            {
                // Act
                ScopedLifestyle.DisposeInstances(disposables.Cast<IDisposable>().ToList());

                // Assert
                Assert.Fail("Exception was expected to bubble up.");
            }
            catch
            {
                Assert.IsTrue(disposables.All(d => d.DisposeCount == 1));
            }
        }

        [TestMethod]
        public void DisposeInstances_WithMultipleItemsThatThrow_WillBubbleUpTheLastThrownException()
        {
            // Arrange
            var disposables = new List<DisposableObject> 
            { 
                new DisposableObject(new Exception()),
                new DisposableObject(new Exception()),
                new DisposableObject(new Exception()),
                new DisposableObject(),
                new DisposableObject(new Exception()),
                new DisposableObject(new Exception())
            };

            try
            {
                // Act
                ScopedLifestyle.DisposeInstances(disposables.Cast<IDisposable>().ToList());

                // Assert
                Assert.Fail("Exception was expected to bubble up.");
            }
            catch (Exception ex)
            {
                Assert.IsTrue(object.ReferenceEquals(disposables.First().ExceptionToThrow, ex));
            }
        }

        private sealed class DisposableObject : IDisposable
        {
            private Action<DisposableObject> disposing;

            public DisposableObject()
            {
            }

            public DisposableObject(Action<DisposableObject> disposing)
            {
                this.disposing = disposing;
            }

            public DisposableObject(Exception exceptionToThrow)
            {
                this.ExceptionToThrow = exceptionToThrow;
            }

            public Exception ExceptionToThrow { get; private set; }

            public int DisposeCount { get; private set; }

            public void Dispose()
            {
                this.DisposeCount++;

                if (this.disposing != null)
                {
                    this.disposing(this);
                }

                if (this.ExceptionToThrow != null)
                {
                    throw this.ExceptionToThrow;
                }
            }
        }
    }
}