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

    public class ConcreteTypeWithMultiplePublicConstructors
    {
        public ConcreteTypeWithMultiplePublicConstructors(ICommand command)
        {
        }

        public ConcreteTypeWithMultiplePublicConstructors(ICommand command1, ICommand command2)
        {
        }
    }
}