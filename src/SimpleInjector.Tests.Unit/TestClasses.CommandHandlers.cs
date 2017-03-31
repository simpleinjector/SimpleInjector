namespace SimpleInjector.Tests.Unit
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public interface ISpecialCommand
    {
    }

    public interface ICommandHandler<TCommand>
    {
    }

    public interface ICommandHandlerDecorator<TCommand> : ICommandHandler<TCommand>
    {
        ICommandHandler<TCommand> Decorated { get; }
    }

    public struct StructCommand
    {
    }
    
    public static class CommandHandlerExtensions
    {
        public static IEnumerable<Type> GetDecoratorTypeChain<T>(this ICommandHandler<T> handler)
        {
            return GetDecoratorChain(handler).Select(instance => instance.GetType());
        }

        public static IEnumerable<ICommandHandler<T>> GetDecoratorChain<T>(this ICommandHandler<T> handler)
        {
            while (handler is ICommandHandlerDecorator<T>)
            {
                yield return handler;

                handler = ((ICommandHandlerDecorator<T>)handler).Decorated;
            }

            yield return handler;
        }
    }

    public class NullCommandHandler<T> : ICommandHandler<T>
    {
        public void Handle(T command)
        {
        }
    }

    public class DefaultCommandHandler<T> : ICommandHandler<T>
    {
        public void Handle(T command)
        {
        }
    }

    public class GenericHandler<TCommand, TDependency> : ICommandHandler<TCommand>
    {
        public GenericHandler(TDependency dependency)
        {
        }
    }

    public class CommandHandlerDecorator<TCommand> : ICommandHandler<TCommand>
    {
        public CommandHandlerDecorator(ICommandHandler<TCommand> decoratee)
        {
            this.Decoratee = decoratee;
        }

        public ICommandHandler<TCommand> Decoratee { get; }
    }

    public class AsyncCommandHandlerProxy<T> : ICommandHandler<T>
    {
        public AsyncCommandHandlerProxy(Container container, Func<ICommandHandler<T>> decorateeFactory)
        {
            this.DecorateeFactory = decorateeFactory;
        }

        public Func<ICommandHandler<T>> DecorateeFactory { get; }
    }

    public class LifetimeScopeCommandHandlerProxy<T> : ICommandHandler<T>
    {
        public LifetimeScopeCommandHandlerProxy(Func<ICommandHandler<T>> decorateeFactory, Container container)
        {
            this.DecorateeFactory = decorateeFactory;
        }

        public Func<ICommandHandler<T>> DecorateeFactory { get; }
    }

    public class TransactionalCommandHandlerDecorator<T> : ICommandHandler<T>, ICommandHandlerDecorator<T>
    {
        public TransactionalCommandHandlerDecorator(ICommandHandler<T> decorated)
        {
            this.Decorated = decorated;
        }

        public ICommandHandler<T> Decorated { get; }
    }

    public class ClassConstraintHandlerDecorator<T> : ICommandHandler<T>, ICommandHandlerDecorator<T>
        where T : class
    {
        public ClassConstraintHandlerDecorator(ICommandHandler<T> wrapped)
        {
            this.Decorated = wrapped;
        }

        public ICommandHandler<T> Decorated { get; }
    }

    // This is not a decorator, the class implements ICommandHandler<int> but wraps ICommandHandler<byte>
    public class BadCommandHandlerDecorator1 : ICommandHandler<int>
    {
        public BadCommandHandlerDecorator1(ICommandHandler<byte> handler)
        {
        }

        public void Handle(int command)
        {
        }
    }

    // This is not a decorator, the class takes 2 generic types but wraps ICommandHandler<T>
    public class CommandHandlerDecoratorWithUnresolvableArgument<T, TUnresolved> : ICommandHandler<T>
    {
        public CommandHandlerDecoratorWithUnresolvableArgument(ICommandHandler<T> handler)
        {
        }
    }

    public class StubCommandHandler : ICommandHandler<RealCommand>
    {
    }

    public class StructCommandHandler : ICommandHandler<StructCommand>
    {
    }

    public class RealCommandHandler : ICommandHandler<RealCommand>
    {
    }

    public class RealCommandHandlerDecorator : ICommandHandler<RealCommand>, ICommandHandlerDecorator<RealCommand>
    {
        public RealCommandHandlerDecorator(ICommandHandler<RealCommand> decorated)
        {
            this.Decorated = decorated;
        }

        public ICommandHandler<RealCommand> Decorated { get; }
    }

    public class TransactionHandlerDecorator<T> : ICommandHandler<T>, ICommandHandlerDecorator<T>
    {
        public TransactionHandlerDecorator(ICommandHandler<T> decorated)
        {
            this.Decorated = decorated;
        }

        public ICommandHandler<T> Decorated { get; }
    }

    public class ContextualHandlerDecorator<T> : ICommandHandler<T>, ICommandHandlerDecorator<T>
    {
        public ContextualHandlerDecorator(ICommandHandler<T> decorated, DecoratorContext context)
        {
            this.Decorated = decorated;
            this.Context = context;
        }

        public ICommandHandler<T> Decorated { get; }

        public DecoratorContext Context { get; }
    }

    public class SpecialCommandHandlerDecorator<T> : ICommandHandler<T> where T : ISpecialCommand
    {
        public SpecialCommandHandlerDecorator(ICommandHandler<T> decorated)
        {
        }

        public void Handle(T command)
        {
        }
    }

    public class LogExceptionCommandHandlerDecorator<T> : ICommandHandler<T>, ICommandHandlerDecorator<T>
    {
        public LogExceptionCommandHandlerDecorator(ICommandHandler<T> decorated)
        {
            this.Decorated = decorated;
        }

        public ICommandHandler<T> Decorated { get; }
    }

    public class LoggingHandlerDecorator1<T> : ICommandHandler<T>, ICommandHandlerDecorator<T>
    {
        private readonly ILogger logger;

        public LoggingHandlerDecorator1(ICommandHandler<T> wrapped, ILogger logger)
        {
            this.Decorated = wrapped;
            this.logger = logger;
        }

        public ICommandHandler<T> Decorated { get; }
    }
}