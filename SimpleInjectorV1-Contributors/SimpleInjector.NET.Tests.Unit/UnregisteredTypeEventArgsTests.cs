namespace SimpleInjector.Tests.Unit
{
    using System;
    using System.Linq.Expressions;

    using NUnit.Framework;

    [TestFixture]
    public class UnregisteredTypeEventArgsTests
    {
        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RegisterFunc_WithNullArgument_ThrowsException()
        {
            // Arrange
            var e = new UnregisteredTypeEventArgs(typeof(IUserRepository));

            Func<object> invalidFunc = null;
            
            // Act
            e.Register(invalidFunc);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RegisterExpression_WithNullArgument_ThrowsException()
        {
            // Arrange
            var e = new UnregisteredTypeEventArgs(typeof(IUserRepository));

            Expression invalidExpression = null;

            // Act
            e.Register(invalidExpression);
        }

        [Test]
        [ExpectedException(typeof(ActivationException))]
        public void RegisterExpression_CalledTwice_ThrowsException()
        {
            // Arrange
            var e = new UnregisteredTypeEventArgs(typeof(IUserRepository));

            e.Register(Expression.Constant(null));

            // Act
            e.Register(Expression.Constant(null));
        }

        [Test]
        [ExpectedException(typeof(ActivationException))]
        public void RegisterFunc_CalledTwice_ThrowsException()
        {
            // Arrange
            var e = new UnregisteredTypeEventArgs(typeof(IUserRepository));

            e.Register(() => null);

            // Act
            e.Register(() => null);
        }

        [Test]
        [ExpectedException(typeof(ActivationException))]
        public void RegisterExpression_CalledAfterCallingRegisterFun_ThrowsException()
        {
            // Arrange
            var e = new UnregisteredTypeEventArgs(typeof(IUserRepository));

            e.Register(() => null);

            // Act
            e.Register(Expression.Constant(null));
        }

        [Test]
        [ExpectedException(typeof(ActivationException))]
        public void RegisterFunc_CalledAfterCallingRegisterExpression_ThrowsException()
        {
            // Arrange
            var e = new UnregisteredTypeEventArgs(typeof(IUserRepository));

            e.Register(Expression.Constant(null));

            // Act
            e.Register(() => null);
        }
    }
}