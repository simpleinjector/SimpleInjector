namespace SimpleInjector.Tests.Unit
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public interface ICovariant<out T>
    {
    }

    public interface ITimeProvider
    {
        DateTime Now { get; }
    }

    public interface IPlugin
    {
    }

    public interface IGeneric<T>
    {
    }

    public interface IUserRepository
    {
    }

    public class RealTimeProvider : ITimeProvider
    {
        public DateTime Now
        {
            get { return DateTime.Now; }
        }
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

        public IUserRepository Repository { get; private set; }
    }

    public class RealUserService : UserServiceBase
    {
        public RealUserService(IUserRepository repository) : base(repository)
        {
        }
    }

    public class FakeUserService : UserServiceBase
    {
        public FakeUserService(IUserRepository repository) : base(repository)
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

        public UserServiceBase UserService { get; private set; }
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

    public class GenericType<T> : IGeneric<T>
    {
        public GenericType()
        {
        }
    }

    public class PluginImpl : IPlugin
    {
    }

    public class PluginImpl2 : IPlugin
    {
    }
    
    public class PluginDecorator : IPlugin
    {
        public PluginDecorator(IPlugin decoratee)
        {
            this.Decoratee = decoratee;
        }

        public IPlugin Decoratee { get; private set; }
    }

    public class PluginDecorator<T> : IPlugin
    {
        public PluginDecorator(IPlugin decoratee)
        {
            this.Decoratee = decoratee;
        }

        public IPlugin Decoratee { get; private set; }
    }

    public class PluginDecoratorWithDependencyOfType<TDependency> : IPlugin
    {
        public PluginDecoratorWithDependencyOfType(TDependency dependency, IPlugin decoratee)
        {
            this.Decoratee = decoratee;
        }

        public IPlugin Decoratee { get; private set; }
    }

    public class PluginWithPropertyDependencyOfType<TDependency> : IPlugin
    {
        public TDependency Dependency { get; set; }
    }

    public class PluginWithDependencyOfType<TDependency> : IPlugin
    {
        public PluginWithDependencyOfType(TDependency dependency)
        {
        }
    }
    
    public class PluginManager
    {
        public PluginManager(IEnumerable<IPlugin> plugins)
        {
            this.Plugins = plugins.ToArray();
        }

        public IPlugin[] Plugins { get; private set; }
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
}