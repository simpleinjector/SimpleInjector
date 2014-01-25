namespace SimpleInjector.Extensions.LifetimeScoping.Tests.Unit
{
    public interface ICommand
    {
        void Execute();
    }

    public interface IGeneric<T>
    {
    }
}