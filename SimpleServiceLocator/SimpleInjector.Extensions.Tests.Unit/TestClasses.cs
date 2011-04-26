namespace SimpleInjector.Extensions.Tests.Unit
{
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
}