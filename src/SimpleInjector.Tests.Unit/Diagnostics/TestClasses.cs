namespace SimpleInjector.Diagnostics.Tests.Unit
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public interface IFoo
    {
    }

    public interface IFooExt : IFoo
    {
    }

    public interface IBar
    {
    }

    public interface IBarExt : IBar
    {
    }
        
    public interface IConcreteThing
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
        void Delete(int userId);
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
        public void Delete(int userId)
        {
        }
    }

    public class InMemoryUserRepository : IUserRepository
    {
        public void Delete(int userId)
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

    public class GenericType<T> : IGeneric<T>
    {
        public GenericType()
        {
        }
    }
    
    public class ComponentDependingOn<TDependency>
    {
        public ComponentDependingOn(TDependency dependency)
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

        public IPlugin Decoratee { get; }
    }

    public class PluginProxy : IPlugin
    {
        public PluginProxy(Func<IPlugin> decorateeFactory)
        {
            this.DecorateeFactory = decorateeFactory;
        }

        public Func<IPlugin> DecorateeFactory { get; }
    }

    public class PluginWithDependencyOfType<TDependency> : IPlugin
    {
        public TDependency Dependency { get; set; }
    }
    
    public class PluginManager
    {
        public PluginManager(IEnumerable<IPlugin> plugins)
        {
            this.Plugins = plugins.ToArray();
        }

        public IPlugin[] Plugins { get; }
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

    public class ConcreteShizzle
    {
    }

    public class ConcreteThing : IConcreteThing
    {
    }
    
    public class SomePluginImpl : IPlugin
    {
    }

    public class DisposablePlugin : IPlugin, IDisposable
    {
        public void Dispose()
        {
        }
    }

    public class PluginWith7Dependencies : IPlugin
    {
        public PluginWith7Dependencies(
            IGeneric<int> dependency1,
            IGeneric<byte> dependency2,
            IGeneric<double> dependency3,
            IGeneric<float> dependency4,
            IGeneric<char> dependency5,
            IGeneric<decimal> dependency6,
            IGeneric<int?> dependency7)
        {
        }
    }

    public class PluginWith8Dependencies : IPlugin
    {
        public PluginWith8Dependencies(
            IGeneric<int> dependency1,
            IGeneric<byte> dependency2,
            IGeneric<double> dependency3,
            IGeneric<float> dependency4,
            IGeneric<char> dependency5,
            IGeneric<decimal> dependency6,
            IGeneric<int?> dependency7,
            IGeneric<decimal?> dependency8)
        {
        }
    }

    public class AnotherPluginWith8Dependencies : IPlugin
    {
        public AnotherPluginWith8Dependencies(
            IGeneric<int> dependency1,
            IGeneric<byte> dependency2,
            IGeneric<double> dependency3,
            IGeneric<float> dependency4,
            IGeneric<char> dependency5,
            IGeneric<decimal> dependency6,
            IGeneric<int?> dependency7,
            IGeneric<decimal?> dependency8)
        {
        }
    }

    public class PluginDecoratorWith5Dependencies : IPlugin
    {
        public PluginDecoratorWith5Dependencies(
            IPlugin decoratee,
            IGeneric<byte> dependency2,
            IGeneric<double> dependency3,
            IGeneric<float> dependency4,
            IGeneric<char> dependency5)
        {
        }
    }

    public class PluginDecoratorWith8Dependencies : IPlugin
    {
        public PluginDecoratorWith8Dependencies(
            IPlugin decoratee,
            IGeneric<byte> dependency2,
            IGeneric<double> dependency3,
            IGeneric<float> dependency4,
            IGeneric<char> dependency5,
            IGeneric<decimal> dependency6,
            IGeneric<int?> dependency7,
            IGeneric<decimal?> dependency8)
        {
        }
    }

    public class Consumer<TDependency>
    {
        public readonly TDependency Dependency;

        public Consumer(TDependency dependency)
        {
            this.Dependency = dependency;
        }
    }

    public class GenericPluginWith6Dependencies<T> : IGenericPlugin<T>
    {
        public GenericPluginWith6Dependencies(
            IGeneric<int> dependency1,
            IGeneric<byte> dependency2,
            IGeneric<double> dependency3,
            IGeneric<float> dependency4,
            IGeneric<char> dependency5,
            IGeneric<decimal> dependency6)
        {
        }
    }

    public class FooBar : IFoo, IBar, IFooExt, IBarExt
    {
    }

    public class FooBarSub : FooBar
    {
    }

    public class ChocolateBar : IFoo, IBar, IFooExt, IBarExt
    {
    }

    public class FooDecorator : IFoo
    {
        public FooDecorator(IFoo decoratee)
        {
        }
    }

    public class BarDecorator : IBar
    {
        public BarDecorator(IBar decoratee)
        {
        }
    }
}