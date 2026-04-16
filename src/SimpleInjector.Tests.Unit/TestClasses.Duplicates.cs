namespace SimpleInjector.Tests.Unit.Duplicates
{
    public interface IDuplicate;

    public interface IDuplicate<T>;

    public class UserController(ILogger logger)
    {
        public ILogger Logger { get; } = logger;
    }
}