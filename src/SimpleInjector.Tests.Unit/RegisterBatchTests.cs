namespace SimpleInjector.Tests.Unit
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class RegisterBatchTests
    {
        public interface IBatchCommandHandler<T>
        {
        }

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
        
        public interface ISkipDecorator<T>
        {
        }
        
        private static readonly IEnumerable<Assembly> Assemblies = 
            new[] { typeof(RegisterBatchTests).GetTypeInfo().Assembly };

        [TestMethod]
        public void RegisterAssemblies_WithNonGenericType_Fails()
        {
            // Arrange
            var container = ContainerFactory.New();

            // Act
            Action action = () => container.Register(typeof(ILogger), Assemblies);

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<ArgumentException>(@"
                The supplied type ILogger is not a generic type. This method only supports open generic types.
                If you meant to register all available implementations of ILogger, call 
                RegisterCollection(typeof(ILogger), IEnumerable<Assembly>) instead.".TrimInside(),
                action);
        }
        
        [TestMethod]
        public void RegisterAssemblies_WithClosedGenericType_Fails()
        {
            // Arrange
            var container = ContainerFactory.New();

            // Act
            Action action = () => container.Register(typeof(IService<int, int>), Assemblies);

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<ArgumentException>(string.Format(@"
                The supplied type {0} is not an open generic type. Supply this method with the open generic 
                type {1} to register all available implementations of this type, or call 
                RegisterCollection(Type, IEnumerable<Assembly>) either with the open or closed version of 
                that type to register a collection of instances based on that type.".TrimInside(),
                typeof(IService<int, int>).ToFriendlyName(),
                Types.ToCSharpFriendlyName(typeof(IService<,>))),
                action);
        }
        
        [TestMethod]
        public void RegisterTypes_WithNonGenericType_Fails()
        {
            // Arrange
            var container = ContainerFactory.New();

            // Act
            Action action = () => container.Register(typeof(ILogger), Type.EmptyTypes);

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<ArgumentException>(@"
                The supplied type ILogger is not a generic type. This method only supports open generic types.
                If you meant to register all available implementations of ILogger, call 
                RegisterCollection(typeof(ILogger), IEnumerable<Type>) instead.".TrimInside(),
                action);
        }

        [TestMethod]
        public void RegisterTypes_WithClosedGenericType_Fails()
        {
            // Arrange
            var container = ContainerFactory.New();

            // Act
            Action action = () => container.Register(typeof(IService<int, int>), Type.EmptyTypes);

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<ArgumentException>(string.Format(@"
                The supplied type {0} is not an open generic type. Supply this method with the open generic 
                type {1} to register all available implementations of this type, or call 
                RegisterCollection(Type, IEnumerable<Type>) either with the open or closed version of 
                that type to register a collection of instances based on that type.".TrimInside(),
                typeof(IService<int, int>).ToFriendlyName(),
                Types.ToCSharpFriendlyName(typeof(IService<,>))),
                action);
        }

        [TestMethod]
        public void Register_WithValidTypeDefinitions_Succeeds()
        {
            // Arrange
            var container = ContainerFactory.New();

            // Act
            container.Register(typeof(IService<,>), Assemblies);
        }

        [TestMethod]
        public void Register_WithValidTypeDefinitions_ReturnsExpectedType1()
        {
            // Arrange
            var container = ContainerFactory.New();

            // We've got these concrete public implementations: Concrete1, Concrete2, Concrete3
            container.Register(typeof(IService<,>), Assemblies);

            // Act
            var impl = container.GetInstance<IService<string, object>>();

            // Assert
            AssertThat.IsInstanceOfType(typeof(Concrete1), impl, "Concrete1 implements IService<string, object> directly.");
        }

        [TestMethod]
        public void Register_WithValidTypeDefinitions_ReturnsExpectedType2()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Register(typeof(IService<,>), Assemblies);

            // Act
            var impl = container.GetInstance<IService<int, string>>();

            // Assert
            AssertThat.IsInstanceOfType(typeof(Concrete2), impl, "Concrete2 implements OpenGenericBase<int> which implements IService<int, string>.");
        }

        [TestMethod]
        public void Register_WithValidTypeDefinitions_ReturnsTransientInstances()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Register(typeof(IService<,>), Assemblies);

            // Act
            var impl1 = container.GetInstance<IService<int, string>>();
            var impl2 = container.GetInstance<IService<int, string>>();

            // Assert
            Assert.AreNotEqual(impl1, impl2, "The types should be registered as transient.");
        }

        [TestMethod]
        public void Register_WithValidTypeDefinitions_RespectsTheSuppliedLifestyle3()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Register(typeof(IService<,>), Assemblies, Lifestyle.Transient);

            // Act
            var impl1 = container.GetInstance<IService<int, string>>();
            var impl2 = container.GetInstance<IService<int, string>>();

            // Assert
            Assert.AreNotEqual(impl1, impl2, "The types should be registered as transient.");
        }

        [TestMethod]
        public void Register_WithValidTypeDefinitions_RespectsTheSuppliedLifestyle4()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Register(typeof(IService<,>), Assemblies, Lifestyle.Singleton);

            // Act
            var impl1 = container.GetInstance<IService<int, string>>();
            var impl2 = container.GetInstance<IService<int, string>>();

            // Assert
            Assert.AreEqual(impl1, impl2, "The type should be returned as singleton.");
        }

        [TestMethod]
        public void Register_WithValidTypeDefinitions_RespectsTheSuppliedLifestyle5()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Register(typeof(IService<,>), Assemblies, Lifestyle.Singleton);

            // Act
            var impl1 = container.GetInstance<IService<int, string>>();
            var impl2 = container.GetInstance<IService<int, string>>();

            // Assert
            Assert.AreEqual(impl1, impl2, "The type should be returned as singleton.");
        }

        [TestMethod]
        public void Register_WithValidTypeDefinitions_ReturnsExpectedType3()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Register(typeof(IService<,>), Assemblies);

            // Act
            var impl = container.GetInstance<IService<float, double>>();

            // Assert
            AssertThat.IsInstanceOfType(typeof(Concrete3), impl, 
                "Concrete3 implements INonGeneric which implements IService<float, double>.");
        }

        [TestMethod]
        public void Register_WithValidTypeDefinitions_ReturnsExpectedType4()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Register(typeof(IService<,>), Assemblies);

            // Act
            var impl = container.GetInstance<IService<Type, Type>>();

            // Assert
            AssertThat.IsInstanceOfType(typeof(Concrete3), impl, "Concrete3 implements IService<Type, Type>.");
        }

        [TestMethod]
        public void Register_WithMultipleTypeDefinitionsReferencingTheSameInterface_Fails()
        {
            // Arrange
            var container = ContainerFactory.New();

            // Act
            // This call should fail, because both Invalid1 and Invalid2 implement the same closed generic
            // interface IInvalid<int, double>
            Action action = () => container.Register(typeof(IInvalid<,>), Assemblies);

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<InvalidOperationException>(
                "There are 2 types in the supplied list of types or assemblies that represent the same " +
                "closed generic type",
                action);
        }

        [TestMethod]
        public void Register_WithMultipleConcreteTypes_RegistersTheExpectedServiceTypes()
        {
            // Arrange
            var container = ContainerFactory.New();

            // Act
            container.Register(typeof(IService<,>), new[] { typeof(Concrete1), typeof(Concrete2) });

            // Assert
            var impl = container.GetInstance<IService<string, object>>();
            var imp2 = container.GetInstance<IService<int, string>>();

            AssertThat.IsInstanceOfType(typeof(Concrete1), impl, "Concrete1 implements IService<string, object>.");
            AssertThat.IsInstanceOfType(typeof(Concrete1), impl, "Concrete2 implements IService<int, string>.");
        }

        [TestMethod]
        public void Register_WithNonInheritableType_ThrowsException()
        {
            // Arrange
            var container = ContainerFactory.New();

            var serviceType = typeof(IService<,>);
            var validType = typeof(ServiceImpl<object, string>);
            var invalidType = typeof(List<int>);

            // Act
            Action action = () => container.Register(serviceType, new[] { validType, invalidType });

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<Exception>("List<Int32>", action);
            AssertThat.ThrowsWithExceptionMessageContains<Exception>("IService<TA, TB>", action);
        }

        [TestMethod]
        public void Register_WithNullAssemblyParamsArgument_ThrowsException()
        {
            // Arrange
            var container = ContainerFactory.New();

            Assembly[] invalidArgument = null;

            // Act
            Action action = () => container.Register(typeof(IService<,>), invalidArgument);

            // Assert
            AssertThat.Throws<ArgumentNullException>(action);
        }

        [TestMethod]
        public void Register_WithNullAssemblyIEnumerableArgument_ThrowsException()
        {
            // Arrange
            var container = ContainerFactory.New();

            IEnumerable<Assembly> invalidArgument = null;

            // Act
            Action action = () => container.Register(typeof(IService<,>), invalidArgument);

            // Assert
            AssertThat.Throws<ArgumentNullException>(action);
        }

        [TestMethod]
        public void Register_WithNullTypesToRegister_ThrowsException()
        {
            // Arrange
            var container = ContainerFactory.New();

            var validServiceType = typeof(IService<,>);
            IEnumerable<Type> invalidTypesToRegister = null;

            // Act
            Action action = () => container.Register(validServiceType, invalidTypesToRegister);

            // Assert
            AssertThat.Throws<ArgumentNullException>(action);
        }

        [TestMethod]
        public void Register_WithNullOpenGenericServiceType_ThrowsException()
        {
            // Arrange
            var container = ContainerFactory.New();

            Type invalidServiceType = null;
            IEnumerable<Type> validTypesToRegister = new Type[] { typeof(object) };

            // Act
            Action action = () => container.Register(invalidServiceType, validTypesToRegister);

            // Assert
            AssertThat.Throws<ArgumentNullException>(action);
        }

        [TestMethod]
        public void Register_WithNullElementInTypesToRegister_ThrowsException()
        {
            // Arrange
            var container = ContainerFactory.New();

            Type validServiceType = typeof(IService<,>);
            IEnumerable<Type> invalidTypesToRegister = new Type[] { null };

            // Act
            Action action = () => container.Register(validServiceType, invalidTypesToRegister);

            // Assert
            AssertThat.Throws<ArgumentException>(action);
        }

        [TestMethod]
        public void GetInstance_ImplementationWithMultipleInterfaces_ReturnsThatImplementationForEachInterface()
        {
            // Arrange
            var container = ContainerFactory.New();

            // MultiInterfaceHandler implements two interfaces.
            container.Register(typeof(IBatchCommandHandler<>), Assemblies);

            // Act
            var instance1 = container.GetInstance<IBatchCommandHandler<int>>();
            var instance2 = container.GetInstance<IBatchCommandHandler<double>>();

            // Assert
            AssertThat.IsInstanceOfType(typeof(MultiInterfaceHandler), instance1);
            AssertThat.IsInstanceOfType(typeof(MultiInterfaceHandler), instance2);
        }

        [TestMethod]
        public void GetInstance_BatchRegistrationUsingSingletonLifestyle_AlwaysReturnsTheSameInstanceForItsInterfaces()
        {
            // Arrange
            var container = ContainerFactory.New();

            // MultiInterfaceHandler implements ICommandHandler<int> and ICommandHandler<double>.
            container.Register(typeof(IBatchCommandHandler<>), Assemblies, Lifestyle.Singleton);

            // Act
            var instance1 = container.GetInstance<IBatchCommandHandler<int>>();
            var instance2 = container.GetInstance<IBatchCommandHandler<double>>();

            // Assert
            Assert.IsTrue(object.ReferenceEquals(instance1, instance2));
        }

        [TestMethod]
        public void RegisterTypes_SuppliedWithOpenGenericType_FailsWithExpectedException()
        {
            // Arrange
            var container = ContainerFactory.New();

            Type[] types = new[] { typeof(GenericHandler<>) };

            // Act
            Action action = () => container.Register(typeof(IBatchCommandHandler<>), types);

            // Assert
            AssertThat.ThrowsWithParamName("implementationTypes", action);
            AssertThat.ThrowsWithExceptionMessageContains<ArgumentException>(@"
                The supplied list of types contains one or multiple open generic types, but this method 
                is unable to handle open generic types because it can only map closed generic service 
                types to a single implementation. You must register the open-generic types separately
                using the Register(Type, Type) overload."
                .TrimInside(),
                action);
        }

        [TestMethod]
        public void RegisterAssembliesLifestyle_WithValidTypeDefinitions_RespectsTheSuppliedLifestyle1()
        {
            // Arrange
            var container = ContainerFactory.New();
            container.Register(typeof(IService<,>), Assemblies, Lifestyle.Transient);

            // Act
            var impl1 = container.GetInstance<IService<int, string>>();
            var impl2 = container.GetInstance<IService<int, string>>();

            // Assert
            Assert.AreNotEqual(impl1, impl2, "The types should be registered as transient.");
        }

        [TestMethod]
        public void RegisterAssembliesLifestyle_WithValidTypeDefinitions_RespectsTheSuppliedLifestyle2()
        {
            // Arrange
            var container = ContainerFactory.New();
            container.Register(typeof(IService<,>), Assemblies, Lifestyle.Singleton);

            // Act
            var impl1 = container.GetInstance<IService<int, string>>();
            var impl2 = container.GetInstance<IService<int, string>>();

            // Assert
            Assert.AreEqual(impl1, impl2, "The type should be returned as singleton.");
        }

        // This is a regression test for work item 21002
        [TestMethod]
        public void RegisterAssembliesLifestyle_RegistersTypeWithTheeImplementations_ResolvesThatTypeAsExpected()
        {
            // Arrange
            var container = new Container();

            // Just registers RequestGroup in three groups.
            container.Register(typeof(IHandler<,>), Assemblies, Lifestyle.Singleton);

            container.RegisterDecorator(typeof(IHandler<,>), typeof(RequestDecorator<,>));

            // Act
            // RequestGroup implements all three these interfaces.
            var decorator1 = container.GetInstance<IHandler<Query1, int>>() as RequestDecorator<Query1, int>;

            // This call fails in v2.1.0 to v2.7.1
            var decorator2 = container.GetInstance<IHandler<Query2, double>>() as RequestDecorator<Query2, double>;
            var decorator3 = container.GetInstance<IHandler<Query3, double>>() as RequestDecorator<Query3, double>;

            // Assert
            Assert.AreSame(decorator1.Decoratee, decorator2.Decoratee);
            Assert.AreSame(decorator2.Decoratee, decorator3.Decoratee);
        }

        [TestMethod]
        public void RegisterTypesLifestyle_RegistersTypeWithTheeImplementations_ResolvesThatTypeAsExpected()
        {
            // Arrange
            var container = new Container();

            container.Register(typeof(IHandler<,>), new Type[] { typeof(RequestGroup) }, Lifestyle.Singleton);

            container.RegisterDecorator(typeof(IHandler<,>), typeof(RequestDecorator<,>));

            // Act
            // RequestGroup implements all three these interfaces.
            var decorator1 = container.GetInstance<IHandler<Query1, int>>() as RequestDecorator<Query1, int>;
            var decorator2 = container.GetInstance<IHandler<Query2, double>>() as RequestDecorator<Query2, double>;
            var decorator3 = container.GetInstance<IHandler<Query3, double>>() as RequestDecorator<Query3, double>;

            // Assert
            Assert.AreSame(decorator1.Decoratee, decorator2.Decoratee);
            Assert.AreSame(decorator2.Decoratee, decorator3.Decoratee);
        }

        [TestMethod]
        public void Register_WithSkippedDecorator_ResolveAsDependencyThrowsExpectedException()
        {
            // Arrange
            var container = new Container();

            container.Register(typeof(ISkipDecorator<>), Assemblies);

            // Act
            Action action = () => container.GetInstance<SkippedDecoratorController>();

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<ActivationException>(
                "was skipped during batch-registration by the container because it is considered to be a decorator",
                action);
        }

        [TestMethod]
        public void Register_WithSkippedDecorator_DirectResolveThrowsExpectedException()
        {
            // Arrange
            var container = new Container();

            container.Register(typeof(ISkipDecorator<>), Assemblies);

            // Act
            Action action = () => container.GetInstance<ISkipDecorator<int>>();

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<ActivationException>(
                "was skipped during batch-registration by the container because it is considered to be a decorator",
                action);
        }

        public class SkippedDecorator : ISkipDecorator<int>
        {
            public SkippedDecorator(ISkipDecorator<int> validator)
            {
            }
        }

        public class SkippedDecoratorController
        {
            public SkippedDecoratorController(ISkipDecorator<int> service)
            {
            }
        }

        #region IInvalid

        // Both Invalid1 and Invalid2 implement the same closed generic type.
        public class Invalid1 : IInvalid<int, double>
        {
        }

        public class Invalid2 : IInvalid<int, double>
        {
        }

        #endregion

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

        public class MultiInterfaceHandler : IBatchCommandHandler<int>, IBatchCommandHandler<double>
        {
        }

        public class DecimalHandler : IBatchCommandHandler<decimal>
        {
        }

        public class FloatHandler : IBatchCommandHandler<float>
        {
        }

        public class ObjectHandler : IBatchCommandHandler<object>
        {
        }

        public class GenericHandler<T> : IBatchCommandHandler<T>
        {
        }

        public class GenericStructHandler<T> : IBatchCommandHandler<T> where T : struct
        {
        }

        public class ServiceDecorator : IService<int, object>
        {
            public ServiceDecorator(IService<int, object> decorated)
            {
            }
        }

        #endregion
    }
}