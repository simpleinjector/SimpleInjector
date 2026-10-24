namespace SimpleInjector.Tests.Unit.Advanced
{
    using System;
    using System.Linq.Expressions;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using SimpleInjector.Advanced;
    using SimpleInjector.Diagnostics.Tests.Unit;

    [TestClass]
    public class ExpressionCompilationBehaviorTests
    {
        [TestMethod]
        public void GetInstance_ExpressionCompilationBehaviorReturnsNull_FallsBackOnBuiltInCompilation()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Options.ExpressionCompilationBehavior = new NullReturningCompilationBehavior();

            container.Register<IFoo, FooBar>();

            container.GetInstance<IFoo>();
        }

        public sealed class NullReturningCompilationBehavior : IExpressionCompilationBehavior
        {
            public Delegate Compile(Expression expression) => null;
        }
    }
}