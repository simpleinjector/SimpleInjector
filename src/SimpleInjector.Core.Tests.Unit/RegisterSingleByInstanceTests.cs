#pragma warning disable 0618
namespace SimpleInjector.Tests.Unit
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class RegisterSingleByInstanceTests
    {
        [TestMethod]
        public void RegisterSingleByInstance_WithValidType_ContainerAlwaysReturnsSameInstance()
        {
            // Arrange
            var container = ContainerFactory.New();
            container.RegisterSingleton<IUserRepository>(new SqlUserRepository());

            // Act
            var instance1 = container.GetInstance<IUserRepository>();
            var instance2 = container.GetInstance<IUserRepository>();

            // Assert
            Assert.IsNotNull(instance1, "GetInstance should never return null.");
            Assert.AreEqual(instance1, instance2, "Values should reference the same instance.");
        }

        [TestMethod]
        public void RegisterSingleByInstance_WithNullArgument_ThrowsException()
        {
            // Arrange
            var container = ContainerFactory.New();
            IUserRepository invalidInstance = null;

            // Act
            Action action = () => container.RegisterSingleton<IUserRepository>(invalidInstance);

            // Assert
            AssertThat.Throws<ArgumentNullException>(action);
        }

        [TestMethod]
        public void RegisterSingleByInstance_CalledTwiceOnSameType_ThrowsException()
        {
            // Arrange
            var container = ContainerFactory.New();
            container.RegisterSingleton<IUserRepository>(new SqlUserRepository());

            // Act
            Action action = () => container.RegisterSingleton<IUserRepository>(new InMemoryUserRepository());

            // Assert
            AssertThat.Throws<InvalidOperationException>(action, "A certain type can only be registered once.");
        }

        [TestMethod]
        public void RegisterSingleByInstance_CalledAfterRegisterOnSameType_ThrowsException()
        {
            // Arrange
            var container = ContainerFactory.New();
            container.Register<UserServiceBase>(() => new RealUserService(null));

            // Act
            Action action = () => container.RegisterSingleton<UserServiceBase>(new FakeUserService(null));

            // Assert
            AssertThat.Throws<InvalidOperationException>(action, "A certain type can only be registered once.");
        }

        [TestMethod]
        public void RegisterSingleByInstance_AfterCallingGetInstance_ThrowsException()
        {
            // Arrange
            var container = ContainerFactory.New();
            container.RegisterSingleton<IUserRepository>(new InMemoryUserRepository());
            container.GetInstance<IUserRepository>();

            // Act
            Action action = () => container.RegisterSingleton<UserServiceBase>(new RealUserService(null));

            // Assert
            AssertThat.Throws<InvalidOperationException>(action, "The container should get locked after a call to GetInstance.");
        }

        [TestMethod]
        public void RegisterSingleByInstance_AfterCallingGetAllInstances_ThrowsException()
        {
            // Arrange
            var container = ContainerFactory.New();
            container.RegisterCollection<IUserRepository>();
            var repositories = container.GetAllInstances<IUserRepository>();

            // Calling count will iterate the collections. 
            // The container will only get locked when the first item is retrieved.
            var count = repositories.Count();

            // Act
            Action action = () => container.RegisterSingleton<UserServiceBase>(new RealUserService(null));

            // Assert
            AssertThat.Throws<InvalidOperationException>(action, "The container should get locked after a call to GetAllInstances.");
        }

        [TestMethod]
        public void GetInstance_ForConcreteUnregisteredTypeWithDependencyRegisteredWithRegisterSingle_Succeeds()
        {
            // Arrange
            var container = ContainerFactory.New();

            // This registration will make the DelegateBuilder call the 
            // SingletonInstanceProducer.BuildExpression method.
            container.RegisterSingleton<IUserRepository>(new SqlUserRepository());

            // Act
            container.GetInstance<RealUserService>();
        }
        
        [TestMethod]
        public void RegisterSingleByInstanceNonGeneric_ValidRegistration_GetInstanceReturnsExpectedInstance()
        {
            // Arrange
            var container = ContainerFactory.New();

            object impl = new SqlUserRepository();

            // Act
            container.RegisterSingleton(typeof(IUserRepository), impl);

            // Assert
            Assert.AreEqual(impl, container.GetInstance<IUserRepository>(),
                "GetInstance should return the instance registered using RegisterSingleton.");
        }

        [TestMethod]
        public void RegisterSingleByInstanceNonGeneric_ValidRegistration_GetInstanceAlwaysReturnsSameInstance()
        {
            // Arrange
            var container = ContainerFactory.New();

            object impl = new SqlUserRepository();

            // Act
            container.RegisterSingleton(typeof(IUserRepository), impl);

            var instance1 = container.GetInstance<IUserRepository>();
            var instance2 = container.GetInstance<IUserRepository>();

            // Assert
            Assert.AreEqual(instance1, instance2, "RegisterSingleton should register singleton.");
        }

        [TestMethod]
        public void RegisterSingleByInstanceNonGeneric_ImplementationNoDescendantOfServiceType_ThrowsException()
        {
            // Arrange
            var container = ContainerFactory.New();

            object impl = new List<int>();

            // Act
            Action action = () => container.RegisterSingleton(typeof(IUserRepository), impl);

            // Assert
            AssertThat.Throws<ArgumentException>(action);
        }

        [TestMethod]
        public void RegisterSingleByInstanceNonGeneric_InstanceOfSameTypeAsService_Succeeds()
        {
            // Arrange
            var container = ContainerFactory.New();

            object impl = new List<int>();

            // Act
            container.RegisterSingleton(impl.GetType(), impl);
        }

        [TestMethod]
        public void RegisterSingleByInstanceNonGeneric_NullServiceType_ThrowsException()
        {
            // Arrange
            var container = ContainerFactory.New();

            Type invalidServiceType = null;
            object validInstance = new SqlUserRepository();

            // Act
            Action action = () => container.RegisterSingleton(invalidServiceType, validInstance);

            // Assert
            AssertThat.Throws<ArgumentNullException>(action);
        }

        [TestMethod]
        public void RegisterSingleByInstanceNonGeneric_NullInstance_ThrowsException()
        {
            // Arrange
            var container = ContainerFactory.New();

            Type validServiceType = typeof(IUserRepository);
            object invalidInstance = null;

            // Act
            Action action = () => container.RegisterSingleton(validServiceType, invalidInstance);

            // Assert
            AssertThat.Throws<ArgumentNullException>(action);
        }

        [TestMethod]
        public void GetInstance_ServiceRegisteredUsingRegisterSingleInstanceGeneric_CallsExpressionBuildingWithConstantExpression()
        {
            // Arrange
            var expressionsBuilding = new List<Expression>();

            var container = ContainerFactory.New();

            container.RegisterSingleton<IUserRepository>(new SqlUserRepository());

            container.ExpressionBuilding += (s, e) =>
            {
                expressionsBuilding.Add(e.Expression);
            };

            // Act
            container.GetInstance<IUserRepository>();

            // Assert
            Assert.AreEqual(1, expressionsBuilding.Count);
            AssertThat.IsInstanceOfType(typeof(ConstantExpression), expressionsBuilding.Single());
        }

        [TestMethod]
        public void GetInstance_ServiceRegisteredUsingRegisterSingleInstanceNonGeneric_CallsExpressionBuildingWithConstantExpression()
        {
            // Arrange
            var expressionsBuilding = new List<Expression>();

            var container = ContainerFactory.New();

            container.RegisterSingleton(typeof(IUserRepository), new SqlUserRepository());

            container.ExpressionBuilding += (s, e) =>
            {
                expressionsBuilding.Add(e.Expression);
            };

            // Act
            container.GetInstance<IUserRepository>();

            // Assert
            Assert.AreEqual(1, expressionsBuilding.Count);
            AssertThat.IsInstanceOfType(typeof(ConstantExpression), expressionsBuilding.Single());
        }
    }
}
#pragma warning restore 0618