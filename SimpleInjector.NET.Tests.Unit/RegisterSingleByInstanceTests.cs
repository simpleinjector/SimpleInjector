namespace SimpleInjector.Tests.Unit
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Text;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class RegisterSingleByInstanceTests
    {
        [TestMethod]
        public void RegisterSingleByInstance_WithValidType_ContainerAlwaysReturnsSameInstance()
        {
            // Arrange
            var container = ContainerFactory.New();
            container.RegisterSingle<IUserRepository>(new SqlUserRepository());

            // Act
            var instance1 = container.GetInstance<IUserRepository>();
            var instance2 = container.GetInstance<IUserRepository>();

            // Assert
            Assert.IsNotNull(instance1, "GetInstance should never return null.");
            Assert.AreEqual(instance1, instance2, "Values should reference the same instance.");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RegisterSingleByInstance_WithNullArgument_ThrowsException()
        {
            // Arrange
            var container = ContainerFactory.New();
            IUserRepository invalidInstance = null;

            // Act
            container.RegisterSingle<IUserRepository>(invalidInstance);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException), "A certain type can only be registered once.")]
        public void RegisterSingleByInstance_CalledTwiceOnSameType_ThrowsException()
        {
            // Arrange
            var container = ContainerFactory.New();
            container.RegisterSingle<IUserRepository>(new SqlUserRepository());

            // Act
            container.RegisterSingle<IUserRepository>(new InMemoryUserRepository());
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException), "A certain type can only be registered once.")]
        public void RegisterSingleByInstance_CalledAfterRegisterOnSameType_ThrowsException()
        {
            // Arrange
            var container = ContainerFactory.New();
            container.Register<UserServiceBase>(() => new RealUserService(null));

            // Act
            container.RegisterSingle<UserServiceBase>(new FakeUserService(null));
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException), "The container should get locked after a call to GetInstance.")]
        public void RegisterSingleByInstance_AfterCallingGetInstance_ThrowsException()
        {
            // Arrange
            var container = ContainerFactory.New();
            container.RegisterSingle<IUserRepository>(new InMemoryUserRepository());
            container.GetInstance<IUserRepository>();

            // Act
            container.RegisterSingle<UserServiceBase>(new RealUserService(null));
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException), "The container should get locked after a call to GetAllInstances.")]
        public void RegisterSingleByInstance_AfterCallingGetAllInstances_ThrowsException()
        {
            // Arrange
            var container = ContainerFactory.New();
            var repositories = container.GetAllInstances<IUserRepository>();

            // Calling count will iterate the collections. 
            // The container will only get locked when the first item is retrieved.
            var count = repositories.Count();

            // Act
            container.RegisterSingle<UserServiceBase>(new RealUserService(null));
        }

        [TestMethod]
        public void GetInstance_ForConcreteUnregisteredTypeWithDependencyRegisteredWithRegisterSingle_Succeeds()
        {
            // Arrange
            var container = ContainerFactory.New();

            // This registration will make the DelegateBuilder call the 
            // SingletonInstanceProducer.BuildExpression method.
            container.RegisterSingle<IUserRepository>(new SqlUserRepository());

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
            container.RegisterSingle(typeof(IUserRepository), impl);

            // Assert
            Assert.AreEqual(impl, container.GetInstance<IUserRepository>(),
                "GetInstance should return the instance registered using RegisterSingle.");
        }

        [TestMethod]
        public void RegisterSingleByInstanceNonGeneric_ValidRegistration_GetInstanceAlwaysReturnsSameInstance()
        {
            // Arrange
            var container = ContainerFactory.New();

            object impl = new SqlUserRepository();

            // Act
            container.RegisterSingle(typeof(IUserRepository), impl);

            var instance1 = container.GetInstance<IUserRepository>();
            var instance2 = container.GetInstance<IUserRepository>();

            // Assert
            Assert.AreEqual(instance1, instance2, "RegisterSingle should register singleton.");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void RegisterSingleByInstanceNonGeneric_ImplementationNoDescendantOfServiceType_ThrowsException()
        {
            // Arrange
            var container = ContainerFactory.New();

            object impl = new List<int>();

            // Act
            container.RegisterSingle(typeof(IUserRepository), impl);
        }

        [TestMethod]
        public void RegisterSingleByInstanceNonGeneric_InstanceOfSameTypeAsService_Succeeds()
        {
            // Arrange
            var container = ContainerFactory.New();

            object impl = new List<int>();

            // Act
            container.RegisterSingle(impl.GetType(), impl);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RegisterSingleByInstanceNonGeneric_NullServiceType_ThrowsException()
        {
            // Arrange
            var container = ContainerFactory.New();

            Type invalidServiceType = null;
            object validInstance = new SqlUserRepository();

            // Act
            container.RegisterSingle(invalidServiceType, validInstance);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RegisterSingleByInstanceNonGeneric_NullInstance_ThrowsException()
        {
            // Arrange
            var container = ContainerFactory.New();

            Type validServiceType = typeof(IUserRepository);
            object invalidInstance = null;

            // Act
            container.RegisterSingle(validServiceType, invalidInstance);
        }

        [TestMethod]
        public void GetInstance_ServiceRegisteredUsingRegisterSingleInstanceGeneric_CallsExpressionBuildingWithConstantExpression()
        {
            // Arrange
            var expressionsBuilding = new List<Expression>();

            var container = ContainerFactory.New();

            container.RegisterSingle<IUserRepository>(new SqlUserRepository());

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

            container.RegisterSingle(typeof(IUserRepository), new SqlUserRepository());

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