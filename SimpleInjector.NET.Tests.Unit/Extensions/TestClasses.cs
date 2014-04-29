namespace SimpleInjector.Tests.Unit.Extensions
{
    using System;
    using System.Collections.ObjectModel;

    public interface ILogger
    {
        void Log(string message);
    }

    public interface ISpecialCommand
    {
    }

    public interface ICommandHandler<TCommand>
    {
        void Handle(TCommand command);
    }

    public interface INonGenericService
    {
        void DoSomething();
    }

    public interface IQuery<TResult>
    {
    }

    public interface ICacheableQuery<TResult> : IQuery<ReadOnlyCollection<TResult>>
    {
    }

    public interface IQueryHandler<TQuery, TResult> where TQuery : IQuery<TResult>
    {
    }

    public interface IStruct<T> where T : struct
    {
    }

    public interface IFoo<T>
    {
    }

    public interface IBar<T>
    {
    }

    public interface IInterface<TOne, TTwo, TThree>
    {
    }

    // This is the open generic interface that will be used as service type.
    public interface IService<TA, TB>
    {
    }

    public interface IValidate<T>
    {
        void Validate(T instance);
    }

    public interface IDoStuff<T>
    {
        IService<T, int> Service { get; }
    }

    public interface IEventHandler<TEvent>
    {
    }

    public interface IAuditableEvent
    {
    }

    public interface IProducer<TValue>
    {
    }

    public interface ICommand
    {
        void Execute();
    }

    public class ConcreteCommand : ICommand
    {
        public void Execute()
        {
        }
    }

    public class ConcreteTypeWithMultiplePublicConstructors
    {
        public ConcreteTypeWithMultiplePublicConstructors(ICommand command)
        {
        }

        public ConcreteTypeWithMultiplePublicConstructors(ICommand command1, ICommand command2)
        {
        }
    }

    public sealed class NullValidator<T> : IValidate<T>
    {
        public void Validate(T instance)
        {
            // Do nothing.
        }
    }

    public sealed class ValidatorWithUnusedTypeArgument<T, TUnused> : IValidate<T>
    {
        public void Validate(T instance)
        {
            // Do nothing.
        }
    }

    public class GenericQuery1<TModel> : IQuery<TModel>
    {
    }

    public class GenericQuery2<TModel> : IQuery<TModel>
    {
    }
    
    public class QueryHandlerWithNestedType1<TModel> : IQueryHandler<GenericQuery1<TModel>, TModel>
    {
    }

    public class QueryHandlerWithNestedType2<TModel> : IQueryHandler<GenericQuery2<TModel>, TModel>
    {
    }

    public class CacheableQuery : ICacheableQuery<DayOfWeek>
    {
    }

    public class NonCacheableQuery : IQuery<DayOfWeek[]>
    {
    }

    public class CacheableQueryHandlerDecorator<TQuery, TResult> : IQueryHandler<TQuery, ReadOnlyCollection<TResult>>
        where TQuery : ICacheableQuery<TResult>
    {
        public CacheableQueryHandlerDecorator(IQueryHandler<TQuery, ReadOnlyCollection<TResult>> handler)
        {
        }
    }

    public class CacheableQueryHandler : IQueryHandler<CacheableQuery, ReadOnlyCollection<DayOfWeek>>
    {
    }

    public class NonCacheableQueryHandler : IQueryHandler<NonCacheableQuery, DayOfWeek[]>
    {
    }

    public struct StructCommand
    {
    }
}