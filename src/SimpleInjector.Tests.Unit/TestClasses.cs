namespace SimpleInjector.Tests.Unit
{
    using System;
    using System.Collections.Generic;

    public sealed class ImportAttribute : Attribute
    {
    }

    public interface IDuplicate
    {
    }

    public interface IDuplicate<T>
    {
    }

    public interface ILogger
    {
    }

    public interface INonGenericService
    {
        void DoSomething();
    }

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

    public interface IProducer<TValue>
    {
    }

    public interface ICommand
    {
        void Execute();
    }

    public interface ICovariant<out T>
    {
    }

    public interface ITimeProvider
    {
        DateTime Now { get; }
    }

    public interface IUserRepository
    {
    }

    public sealed class FakeLogger : ILogger
    {
    }

    public class Duplicate : IDuplicate
    {
    }

    public class Duplicate<T> : IDuplicate<T>
    {
    }

    public class RealTimeProvider : ITimeProvider
    {
        public DateTime Now => DateTime.Now;
    }

    public class FakeTimeProvider : ITimeProvider
    {
        public DateTime Now { get; set; }
    }

    public class SqlUserRepository : IUserRepository
    {
    }

    public class InMemoryUserRepository : IUserRepository
    {
    }

    public class PluginDependantUserRepository : IUserRepository
    {
        public PluginDependantUserRepository(IPlugin plugin)
        {
        }
    }

    public abstract class UserServiceBase
    {
        protected UserServiceBase(IUserRepository repository)
        {
            this.Repository = repository;
        }

        public IUserRepository Repository { get; }
    }

    public class RealUserService : UserServiceBase
    {
        public RealUserService(IUserRepository repository)
            : base(repository)
        {
        }
    }

    public class FakeUserService : UserServiceBase
    {
        public FakeUserService(IUserRepository repository)
            : base(repository)
        {
        }
    }

    public class UserController
    {
        public UserController(UserServiceBase userService)
        {
            this.UserService = userService;
        }

        public int UserKarmaOffset { get; set; }

        public UserServiceBase UserService { get; }
    }

    public class ConcreteTypeWithConcreteTypeConstructorArgument
    {
        public ConcreteTypeWithConcreteTypeConstructorArgument(RealUserService userService)
        {
        }
    }

    public class ConcreteTypeWithMultiplePublicConstructors
    {
        public ConcreteTypeWithMultiplePublicConstructors()
        {
        }

        public ConcreteTypeWithMultiplePublicConstructors(IUserRepository userRepository)
        {
        }
    }

    public class ConcreteTypeWithValueTypeConstructorArgument
    {
        public ConcreteTypeWithValueTypeConstructorArgument(int intParam)
        {
        }
    }

    public class ConcreteTypeWithStringConstructorArgument
    {
        public ConcreteTypeWithStringConstructorArgument(string stringParam)
        {
        }
    }

    public class ServiceWithUnregisteredDependencies
    {
        public ServiceWithUnregisteredDependencies(IDisposable a, IComparable b)
        {
        }
    }

    public class CovariantImplementation<T> : ICovariant<T>
    {
    }

    public class Consumer
    {
        public Consumer(Dep1 first, Dep2 second)
        {
        }
    }

    public class Dep1
    {
        public Dep1(FirstSub c, SecondSub d, ThirdSub e)
        {
        }
    }

    public class Dep2
    {
        public Dep2(FirstSub c, SecondSub d)
        {
        }
    }

    public class FirstSub
    {
    }

    public class SecondSub
    {
    }

    public class ThirdSub
    {
    }

    public class ConcreteCommand : ICommand
    {
        public void Execute()
        {
        }
    }

    public sealed class Logger<T> : ILogger
    {
    }

    public sealed class NullLogger : ILogger
    {
    }
    
    public sealed class ConsoleLogger : ILogger
    {
    }

    public sealed class LoggerDecorator : ILogger
    {
        public readonly ILogger Logger;

        public LoggerDecorator(ILogger logger)
        {
            this.Logger = logger;
        }
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

    public sealed class ServiceImplWithDependency<TA, TB> : IService<TA, TB>
    {
        public ServiceImplWithDependency(IProducer<int> producer)
        {
        }
    }

    public class ServiceWithDependency<TDependency>
    {
        public ServiceWithDependency(TDependency dependency)
        {
            this.Dependency = dependency;
        }

        public TDependency Dependency { get; }
    }

    public class AnotherServiceWithDependency<TDependency>
    {
        public AnotherServiceWithDependency(TDependency dependency)
        {
            this.Dependency = dependency;
        }

        public TDependency Dependency { get; }
    }

    public class ServiceDecorator : IService<int, object>
    {
        public ServiceDecorator(IService<int, object> decorated)
        {
        }
    }

    public class ServiceWithEnumerable<T>
    {
        public ServiceWithEnumerable(IEnumerable<T> collection)
        {
        }
    }

    public class ServiceWithProperty<TProperty> : INonGenericService
    {
        public TProperty Property { get; set; }

        public void DoSomething()
        {
        }
    }
}