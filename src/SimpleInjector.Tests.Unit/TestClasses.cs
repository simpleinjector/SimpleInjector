namespace SimpleInjector.Tests.Unit
{
    using System;
    using System.Collections.Generic;

    public sealed class ImportAttribute : Attribute;

    public interface IDuplicate;

    public interface IDuplicate<T>;

    public interface ILogger;

    public interface INonGenericService
    {
        void DoSomething();
    }

    public interface IStruct<T> where T : struct;

    public interface IFoo<T>;

    public interface IBar<T>;

    public interface IInterface<TOne, TTwo, TThree>;

    // This is the open generic interface that will be used as service type.
    public interface IService<TA, TB>;

    public interface IValidate<T>
    {
        void Validate(T instance);
    }

    public interface IDoStuff<T>
    {
        IService<T, int> Service { get; }
    }

    public interface IProducer<TValue>;

    public interface ICommand
    {
        void Execute();
    }

    public interface ICovariant<out T>;

    public interface ITimeProvider
    {
        DateTime Now { get; }
    }

    public interface IUserRepository;

    public sealed class FakeLogger : ILogger;

    public class Duplicate : IDuplicate;

    public class Duplicate<T> : IDuplicate<T>, IDuplicate;

    public class RealTimeProvider : ITimeProvider
    {
        public DateTime Now => DateTime.Now;
    }

    public class FakeTimeProvider : ITimeProvider
    {
        public DateTime Now { get; set; }
    }

    public class PluginTimeProvider : ITimeProvider, IPlugin
    {
        public DateTime Now { get; set; }
    }

    public class SqlUserRepository : IUserRepository;

    public class InMemoryUserRepository : IUserRepository;

    public class PluginDependantUserRepository(IPlugin plugin) : IUserRepository;

    public abstract class UserServiceBase(IUserRepository repository)
    {
        public IUserRepository Repository { get; } = repository;
    }

    public class RealUserService(IUserRepository repository) : UserServiceBase(repository);

    public class FakeUserService(IUserRepository repository) : UserServiceBase(repository);

    public class UserController(UserServiceBase userService)
    {
        public int UserKarmaOffset { get; set; }

        public UserServiceBase UserService { get; } = userService;
    }

    public class ConcreteTypeWithConcreteTypeConstructorArgument(RealUserService userService);

    public class ConcreteTypeWithMultiplePublicConstructors
    {
        public ConcreteTypeWithMultiplePublicConstructors()
        {
        }

        public ConcreteTypeWithMultiplePublicConstructors(IUserRepository userRepository)
        {
        }
    }

    public class ConcreteTypeWithValueTypeConstructorArgument(int intParam);

    public class ConcreteTypeWithStringConstructorArgument(string stringParam);

    public class ServiceWithUnregisteredDependencies(IDisposable a, IComparable b);

    public class CovariantImplementation<T> : ICovariant<T>;

    public class Consumer(Dep1 first, Dep2 second);

    public class Dep1(FirstSub c, SecondSub d, ThirdSub e);

    public class Dep2(FirstSub c, SecondSub d);

    public class FirstSub;

    public class SecondSub;

    public class ThirdSub;

    public class ConcreteCommand : ICommand
    {
        public void Execute()
        {
        }
    }

    public sealed class Logger<T> : ILogger;

    public sealed class NullLogger : ILogger;

    public sealed class ConsoleLogger : ILogger;

    public sealed class FailingConstructorLogger : ILogger
    {
        public FailingConstructorLogger()
        {
            throw new ArgumentNullException("some programming error.");
        }
    }

    public sealed class LoggerDecorator(ILogger logger) : ILogger
    {
        public readonly ILogger Logger = logger;
    }

    public sealed class ScopedLoggerDecoratorProxy(Func<Scope, ILogger> decorateeFactory) : ILogger
    {
        public readonly Func<Scope, ILogger> DecorateeFactory = decorateeFactory;
    }

    public sealed class NullValidator<T> : IValidate<T>
    {
        public void Validate(T instance)
        {
            // Do nothing.
        }
    }

    public sealed class ServiceImpl<TA, TB> : IService<TA, TB>
    {
    }

    public sealed class ServiceImplWithMultipleCtors<TA, TB> : IService<TA, TB>
    {
        public ServiceImplWithMultipleCtors()
        {
        }

        public ServiceImplWithMultipleCtors(int x)
        {
        }
    }

    public sealed class ServiceImplWithDependency<TA, TB>(IProducer<int> producer) : IService<TA, TB>;

    public class ServiceWithDependency<TDependency>(TDependency dependency)
    {
        public TDependency Dependency { get; } = dependency;
    }

    public class AnotherServiceWithDependency<TDependency>(TDependency dependency)
    {
        public TDependency Dependency { get; } = dependency;
    }

    public class ServiceDecorator(IService<int, object> decorated) : IService<int, object>;

    public class ServiceWithEnumerable<T>(IEnumerable<T> collection);

    public class ServiceWithProperty<TProperty> : INonGenericService
    {
        public TProperty Property { get; set; }

        public void DoSomething()
        {
        }
    }
}