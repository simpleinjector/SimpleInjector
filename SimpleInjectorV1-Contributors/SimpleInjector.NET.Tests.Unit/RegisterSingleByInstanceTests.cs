namespace SimpleInjector.Tests.Unit
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    using NUnit.Framework;

    [TestFixture]
    public class RegisterSingleByInstanceTests
    {
        [Test]
        public void RegisterSingleByInstance_WithValidType_ContainerAlwaysReturnsSameInstance()
        {
            // Arrange
            var container = new Container();
            container.RegisterSingle<IUserRepository>(new SqlUserRepository());

            // Act
            var instance1 = container.GetInstance<IUserRepository>();
            var instance2 = container.GetInstance<IUserRepository>();

            // Assert
            Assert.IsNotNull(instance1, "GetInstance should never return null.");
            Assert.AreEqual(instance1, instance2, "Values should reference the same instance.");
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RegisterSingleByInstance_WithNullArgument_ThrowsException()
        {
            // Arrange
            var container = new Container();
            IUserRepository invalidInstance = null;

            // Act
            container.RegisterSingle<IUserRepository>(invalidInstance);
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException), UserMessage = "A certain type can only be registered once.")]
        public void RegisterSingleByInstance_CalledTwiceOnSameType_ThrowsException()
        {
            // Arrange
            var container = new Container();
            container.RegisterSingle<IUserRepository>(new SqlUserRepository());

            // Act
            container.RegisterSingle<IUserRepository>(new InMemoryUserRepository());
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException), UserMessage = "A certain type can only be registered once.")]
        public void RegisterSingleByInstance_CalledAfterRegisterOnSameType_ThrowsException()
        {
            // Arrange
            var container = new Container();
            container.Register<UserServiceBase>(() => new RealUserService(null));

            // Act
            container.RegisterSingle<UserServiceBase>(new FakeUserService(null));
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException), UserMessage = "The container should get locked after a call to GetInstance.")]
        public void RegisterSingleByInstance_AfterCallingGetInstance_ThrowsException()
        {
            // Arrange
            var container = new Container();
            container.RegisterSingle<IUserRepository>(new InMemoryUserRepository());
            container.GetInstance<IUserRepository>();

            // Act
            container.RegisterSingle<UserServiceBase>(new RealUserService(null));
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException), UserMessage = "The container should get locked after a call to GetAllInstances.")]
        public void RegisterSingleByInstance_AfterCallingGetAllInstances_ThrowsException()
        {
            // Arrange
            var container = new Container();
            var repositories = container.GetAllInstances<IUserRepository>();

            // Calling count will iterate the collections. 
            // The container will only get locked when the first item is retrieved.
            var count = repositories.Count();

            // Act
            container.RegisterSingle<UserServiceBase>(new RealUserService(null));
        }

        [Test]
        public void GetInstance_ForConcreteUnregisteredTypeWithDependencyRegisteredWithRegisterSingle_Succeeds()
        {
            // Arrange
            var container = new Container();

            // This registration will make the DelegateBuilder call the 
            // SingletonInstanceProducer.BuildExpression method.
            container.RegisterSingle<IUserRepository>(new SqlUserRepository());

            // Act
            container.GetInstance<RealUserService>();
        }
    }
}
