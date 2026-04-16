#pragma warning disable CS9113 // Parameter is unread.
namespace SimpleInjector.Tests.Unit
{
    using System.Collections.Generic;
    using System.Linq;

    public interface IPlugin;

    public abstract class PluginBase : IPlugin;

    public class PluginImpl : IPlugin;

    public class PluginImpl2 : IPlugin;

    public class PluginWithDependency<TDependency>(TDependency dependency) : IPlugin;

    public class PluginDecorator(IPlugin decoratee) : IPlugin
    {
        public IPlugin Decoratee { get; } = decoratee;
    }

    public class PluginDecorator<T>(IPlugin decoratee) : IPlugin
    {
        public IPlugin Decoratee { get; } = decoratee;
    }

    public class PluginDecoratorWithDependencyOfType<TDependency>(TDependency dependency, IPlugin decoratee) : IPlugin
    {
        public IPlugin Decoratee { get; } = decoratee;
    }

    public class PluginWithPropertyDependencyOfType<TDependency> : IPlugin
    {
        public TDependency Dependency { get; set; }
    }

    public class PluginWithDependencyOfType<TDependency>(TDependency dependency) : IPlugin;

    public class PluginWithDependencies<TDependency1, TDependency2>(TDependency1 dep1, TDependency2 dep2) : IPlugin;

    public class PluginManager(IEnumerable<IPlugin> plugins)
    {
        public IPlugin[] Plugins { get; } = plugins.ToArray();
    }
}
#pragma warning restore CS9113 // Parameter is unread.
