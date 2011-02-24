using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Reflection;

using CuttingEdge.ServiceLocation;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Practices.ServiceLocation;

namespace CuttingEdge.ServiceLocator.Extensions.Tests.Unit
{
    [TestClass]
    public class SslBatchRegistrationExtensionsTests
    {
        // This is the open generic interface that will be used as service type.
        public interface IService<TA, TB>
        {
        }

        // An non-generic interface that inherits from the closed generic IGenericService.
        public interface INonGeneric : IService<float, double>
        {
        }

        public interface IInvalid<TA, TB>
        {
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void RegisterOpenGeneric_WithClosedGenericType_Fails()
        {
            // Arrange
            var container = new SimpleServiceLocator();

            // Act
            container.RegisterManyForOpenGeneric(typeof(IService<int, int>), Assembly.GetExecutingAssembly());
        }

        [TestMethod]
        public void RegisterOpenGeneric_WithValidTypeDefinitions_Succeeds()
        {
            // Arrange
            var container = new SimpleServiceLocator();

            // Act
            container.RegisterManyForOpenGeneric(typeof(IService<,>), Assembly.GetExecutingAssembly());
        }

        [TestMethod]
        public void RegisterManyForOpenGeneric_WithValidTypeDefinitions_ReturnsExpectedType1()
        {
            // Arrange
            var container = new SimpleServiceLocator();
            container.RegisterManyForOpenGeneric(typeof(IService<,>), Assembly.GetExecutingAssembly());

            // Act
            var impl = container.GetInstance<IService<string, object>>();

            // Assert
            Assert.IsInstanceOfType(impl, typeof(Concrete1),
                "Concrete1 implements IService<string, object> directly.");
        }

        [TestMethod]
        public void RegisterManyForOpenGeneric_WithValidTypeDefinitions_ReturnsExpectedType2()
        {
            // Arrange
            var container = new SimpleServiceLocator();
            container.RegisterManyForOpenGeneric(typeof(IService<,>), Assembly.GetExecutingAssembly());

            // Act
            var impl = container.GetInstance<IService<int, string>>();

            // Assert
            Assert.IsInstanceOfType(impl, typeof(Concrete2),
                "Concrete2 implements OpenGenericBase<int> which implements IService<int, string>.");
        }

        [TestMethod]
        public void RegisterManyForOpenGeneric_IncludingInternalTypes_ReturnsExpectedType()
        {
            // Arrange
            var container = new SimpleServiceLocator();
            container.RegisterManyForOpenGeneric(typeof(IService<,>), true, Assembly.GetExecutingAssembly());

            // Act
            var impl = container.GetInstance<IService<decimal, decimal>>();

            // Assert
            Assert.IsInstanceOfType(impl, typeof(Concrete4),
                "Internal type Concrete4 should be found.");
        }

        [TestMethod]
        [ExpectedException(typeof(ActivationException))]
        public void RegisterManyForOpenGeneric_ExcludingInternalTypes_ReturnsExpectedType()
        {
            // Arrange
            var container = new SimpleServiceLocator();
            container.RegisterManyForOpenGeneric(typeof(IService<,>), false, Assembly.GetExecutingAssembly());

            // Act
            container.GetInstance<IService<decimal, decimal>>();
        }

        [TestMethod]
        public void RegisterManyForOpenGeneric_WithValidTypeDefinitions_ReturnsExpectedType3()
        {
            // Arrange
            var container = new SimpleServiceLocator();
            container.RegisterManyForOpenGeneric(typeof(IService<,>), Assembly.GetExecutingAssembly());

            // Act
            var impl = container.GetInstance<IService<float, double>>();

            // Assert
            Assert.IsInstanceOfType(impl, typeof(Concrete3),
                "Concrete3 implements INonGeneric which implements IService<float, double>.");
        }

        [TestMethod]
        public void RegisterManyForOpenGeneric_WithValidTypeDefinitions_ReturnsExpectedType4()
        {
            // Arrange
            var container = new SimpleServiceLocator();
            container.RegisterManyForOpenGeneric(typeof(IService<,>), Assembly.GetExecutingAssembly());

            // Act
            var impl = container.GetInstance<IService<Type, Type>>();

            // Assert
            Assert.IsInstanceOfType(impl, typeof(Concrete3),
                "Concrete3 implements implements IService<Type, Type>.");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void RegisterManyForOpenGeneric_WithMultipleTypeDefinitionsReferencingTheSameInterface_Fails()
        {
            // Arrange
            var container = new SimpleServiceLocator();

            // Act
            // This call should fail, because both Invalid1 and Invalid2 implement the same closed generic
            // interface IInvalid<int, double>
            container.RegisterManyForOpenGeneric(typeof(IInvalid<,>), Assembly.GetExecutingAssembly());
        }

        [TestMethod]
        public void RegisterManyForOpenGeneric_WithMultipleConcreteTypes_RegistersTheExpectedServiceTypes()
        {
            // Arrange
            var container = new SimpleServiceLocator();

            // Act
            container.RegisterManyForOpenGeneric(typeof(IService<,>), typeof(Concrete1), typeof(Concrete2));

            // Assert
            var impl = container.GetInstance<IService<string, object>>();
            var imp2 = container.GetInstance<IService<int, string>>();

            Assert.IsInstanceOfType(impl, typeof(Concrete1),
                "Concrete1 implements implements IService<string, object>.");
            Assert.IsInstanceOfType(impl, typeof(Concrete1),
                "Concrete2 implements implements IService<int, string>.");
        }

        [TestMethod]
        public void RegisterManyForOpenGeneric_WithNonInheritableType_ThrowsException()
        {
            // Arrange
            var container = new SimpleServiceLocator();

            var serviceType = typeof(IService<,>);
            var validType = typeof(ServiceImpl<object, string>);
            var invalidType = typeof(SqlConnection);

            try
            {
                // Act
                container.RegisterManyForOpenGeneric(serviceType, validType, invalidType);

                // Assert
                Assert.Fail("Exception expected.");
            }
            catch (Exception ex)
            {
                const string AssertMessage = "Exception message not descriptive. Actual message: ";
                Assert.IsTrue(ex.Message.Contains(invalidType.Name), AssertMessage + ex.Message);
                Assert.IsTrue(ex.Message.Contains(serviceType.Name), AssertMessage + ex.Message);
            }
        }
        
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RegisterManyForOpenGeneric_WithNullAssemblyParamsArgument_ThrowsException()
        {
            // Arrange
            var container = new SimpleServiceLocator();

            Assembly[] invalidArgument = null;

            // Act
            container.RegisterManyForOpenGeneric(typeof(IService<,>), invalidArgument);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RegisterManyForOpenGeneric_WithNullAssemblyIEnumerableArgument_ThrowsException()
        {
            // Arrange
            var container = new SimpleServiceLocator();

            IEnumerable<Assembly> invalidArgument = null;

            // Act
            container.RegisterManyForOpenGeneric(typeof(IService<,>), invalidArgument);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RegisterManyForOpenGeneric_WithNullContainer_ThrowsException()
        {
            // Arrange
            SimpleServiceLocator invalidContainer = null;
            var validServiceType = typeof(IService<,>);
            var validAssembly = Assembly.GetExecutingAssembly();

            // Act
            invalidContainer.RegisterManyForOpenGeneric(validServiceType, validAssembly);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RegisterManyForOpenGeneric_WithNullTypesToRegister_ThrowsException()
        {
            // Arrange
            var container = new SimpleServiceLocator();

            var validServiceType = typeof(IService<,>);
            IEnumerable<Type> invalidTypesToRegister = null;

            // Act
            container.RegisterManyForOpenGeneric(validServiceType, invalidTypesToRegister);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RegisterManyForOpenGeneric_WithNullOpenGenericServiceType_ThrowsException()
        {
            // Arrange
            var container = new SimpleServiceLocator();

            Type invalidServiceType = null;
            IEnumerable<Type> validTypesToRegister = new Type[] { typeof(object) };

            // Act
            container.RegisterManyForOpenGeneric(invalidServiceType, validTypesToRegister);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void RegisterManyForOpenGeneric_WithNullElementInTypesToRegister_ThrowsException()
        {
            // Arrange
            var container = new SimpleServiceLocator();

            Type validServiceType = typeof(IService<,>);
            IEnumerable<Type> invalidTypesToRegister = new Type[] { null };

            // Act
            container.RegisterManyForOpenGeneric(validServiceType, invalidTypesToRegister);
        }

        #region IService

        public class ServiceImpl<TA, TB> : IService<TA, TB>
        {
        }

        // An generic abstract class. Should not be used by the registration.
        public abstract class OpenGenericBase<T> : IService<T, string>
        {
        }

        // A non-generic abstract class. Should not be used by the registration.
        public abstract class ClosedGenericBase : IService<int, object>
        {
        }

        // A non-abstract generic type. Should not be used by the registration.
        public class OpenGeneric<T> : OpenGenericBase<T>
        {
        }

        // Instance of this type should be returned on container.GetInstance<IService<int, string>>()
        public class Concrete1 : IService<string, object>
        {
        }

        // Instance of this type should be returned on container.GetInstance<IService<string, object>>()
        public class Concrete2 : OpenGenericBase<int>
        {
        }

        // Instance of this type should be returned on container.GetInstance<IService<float, double>>() and
        // on container.GetInstance<IService<Type, Type>>()
        public class Concrete3 : INonGeneric, IService<Type, Type>
        {
        }

        // Internal type.
        private class Concrete4 : IService<decimal, decimal>
        {
        }

        #endregion

        #region IInvalid

        // Both Invalid1 and Invalid2 implement the same closed generic type.
        public class Invalid1 : IInvalid<int, double>
        {
        }

        public class Invalid2 : IInvalid<int, double>
        {
        }

        #endregion
    }
}