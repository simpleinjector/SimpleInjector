using System;
using System.Collections.Generic;
using System.Linq;

namespace SimpleInjector.Tests.Unit
{
    public interface ITimeProvider
    {
        DateTime Now { get; }
    }

    public interface IPlugin
    {
    }
    
    public interface IUserRepository
    {
        void Delete(int userId);
    }

    public class RealTimeProvider : ITimeProvider
    {
        public DateTime Now
        {
            get { return DateTime.Now; }
        }
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
        }

        public int UserKarmaOffset { get; set; }
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

    public class GenericType<T>
    {
        public GenericType()
        {
        }
    }

    public class PluginImpl : IPlugin
    {
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
}