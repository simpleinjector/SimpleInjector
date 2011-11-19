namespace SimpleInjector.CodeSamples.Tests.Unit
{
    using System;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class ContextDependentExtensionsTests
    {
        public interface IContext
        {
        }

        public interface ICommandHandler<TCommand>
        {
            void Execute(TCommand command);
        }

        [TestMethod]
        public void GetInstance_ResolvingAConcreteTypeThatDependsOnAContextDependentType_InjectsExpectedType()
        {
            // Arrange
            var container = new Container();

            container.RegisterWithContext<IContext>(dc =>
            {
                var contextType = typeof(CommandHandlerContext<>).MakeGenericType(dc.ImplementationType);

                return (IContext)container.GetInstance(contextType);
            });

            // Act
            // Note: IntCommandHandler depends on IContext.
            var handler = container.GetInstance<IntCommandHandler>();

            // Assert
            Assert.IsInstanceOfType(handler.InjectedContext, typeof(CommandHandlerContext<IntCommandHandler>));
        }

        [TestMethod]
        public void GetInstance_ResolvingAConcreteTypeThatDependsOnAContextDependentType_InjectsExpectedType2()
        {
            // Arrange
            var container = new Container();

            container.RegisterWithContext<IContext>(dc =>
            {
                // Now we use the ServiceType instead of the ImplementationType.
                var contextType = typeof(CommandHandlerContext<>).MakeGenericType(dc.ServiceType);

                return (IContext)container.GetInstance(contextType);
            });

            // Act
            // IntCommandHandler depends on IContext.
            var handler = container.GetInstance<IntCommandHandler>();

            // Assert
            // Because we requested IntCommandHandler directly, the dc.ServiceType is of type IntCommandHandler.
            Assert.IsInstanceOfType(handler.InjectedContext, typeof(CommandHandlerContext<IntCommandHandler>));
        }

        [TestMethod]
        public void GetInstance_ResolvingAnInterfaceWhosImplementationDependsOnAContextDependentType_InjectsExpectedType()
        {
            // Arrange
            var container = new Container();

            container.RegisterWithContext<IContext>(dc =>
            {
                var contextType = typeof(CommandHandlerContext<>).MakeGenericType(dc.ImplementationType);

                return (IContext)container.GetInstance(contextType);
            });

            // Note: IntCommandHandler depends on IContext.
            container.Register<ICommandHandler<int>, IntCommandHandler>();

            // Act
            var handler = container.GetInstance<ICommandHandler<int>>() as IntCommandHandler;

            // Assert.IsInstanceOfType failed.  
            // Expected type:<CommandHandlerContext`1[IntCommandHandler]>. 
            // Actual type:<CommandHandlerContext`1[ICommandHandler`1[System.Int32]]>.

            // Assert
            Assert.IsInstanceOfType(handler.InjectedContext, typeof(CommandHandlerContext<IntCommandHandler>));
        }

        [TestMethod]
        public void GetInstance_ResolvingAnInterfaceWhosImplementationDependsOnAContextDependentType_InjectsExpectedType2()
        {
            // Arrange
            var container = new Container();

            container.RegisterWithContext<IContext>(dc =>
            {
                // Now we use the ServiceType instead of the ImplementationType.
                var contextType = typeof(CommandHandlerContext<>).MakeGenericType(dc.ServiceType);

                return (IContext)container.GetInstance(contextType);
            });

            // IntCommandHandler depends on IContext.
            container.Register<ICommandHandler<int>, IntCommandHandler>();

            // Act
            var handler = container.GetInstance<ICommandHandler<int>>() as IntCommandHandler;

            // Assert
            Assert.IsInstanceOfType(handler.InjectedContext, typeof(CommandHandlerContext<ICommandHandler<int>>));
        }

        [TestMethod]
        public void GetInstance_CalledDirectlyOnTheContextDependentType_InjectsADependencyContextWithoutServiceType()
        {
            // Arrange
            var container = new Container();

            container.RegisterWithContext<IContext>(dc =>
            {
                // Assert
                Assert.IsNull(dc.ServiceType);

                return new CommandHandlerContext<object>();
            });

            // Act
            container.GetInstance<IContext>();
        }

        [TestMethod]
        public void GetInstance_CalledDirectlyOnTheContextDependentType_InjectsADependencyContextWithoutImplementationType()
        {
            // Arrange
            var container = new Container();

            container.RegisterWithContext<IContext>(dc =>
            {
                // Assert
                Assert.IsNull(dc.ImplementationType);

                return new CommandHandlerContext<object>();
            });

            // Act
            container.GetInstance<IContext>();
        }

        [TestMethod]
        public void GetInstance_TypeWithContextRegisteredMultipleLevelsDeep_GetsInjectedWithExpectedContext()
        {
            // Arrange
            var container = new Container();

            container.RegisterWithContext<IContext>(dc =>
            {
                var contextType = typeof(CommandHandlerContext<>).MakeGenericType(dc.ImplementationType);

                return (IContext)container.GetInstance(contextType);
            });

            // IntCommandHandler depends on IContext.
            container.Register<ICommandHandler<int>, IntCommandHandler>();

            // Act
            // CommandHandlerWrapper<T> depends on ICommandHandler<T>
            var wrapper = container.GetInstance<CommandHandlerWrapper<int>>();

            // Assert
            var handler = (IntCommandHandler)wrapper.InjectedHandler;

            Assert.IsInstanceOfType(handler.InjectedContext, typeof(CommandHandlerContext<IntCommandHandler>));
        }

        [TestMethod]
        public void GetInstance_TypeWithContextRegisteredAtMultipleLevels_GetsInjectedWithExpectedContext()
        {
            // Arrange
            var container = new Container();

            container.RegisterWithContext<IContext>(dc =>
            {
                var contextType = typeof(CommandHandlerContext<>).MakeGenericType(dc.ImplementationType);

                return (IContext)container.GetInstance(contextType);
            });

            // IntCommandHandler depends on IContext.
            container.Register<ICommandHandler<int>, IntCommandHandler>();

            // Act
            // CommandHandlerWrapper<T> depends on ICommandHandler<T>
            var wrapper = container.GetInstance<CommandHandlerWrapper<int>>();

            // Assert
            Assert.IsInstanceOfType(wrapper.InjectedContext,
                typeof(CommandHandlerContext<CommandHandlerWrapper<int>>));
        }

        private sealed class CommandHandlerWrapper<T>
        {
            public CommandHandlerWrapper(ICommandHandler<T> handler, IContext context, 
                ConcreteCommand justAnExtraArgumentToMakeUsFindBugsFaster)
            {
                this.InjectedHandler = handler;
                this.InjectedContext = context;
            }

            public ICommandHandler<T> InjectedHandler { get; private set; }

            public IContext InjectedContext { get; private set; }
        }

        private sealed class IntCommandHandler : ICommandHandler<int>
        {
            public IntCommandHandler(IContext context)
            {
                this.InjectedContext = context;
            }

            public IContext InjectedContext { get; private set; }

            public void Execute(int command)
            {
                // Not important.
            }
        }

        private sealed class CommandHandlerContext<TCommandHandler> : IContext
        {
        }
    }
}