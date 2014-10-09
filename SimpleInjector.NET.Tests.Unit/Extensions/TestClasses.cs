namespace SimpleInjector.Tests.Unit.Extensions
{
    using System;
    using System.Collections.Generic;
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
    
    public struct StructEvent : IAuditableEvent
    {
    }

    public class DefaultConstructorEvent
    {
        public DefaultConstructorEvent()
        {
        }
    }

    public class NoDefaultConstructorEvent
    {
        public NoDefaultConstructorEvent(IValidate<int> dependency)
        {
        }
    }

    public class ClassEvent
    {
    }

    public class AuditableEvent : IAuditableEvent
    {
    }

    public struct AuditableStructEvent : IAuditableEvent
    {
    }

    public class StructEventHandler : IEventHandler<StructEvent>
    {
    }

    public class AuditableEventEventHandler : IEventHandler<AuditableEvent>
    {
    }

    public class AuditableEventEventHandler<TAuditableEvent> : IEventHandler<TAuditableEvent>
        where TAuditableEvent : IAuditableEvent
    {
    }

    public class AuditableEventEventHandlerWithUnknown<TUnknown> : IEventHandler<AuditableEvent>
    {
    }

    public class NewConstraintEventHandler<TEvent> : IEventHandler<TEvent> where TEvent : new()
    {
    }

    public class StructConstraintEventHandler<TEvent> : IEventHandler<TEvent> where TEvent : struct
    {
    }

    public class ClassConstraintEventHandler<TClassEvent> : IEventHandler<TClassEvent>
        where TClassEvent : class
    {
    }

    public abstract class AbstractEventHandler<TEvent> : IEventHandler<TEvent>
    {
    }

    public class EventHandlerWithLoggerDependency<TEvent> : IEventHandler<TEvent>
    {
        public EventHandlerWithLoggerDependency(ILogger logger)
        {
        }
    }
    
    public class EventHandlerWithConstructorContainingPrimitive<T> : IEventHandler<T>
    {
        public EventHandlerWithConstructorContainingPrimitive(int somePrimitive)
        {
        }
    }

    public class EventHandlerWithDependency<T, TDependency> : IEventHandler<T>
    {
        public EventHandlerWithDependency(TDependency dependency)
        {
        }
    }

    public class MonoDictionary<T> : Dictionary<T, T>
    {
    }

    public class SneakyMonoDictionary<T, TUnused> : Dictionary<T, T>
    {
    }

    // Note: This class deliberately implements a second IProducer. This will verify whether the code can
    // handle types with multiple versions of the same interface.
    public class NullableProducer<T> : IProducer<T?>, IProducer<IValidate<T>>, IProducer<double>
        where T : struct
    {
    }

    public sealed class ServiceImpl<TA, TB> : IService<TA, TB>
    {
    }

    public sealed class ServiceImplWithMultipleCtors<TA, TB> : IService<TA, TB>
    {
        public ServiceImplWithMultipleCtors()
        {
        }

        public ServiceImplWithMultipleCtors(int x)
        {
        }
    }

    public sealed class ServiceImplWithDependency<TA, TB> : IService<TA, TB>
    {
        public ServiceImplWithDependency(IProducer<int> producer)
        {
        }
    }

    // The type constraint will prevent the type from being created when the arguments are ordered
    // incorrectly.
    public sealed class ServiceImplWithTypesArgsSwapped<B, A> : IService<A, B>
        where B : struct
        where A : class
    {
    }

    public class Bar
    {
    }

    public class Baz : IBar<Bar>
    {
    }

    public class Foo<T1, T2> : IFoo<T1> where T1 : IBar<T2>
    {
    }

    public class ServiceWhereTInIsTOut<TA, TB> : IService<TA, TB> where TA : TB
    {
    }

    public class NonGenericEventHandler : IEventHandler<ClassEvent>
    {
    }

    public class ServiceWithDependency<TDependency>
    {
        public ServiceWithDependency(TDependency dependency)
        {
            this.Dependency = dependency;
        }

        public TDependency Dependency { get; private set; }
    }

    public class Implementation<X, TUnused1, TUnused2, Y> : IInterface<X, X, Y>
    {
    }

    public interface IOpenGenericWithPredicate<T>
    {
    }

    public class OpenGenericWithPredicate1<T> : IOpenGenericWithPredicate<T>
    {
    }

    public class OpenGenericWithPredicate2<T> : IOpenGenericWithPredicate<T>
    {
    }

    internal class InternalEventHandler<TEvent> : IEventHandler<TEvent>
    {
    }
}