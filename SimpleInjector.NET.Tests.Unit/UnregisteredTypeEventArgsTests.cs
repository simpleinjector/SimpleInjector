namespace SimpleInjector.Tests.Unit
{
    using System;
    using System.Linq.Expressions;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class UnregisteredTypeEventArgsTests
    {
        [TestMethod]
        public void RegisterFunc_WithNullArgument_ThrowsException()
        {
            // Arrange
            var e = new UnregisteredTypeEventArgs(typeof(IUserRepository));

            Func<object> invalidFunc = null;
            
            // Act
            Action action = () => e.Register(invalidFunc);

            // Assert
            AssertThat.Throws<ArgumentNullException>(action);
        }

        [TestMethod]
        public void RegisterExpression_WithNullArgument_ThrowsException()
        {
            // Arrange
            var e = new UnregisteredTypeEventArgs(typeof(IUserRepository));

            Expression invalidExpression = null;

            // Act
            Action action = () => e.Register(invalidExpression);

            // Assert
            AssertThat.Throws<ArgumentNullException>(action);
        }

        [TestMethod]
        public void RegisterExpression_CalledTwice_ThrowsException()
        {
            // Arrange
            var e = new UnregisteredTypeEventArgs(typeof(IUserRepository));

            e.Register(Expression.Constant(null));

            // Act
            Action action = () => e.Register(Expression.Constant(null));

            // Assert
            AssertThat.Throws<ActivationException>(action);
        }

        [TestMethod]
        public void RegisterFunc_CalledTwice_ThrowsException()
        {
            // Arrange
            var e = new UnregisteredTypeEventArgs(typeof(IUserRepository));

            e.Register(() => null);

            // Act
            Action action = () => e.Register(() => null);

            // Assert
            AssertThat.Throws<ActivationException>(action);
        }

        [TestMethod]
        public void RegisterExpression_CalledAfterCallingRegisterFun_ThrowsException()
        {
            // Arrange
            var e = new UnregisteredTypeEventArgs(typeof(IUserRepository));

            e.Register(() => null);

            // Act
            Action action = () => e.Register(Expression.Constant(null));

            // Assert
            AssertThat.Throws<ActivationException>(action);
        }

        [TestMethod]
        public void RegisterFunc_CalledAfterCallingRegisterExpression_ThrowsException()
        {
            // Arrange
            var e = new UnregisteredTypeEventArgs(typeof(IUserRepository));

            e.Register(Expression.Constant(null));

            // Act
            Action action = () => e.Register(() => null);

            // Assert
            AssertThat.Throws<ActivationException>(action);
        }
    }
}