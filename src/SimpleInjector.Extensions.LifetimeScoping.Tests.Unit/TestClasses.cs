namespace SimpleInjector.Extensions.LifetimeScoping.Tests.Unit
{
    public interface ILogger
    {
        void Log(string message);
    }

    public interface ICommand
    {
        void Execute();
    }

    public interface IGeneric<T>
    {
    }
}