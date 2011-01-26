using System.Collections.Generic;

using CuttingEdge.ServiceLocation;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CuttingEdge.ServiceLocator.Extensions.Tests.Unit
{
    [TestClass]
    public class SslCollectionRegistrationExtensionsTests
    {
        [TestMethod]
        public void GetInstance_OnTypeWithArrayArgumentAfterCallingRegisterArrays_CreatesTypeAsEspected()
        {
            // Arrange
            var container = new SimpleServiceLocator();

            container.AllowToResolveArrays();

            // Act
            var deposite = container.GetInstance<WeaponDeposite>();
        }

        [TestMethod]
        public void GetInstance_OnTypeWithArrayArgumentAfterCallingRegisterArrays_InjectsExpectedNumberOfArguments()
        {
            // Arrange
            var container = new SimpleServiceLocator();

            container.RegisterAll<IWeapon>(new Katana());

            container.AllowToResolveArrays();

            // Act
            var deposite = container.GetInstance<WeaponDeposite>();

            // Assert
            Assert.IsNotNull(deposite.Weapons);
            Assert.AreEqual(1, deposite.Weapons.Length);
        }

        [TestMethod]
        public void GetInstance_OnTypeWithArrayArgumentAfterCallingRegisterArrays_InjectsExpectedElement()
        {
            // Arrange
            var expectedKatana = new Katana();

            var container = new SimpleServiceLocator();

            container.RegisterAll<IWeapon>(expectedKatana);

            container.AllowToResolveArrays();

            // Act
            var deposite = container.GetInstance<WeaponDeposite>();

            // Assert
            Assert.AreEqual(expectedKatana, deposite.Weapons[0]);
        }

        [TestMethod]
        public void GetInstance_CalledMultipleTimesOnSameType_InjectsANewArrayOnEachRequest()
        {
            // Arrange
            var container = new SimpleServiceLocator();

            container.RegisterAll<IWeapon>(new Katana());

            container.AllowToResolveArrays();

            // Act
            var deposite = container.GetInstance<WeaponDeposite>();
            
            deposite.Weapons[0] = null;

            deposite = container.GetInstance<WeaponDeposite>();

            // Assert
            Assert.IsNotNull(deposite.Weapons[0], 
                "The element in the array is expected NOT to be null. When it is null, it means that the " +
                "array has been cached.");
        }

        [TestMethod]
        public void GetInstance_Always_InjectsAFreshArrayInstance()
        {
            // Arrange
            var container = new SimpleServiceLocator();

            List<IWeapon> weapons = new List<IWeapon>();

            // Add a first weapon
            weapons.Add(new Katana());

            container.RegisterAll<IWeapon>(weapons);

            container.AllowToResolveArrays();

            container.GetInstance<WeaponDeposite>();

            // Add yet another weapon
            weapons.Add(new Katana());
            
            // Act
            var deposite = container.GetInstance<WeaponDeposite>();

            // Assert
            Assert.AreEqual(2, deposite.Weapons.Length, "The IEnumerable<IWeapon> collection should be " +
                "cached by its reference, and not by its current content, because that content is allowed " +
                "to change.");
        }

        [TestMethod]
        public void GetInstance_OnConcreteTypeWithIListArgumentAfterAllowToResolveArrays_Succeeds()
        {
            // Arrange
            var container = new SimpleServiceLocator();

            container.AllowToResolveArrays();

            // Act
            var collection = container.GetInstance<WeaponCollection>();
            
            // Assert
            Assert.IsNotNull(collection.Weapons);
            Assert.AreEqual(0, collection.Weapons.Count);
        }

        private sealed class WeaponDeposite
        {
            public WeaponDeposite(IWeapon[] weapons)
            {
                this.Weapons = weapons;
            }

            public IWeapon[] Weapons { get; private set; }
        }

        private sealed class WeaponCollection
        {
            public WeaponCollection(IList<IWeapon> weapons)
            {
                this.Weapons = weapons;
            }

            public IList<IWeapon> Weapons { get; private set; }
        }
    }
}