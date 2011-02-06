using System.Collections.Generic;
using System.Linq;

namespace CuttingEdge.ServiceLocation.Tests.Unit
{
    public interface IPlugin
    {
    }
    
    public interface IWeapon
    {
        void Hit(string target);
    }

    public class Katana : IWeapon
    {
        public void Hit(string target)
        {
        }
    }

    public class Tanto : IWeapon
    {
        public void Hit(string target)
        {
        }
    }

    public abstract class Warrior
    {
        protected Warrior(IWeapon weapon)
        {
            this.Weapon = weapon;
        }

        public IWeapon Weapon { get; private set; }

        public abstract void Attack(string target);
    }

    public class Samurai : Warrior
    {
        public Samurai(IWeapon weapon)
            : base(weapon)
        {
        }

        public override void Attack(string target)
        {
            this.Weapon.Hit(target);
        }
    }

    public class Ninja : Warrior
    {
        public Ninja(IWeapon weapon)
            : base(weapon)
        {
        }

        public override void Attack(string target)
        {
            // Ninja's are faster. They hit twice :-)
            this.Weapon.Hit(target);
            this.Weapon.Hit(target);
        }
    }

    public class Kingdom
    {
        public Kingdom(Warrior warrior)
        {
        }

        public int Karma { get; set; }
    }

    public class ConcreteTypeWithConcreteTypeConstructorArgument
    {
        public ConcreteTypeWithConcreteTypeConstructorArgument(Samurai samurai)
        {
        }
    }
    
    public class ConcreteTypeWithMultiplePublicConstructors
    {
        public ConcreteTypeWithMultiplePublicConstructors()
        {
        }

        public ConcreteTypeWithMultiplePublicConstructors(IWeapon weapon)
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