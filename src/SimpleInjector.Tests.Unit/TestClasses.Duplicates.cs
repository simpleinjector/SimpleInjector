namespace SimpleInjector.Tests.Unit.Duplicates
{
    public interface IDuplicate
    {
    }

    public interface IDuplicate<T>
    {
    }

    public class UserController
    {
        public UserController(ILogger logger)
        {
            this.Logger = logger;
        }

        public ILogger Logger { get; }
    }
}