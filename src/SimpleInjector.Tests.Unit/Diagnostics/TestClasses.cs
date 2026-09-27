#pragma warning disable CS9113 // Parameter is unread.
namespace SimpleInjector.Diagnostics.Tests.Unit
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public interface IFoo;
    public interface IFooExt : IFoo;
    public interface IBar;
    public interface IBarExt : IBar;
    public interface IConcreteThing;

    public interface ITimeProvider
    {
        DateTime Now { get; }
    }

    public interface IPlugin;
    public interface IGeneric<T>;

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

    public class GenericType<T> : IGeneric<T>
    {
        public GenericType()
        {
        }
    }

    public class ComponentDependingOn<TDependency>(TDependency dependency);

    public class PluginImpl : IPlugin;

    public class PluginImpl2 : IPlugin;

    public class PluginDecorator(IPlugin decoratee) : IPlugin
    {
        public IPlugin Decoratee { get; } = decoratee;
    }

    public class PluginProxy(Func<IPlugin> decorateeFactory) : IPlugin
    {
        public Func<IPlugin> DecorateeFactory { get; } = decorateeFactory;
    }

    public class PluginWithDependencyOfType<TDependency> : IPlugin
    {
        public TDependency Dependency { get; set; }
    }

    public class PluginManager(IEnumerable<IPlugin> plugins)
    {
        public IPlugin[] Plugins { get; } = plugins.ToArray();
    }

    public class ConcreteTypeWithValueTypeConstructorArgument(int intParam);
    public class ConcreteTypeWithStringConstructorArgument(string stringParam);
    public class ServiceWithUnregisteredDependencies(IDisposable a, IComparable b);
    public class ConcreteShizzle;
    public class ConcreteThing : IConcreteThing;
    public class SomePluginImpl : IPlugin;

    public class DisposablePlugin : IPlugin, IDisposable
    {
        public void Dispose()
        {
        }
    }

    public class AsyncDisposablePlugin : IPlugin, IAsyncDisposable
    {
        public ValueTask DisposeAsync() => default;
    }

    public class DisposableAsyncDisposablePlugin : IPlugin, IAsyncDisposable, IDisposable
    {
        public void Dispose()
        {
        }

        public ValueTask DisposeAsync() => default;
    }

    public class PluginWith7Dependencies(
        IGeneric<int> dependency1,
        IGeneric<byte> dependency2,
        IGeneric<double> dependency3,
        IGeneric<float> dependency4,
        IGeneric<char> dependency5,
        IGeneric<decimal> dependency6,
        IGeneric<int?> dependency7) : IPlugin;

    public class PluginWith8Dependencies(
        IGeneric<int> dependency1,
        IGeneric<byte> dependency2,
        IGeneric<double> dependency3,
        IGeneric<float> dependency4,
        IGeneric<char> dependency5,
        IGeneric<decimal> dependency6,
        IGeneric<int?> dependency7,
        IGeneric<decimal?> dependency8) : IPlugin;

    public class AnotherPluginWith8Dependencies(
        IGeneric<int> dependency1,
        IGeneric<byte> dependency2,
        IGeneric<double> dependency3,
        IGeneric<float> dependency4,
        IGeneric<char> dependency5,
        IGeneric<decimal> dependency6,
        IGeneric<int?> dependency7,
        IGeneric<decimal?> dependency8) : IPlugin;

    public class PluginDecoratorWith5Dependencies(
        IPlugin decoratee,
        IGeneric<byte> dependency2,
        IGeneric<double> dependency3,
        IGeneric<float> dependency4,
        IGeneric<char> dependency5) : IPlugin;

    public class PluginDecoratorWith8Dependencies(
        IPlugin decoratee,
        IGeneric<byte> dependency2,
        IGeneric<double> dependency3,
        IGeneric<float> dependency4,
        IGeneric<char> dependency5,
        IGeneric<decimal> dependency6,
        IGeneric<int?> dependency7,
        IGeneric<decimal?> dependency8) : IPlugin;

    public class Consumer<TDependency>(TDependency dependency)
    {
        public readonly TDependency Dependency = dependency;
    }

    public class GenericPluginWith6Dependencies<T>(
        IGeneric<int> dependency1,
        IGeneric<byte> dependency2,
        IGeneric<double> dependency3,
        IGeneric<float> dependency4,
        IGeneric<char> dependency5,
        IGeneric<decimal> dependency6) : IGenericPlugin<T>;

    public class FooBar : IFoo, IBar, IFooExt, IBarExt;
    public class FooBarSub : FooBar;
    public class ChocolateBar : IFoo, IBar, IFooExt, IBarExt;
    public class FooDecorator(IFoo decoratee) : IFoo;
    public class BarDecorator(IBar decoratee) : IBar;
}
#pragma warning restore CS9113 // Parameter is unread.