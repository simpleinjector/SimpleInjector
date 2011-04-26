namespace SimpleInjector.CodeSamples.Tests.Unit
{
    public interface ILogger
    {
        void Log(string message);
    }

    public interface ICommand
    {
        void Execute();
    }

    public interface IValidator<T>
    {
        void Validate(T instance);
    }

    public class ConcreteCommand : ICommand
    {
        public void Execute()
        {
        }
    }

    public class NullValidator<T> : IValidator<T>
    {
        public void Validate(T instance)
        {
        }
    }
}