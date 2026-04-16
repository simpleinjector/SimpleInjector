#pragma warning disable CS9113 // Parameter is unread.
namespace SimpleInjector.Tests.Unit
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public interface ISpecialCommand;

    public interface ICommandHandler<TCommand>;

    public interface ICommandHandlerDecorator<TCommand> : ICommandHandler<TCommand>
    {
        ICommandHandler<TCommand> Decorated { get; }
    }

    public struct StructCommand;
    
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

    public class GenericHandler<TCommand, TDependency>(TDependency dependency) : ICommandHandler<TCommand>;

    public class CommandHandlerDecorator<TCommand>(ICommandHandler<TCommand> decoratee) : ICommandHandler<TCommand>
    {
        public ICommandHandler<TCommand> Decoratee { get; } = decoratee;
    }

    public class AsyncCommandHandlerProxy<T>(Container container, Func<ICommandHandler<T>> decorateeFactory)
        : ICommandHandler<T>
    {
        public Func<ICommandHandler<T>> DecorateeFactory { get; } = decorateeFactory;
    }

    public class LifetimeScopeCommandHandlerProxy<T>(
        Func<ICommandHandler<T>> decorateeFactory, Container container) : ICommandHandler<T>
    {
        public Func<ICommandHandler<T>> DecorateeFactory { get; } = decorateeFactory;
    }

    public class TransactionalCommandHandlerDecorator<T>(ICommandHandler<T> decorated)
        : ICommandHandler<T>, ICommandHandlerDecorator<T>
    {
        public ICommandHandler<T> Decorated { get; } = decorated;
    }

    public class ClassConstraintHandlerDecorator<T>(ICommandHandler<T> wrapped)
        : ICommandHandler<T>, ICommandHandlerDecorator<T>
        where T : class
    {
        public ICommandHandler<T> Decorated { get; } = wrapped;
    }

    // This is not a decorator, the class implements ICommandHandler<int> but wraps ICommandHandler<byte>
    public class BadCommandHandlerDecorator1(ICommandHandler<byte> handler) : ICommandHandler<int>
    {
        public void Handle(int command)
        {
        }
    }

    // This is not a decorator, the class takes 2 generic types but wraps ICommandHandler<T>
    public class CommandHandlerDecoratorWithUnresolvableArgument<T, TUnresolved>(ICommandHandler<T> handler)
        : ICommandHandler<T>
    {
    }

    public class StubCommandHandler : ICommandHandler<RealCommand>;

    public class StructCommandHandler : ICommandHandler<StructCommand>;

    public class RealCommandHandler : ICommandHandler<RealCommand>;

    public class RealCommandHandlerDecorator(ICommandHandler<RealCommand> decorated)
        : ICommandHandler<RealCommand>, ICommandHandlerDecorator<RealCommand>
    {
        public ICommandHandler<RealCommand> Decorated { get; } = decorated;
    }

    public class TransactionHandlerDecorator<T>(ICommandHandler<T> decorated)
        : ICommandHandler<T>, ICommandHandlerDecorator<T>
    {
        public ICommandHandler<T> Decorated { get; } = decorated;
    }

    public class ContextualHandlerDecorator<T>(ICommandHandler<T> decorated, DecoratorContext context)
        : ICommandHandler<T>, ICommandHandlerDecorator<T>
    {
        public ICommandHandler<T> Decorated { get; } = decorated;

        public DecoratorContext Context { get; } = context;
    }

    public class SpecialCommandHandlerDecorator<T>(ICommandHandler<T> decorated)
        : ICommandHandler<T> where T : ISpecialCommand
    {
        public void Handle(T command)
        {
        }
    }

    public class LogExceptionCommandHandlerDecorator<T>(ICommandHandler<T> decorated)
        : ICommandHandler<T>, ICommandHandlerDecorator<T>
    {
        public ICommandHandler<T> Decorated { get; } = decorated;
    }

    public class LoggingHandlerDecorator1<T>(ICommandHandler<T> wrapped, ILogger logger)
        : ICommandHandler<T>, ICommandHandlerDecorator<T>
    {
        public ICommandHandler<T> Decorated { get; } = wrapped;
    }
}
#pragma warning restore CS9113 // Parameter is unread.