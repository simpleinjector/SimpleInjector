namespace SimpleInjector.Tests.Unit.Extensions
{
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
}