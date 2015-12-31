namespace SimpleInjector.Extensions.ExecutionContextScoping.Tests.Unit
{
    public interface ICommand
    {
        void Execute();
    }

    public interface IGeneric<T>
    {
    }
}