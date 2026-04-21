namespace SimpleInjector.Tests.Unit
{
    public interface IAuditableEvent;

    // This interface is contravariant, since TEvent is defined with the 'in' keyword.
    public interface IEventHandler<in TEvent>;

    public struct StructEvent : IAuditableEvent;

    public class DefaultConstructorEvent
    {
        public DefaultConstructorEvent()
        {
        }
    }

    public class NoDefaultConstructorEvent(IValidate<int> dependency);

    public class ClassEvent;

    public class AuditableEvent : IAuditableEvent;

    public struct AuditableStructEvent : IAuditableEvent;

    public class StructEventHandler : IEventHandler<StructEvent>;

    public class AuditableEventEventHandler : IEventHandler<AuditableEvent>;

    public class EventHandlerImplementationTwoInterface : IEventHandler<AuditableEvent>, IEventHandler<ClassEvent>;

    public class AuditableEventEventHandler<TAuditableEvent> : IEventHandler<TAuditableEvent>
        where TAuditableEvent : IAuditableEvent;

    public class AuditableEventEventHandlerWithUnknown<TUnknown> : IEventHandler<AuditableEvent>;

    public class GenericEventHandler<TEvent> : IEventHandler<TEvent>;

    public class NewConstraintEventHandler<TEvent> : IEventHandler<TEvent> where TEvent : new();

    public class StructConstraintEventHandler<TEvent> : IEventHandler<TEvent> where TEvent : struct;

    public class ClassConstraintEventHandler<TClassEvent> : IEventHandler<TClassEvent>
        where TClassEvent : class;

    public abstract class AbstractEventHandler<TEvent> : IEventHandler<TEvent>;

    public class EventHandlerWithLoggerDependency<TEvent>(ILogger logger) : IEventHandler<TEvent>;

    public class EventHandlerWithConstructorContainingPrimitive<T>(int somePrimitive) : IEventHandler<T>;

    public class EventHandlerWithDependency<T, TDependency>(TDependency dependency) : IEventHandler<T>;

    public class NonGenericEventHandler : IEventHandler<ClassEvent>;

    internal class InternalEventHandler<TEvent> : IEventHandler<TEvent>;
}