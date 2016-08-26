namespace SimpleInjector.Tests.Unit
{
    using System.Collections.Generic;
    using System.Linq;

    public interface IPlugin
    {
    }

    public class PluginImpl : IPlugin
    {
    }

    public class PluginImpl2 : IPlugin
    {
    }

    public class PluginWithDependency<TDependency> : IPlugin
    {
        public PluginWithDependency(TDependency dependency)
        {
        }
    }

    public class PluginDecorator : IPlugin
    {
        public PluginDecorator(IPlugin decoratee)
        {
            this.Decoratee = decoratee;
        }

        public IPlugin Decoratee { get; }
    }

    public class PluginDecorator<T> : IPlugin
    {
        public PluginDecorator(IPlugin decoratee)
        {
            this.Decoratee = decoratee;
        }

        public IPlugin Decoratee { get; }
    }

    public class PluginDecoratorWithDependencyOfType<TDependency> : IPlugin
    {
        public PluginDecoratorWithDependencyOfType(TDependency dependency, IPlugin decoratee)
        {
            this.Decoratee = decoratee;
        }

        public IPlugin Decoratee { get; }
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
    
    public class PluginWithDependencies<TDependency1, TDependency2> : IPlugin
    {
        public PluginWithDependencies(TDependency1 dep1, TDependency2 dep2)
        {
        }
    }

    public class PluginManager
    {
        public PluginManager(IEnumerable<IPlugin> plugins)
        {
            this.Plugins = plugins.ToArray();
        }

        public IPlugin[] Plugins { get; }
    }
}