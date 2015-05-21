namespace SimpleInjector.Tests.Unit.Extensions
{
    // Suppress the Obsolete warnings, since we want to test GetTypesToRegister overloads that are marked obsolete.
#pragma warning disable 0618
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SimpleInjector.Extensions;

    /// <summary>Normal tests.</summary>
    [TestClass]
    public partial class BatchRegistrationExtensionsTests
    {
        public interface ICommandHandler<T> 
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

        [TestMethod]
        public void GetTypesToRegister_WithContainerArgument_ReturnsNoDecorators()
        {
            // Arrange
            var container = new Container();

            // Act
            var types = 
                container.GetTypesToRegister(typeof(IService<,>), new[] { typeof(IService<,>).Assembly });

            // Assert
            Assert.IsFalse(types.Any(type => type == typeof(ServiceDecorator)), "The decorator was included.");
        }

        [TestMethod]
        public void RegisterManyForOpenGeneric_WithClosedGenericType_Fails()
        {
            // Arrange
            var container = ContainerFactory.New();

            var assemblies = new[] { Assembly.GetExecutingAssembly() };

            // Act
            Action action = () => container.Register(typeof(IService<int, int>), assemblies);

            // Assert
            AssertThat.Throws<ArgumentException>(action);
        }

        [TestMethod]
        public void RegisterManyForOpenGeneric_WithValidTypeDefinitions_Succeeds()
        {
            // Arrange
            var container = ContainerFactory.New();

            // Act
            container.Register(typeof(IService<,>), new[] { Assembly.GetExecutingAssembly() });
        }

        [TestMethod]
        public void RegisterManyForOpenGeneric_WithValidTypeDefinitions_ReturnsExpectedType1()
        {
            // Arrange
            var container = ContainerFactory.New();

            // We've got these concrete public implementations: Concrete1, Concrete2, Concrete3
            container.Register(typeof(IService<,>), new[] { Assembly.GetExecutingAssembly() });

            // Act
            var impl = container.GetInstance<IService<string, object>>();

            // Assert
            AssertThat.IsInstanceOfType(typeof(Concrete1), impl, "Concrete1 implements IService<string, object> directly.");
        }

        [TestMethod]
        public void RegisterManyForOpenGeneric_WithValidTypeDefinitions_ReturnsExpectedType2()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Register(typeof(IService<,>), new[] { Assembly.GetExecutingAssembly() });

            // Act
            var impl = container.GetInstance<IService<int, string>>();

            // Assert
            AssertThat.IsInstanceOfType(typeof(Concrete2), impl, "Concrete2 implements OpenGenericBase<int> which implements IService<int, string>.");
        }

        [TestMethod]
        public void RegisterManyForOpenGeneric_WithValidTypeDefinitions_ReturnsTransientInstances()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Register(typeof(IService<,>), new[] { Assembly.GetExecutingAssembly() });

            // Act
            var impl1 = container.GetInstance<IService<int, string>>();
            var impl2 = container.GetInstance<IService<int, string>>();

            // Assert
            Assert.AreNotEqual(impl1, impl2, "The types should be registered as transient.");
        }

        [TestMethod]
        public void RegisterManyForOpenGeneric_WithValidTypeDefinitions_RespectsTheSuppliedLifestyle3()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Register(typeof(IService<,>), new[] { Assembly.GetExecutingAssembly() }, Lifestyle.Transient);

            // Act
            var impl1 = container.GetInstance<IService<int, string>>();
            var impl2 = container.GetInstance<IService<int, string>>();

            // Assert
            Assert.AreNotEqual(impl1, impl2, "The types should be registered as transient.");
        }

        [TestMethod]
        public void RegisterManyForOpenGeneric_WithValidTypeDefinitions_RespectsTheSuppliedLifestyle4()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Register(typeof(IService<,>), new[] { Assembly.GetExecutingAssembly() }, Lifestyle.Singleton);

            // Act
            var impl1 = container.GetInstance<IService<int, string>>();
            var impl2 = container.GetInstance<IService<int, string>>();

            // Assert
            Assert.AreEqual(impl1, impl2, "The type should be returned as singleton.");
        }

        [TestMethod]
        public void RegisterManyForOpenGeneric_WithValidTypeDefinitions_RespectsTheSuppliedLifestyle5()
        {
            // Arrange
            IEnumerable<Assembly> assemblies = new[] { Assembly.GetExecutingAssembly() };

            var container = ContainerFactory.New();

            container.Register(typeof(IService<,>), assemblies, Lifestyle.Singleton);

            // Act
            var impl1 = container.GetInstance<IService<int, string>>();
            var impl2 = container.GetInstance<IService<int, string>>();

            // Assert
            Assert.AreEqual(impl1, impl2, "The type should be returned as singleton.");
        }

        [TestMethod]
        public void RegisterManyForOpenGeneric_WithValidTypeDefinitions_ReturnsExpectedType3()
        {
            // Arrange
            var container = ContainerFactory.New();
            
            container.Register(typeof(IService<,>), new[] { Assembly.GetExecutingAssembly() });

            // Act
            var impl = container.GetInstance<IService<float, double>>();

            // Assert
            AssertThat.IsInstanceOfType(typeof(Concrete3), impl, "Concrete3 implements INonGeneric which implements IService<float, double>.");
        }

        [TestMethod]
        public void RegisterManyForOpenGeneric_WithValidTypeDefinitions_ReturnsExpectedType4()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Register(typeof(IService<,>), new[] { Assembly.GetExecutingAssembly() });

            // Act
            var impl = container.GetInstance<IService<Type, Type>>();

            // Assert
            AssertThat.IsInstanceOfType(typeof(Concrete3), impl, "Concrete3 implements IService<Type, Type>.");
        }

        [TestMethod]
        public void RegisterManyForOpenGeneric_WithMultipleTypeDefinitionsReferencingTheSameInterface_Fails()
        {
            // Arrange
            var container = ContainerFactory.New();

            // Act
            // This call should fail, because both Invalid1 and Invalid2 implement the same closed generic
            // interface IInvalid<int, double>
            Action action = () => container.Register(typeof(IInvalid<,>), new[] { Assembly.GetExecutingAssembly() });

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<InvalidOperationException>(
                "There are 2 types in the supplied list of types or assemblies that represent the same " + 
                "closed generic type",
                action);
        }

        [TestMethod]
        public void RegisterManyForOpenGeneric_WithMultipleConcreteTypes_RegistersTheExpectedServiceTypes()
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
        public void RegisterManyForOpenGeneric_WithNonInheritableType_ThrowsException()
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
        public void RegisterManyForOpenGeneric_WithNullAssemblyParamsArgument_ThrowsException()
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
        public void RegisterManyForOpenGeneric_WithNullAssemblyIEnumerableArgument_ThrowsException()
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
        public void RegisterManyForOpenGeneric_WithNullTypesToRegister_ThrowsException()
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
        public void RegisterManyForOpenGeneric_WithNullOpenGenericServiceType_ThrowsException()
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
        public void RegisterManyForOpenGeneric_WithNullElementInTypesToRegister_ThrowsException()
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
        public void RegisterManyForOpenGenericEnumerable_WithValidArguments_Succeeds()
        {
            // Arrange
            var container = ContainerFactory.New();

            BatchRegistrationCallback callback = (closedServiceType, implementations) => { };

            IEnumerable<Assembly> assemblies = new[] { Assembly.GetExecutingAssembly() };

            // Act
            container.Register(typeof(IService<,>), assemblies);
        }

        [TestMethod]
        public void RegisterManyForOpenGeneric_WithCallback_IsCalledTheExpectedAmountOfTimes()
        {
            // Arrange
            var container = ContainerFactory.New();

            // class Concrete3 : IService<float, double>, IService<Type, Type>
            container.RegisterCollection(typeof(IService<,>), new[] { typeof(Concrete3) });

            // Act
            container.GetAllInstances<IService<float, double>>().Single();
            container.GetAllInstances<IService<Type, Type>>().Single();
        }

        [TestMethod]
        public void GetInstance_ImplementationWithMultipleInterfaces_ReturnsThatImplementationForEachInterface()
        {
            // Arrange
            var container = ContainerFactory.New();

            // MultiInterfaceHandler implements two interfaces.
            container.Register(typeof(ICommandHandler<>), new[] { Assembly.GetExecutingAssembly() });

            // Act
            var instance1 = container.GetInstance<ICommandHandler<int>>();
            var instance2 = container.GetInstance<ICommandHandler<double>>();

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
            container.Register(typeof(ICommandHandler<>), new[] { Assembly.GetExecutingAssembly() }, Lifestyle.Singleton);

            // Act
            var instance1 = container.GetInstance<ICommandHandler<int>>();
            var instance2 = container.GetInstance<ICommandHandler<double>>();

            // Assert
            Assert.IsTrue(object.ReferenceEquals(instance1, instance2));
        }

        [TestMethod]
        public void RegisterManyForOpenGenericWithoutCallback1_SuppliedWithOpenGenericType_FailsWithExpectedException()
        {
            // Arrange
            var container = ContainerFactory.New();

            Type[] types = new[] { typeof(GenericHandler<>) };

            // Act
            Action action = () => container.Register(typeof(ICommandHandler<>), types);

            // Assert
            AssertThat.ThrowsWithParamName("implementationTypes", action);
            AssertThat.ThrowsWithExceptionMessageContains<ArgumentException>(@"
                The supplied list of types contains one or multiple open generic types, but this method 
                is unable to handle open generic types because it can only map closed-generic service 
                types to a single implementation. Try using RegisterCollection instead."
                .TrimInside(),
                action);
        }

        [TestMethod]
        public void RegisterManyForOpenGenericWithoutCallback2_SuppliedWithOpenGenericType_FailsWithExpectedException()
        {
            // Arrange
            var container = ContainerFactory.New();

            IEnumerable<Type> types = new[] { typeof(GenericHandler<>) };

            // Act
            Action action = () => container.Register(typeof(ICommandHandler<>), types);

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<ArgumentException>(
                "The supplied list of types contains one or multiple open generic types", action);
        }

        [TestMethod]
        public void RegisterManyForOpenGenericWithCallback_SuppliedWithOpenGenericType_Succeeds()
        {
            // Arrange
            var container = ContainerFactory.New();

            var types = new[] { typeof(GenericHandler<>) };

            container.RegisterCollection(typeof(ICommandHandler<>), types);

            // Act
            var handler = container.GetAllInstances<ICommandHandler<int>>().Single();

            // Assert
            AssertThat.IsInstanceOfType(typeof(GenericHandler<int>), handler);
        }

        [TestMethod]
        public void RegisterManyForOpenGenericWithCallback_SuppliedWithOpenGenericType_ReturnsTheExpectedClosedGenericVersion()
        {
            // Arrange
            var registeredTypes = new[] { typeof(DecimalHandler), typeof(GenericHandler<>) };

            var expected = new[] { typeof(DecimalHandler), typeof(GenericHandler<decimal>) };
                        
            var container = ContainerFactory.New();

            container.RegisterCollection(typeof(ICommandHandler<>), registeredTypes);

            // Act
            var handlers = container.GetAllInstances<ICommandHandler<decimal>>();

            // Assert
            var actual = handlers.Select(handler => handler.GetType()).ToArray();
            Assert.AreEqual(expected.ToFriendlyNamesText(), actual.ToFriendlyNamesText());
        }

        [TestMethod]
        public void RegisterManyForOpenGenericWithCallback_SuppliedWithOpenGenericTypeWithCompatibleTypeConstraint_ReturnsThatGenericType()
        {
            // Arrange
            var registeredTypes = new[] { typeof(FloatHandler), typeof(GenericStructHandler<>) };

            var expected = new[] { typeof(FloatHandler), typeof(GenericStructHandler<float>) };

            var container = ContainerFactory.New();

            container.RegisterCollection(typeof(ICommandHandler<>), registeredTypes);

            // Assert
            var handlers = container.GetAllInstances<ICommandHandler<float>>();

            // Assert
            var actual = handlers.Select(handler => handler.GetType()).ToArray();
            Assert.AreEqual(expected.ToFriendlyNamesText(), actual.ToFriendlyNamesText());
        }

        [TestMethod]
        public void RegisterManyForOpenGenericWithCallback_SuppliedWithOpenGenericTypeWithIncompatibleTypeConstraint_DoesNotReturnThatGenericType()
        {
            // Arrange
            var registeredTypes = new[] { typeof(ObjectHandler), typeof(GenericStructHandler<>) };

            var expected = new[] { typeof(ObjectHandler) };
            
            var container = ContainerFactory.New();

            container.RegisterCollection(typeof(ICommandHandler<>), registeredTypes);

            // Assert
            var handlers = container.GetAllInstances<ICommandHandler<object>>();

            // Assert
            var actual = handlers.Select(handler => handler.GetType()).ToArray();
            Assert.AreEqual(expected.ToFriendlyNamesText(), actual.ToFriendlyNamesText());
        }

        [TestMethod]
        public void RegisterManyForOpenGenericAccessibilityOptionAndParams_WithValidTypeDefinitions_Succeeds()
        {
            // Arrange
            var container = ContainerFactory.New();

            // Act
            container.Register(typeof(IService<,>), new[] { Assembly.GetExecutingAssembly() });
        }

        [TestMethod]
        public void RegisterManyForOpenGenericAccessibilityOptionAndEnumerable_WithValidTypeDefinitions_Succeeds()
        {
            // Arrange
            var container = ContainerFactory.New();

            IEnumerable<Assembly> assemblies = new[] { Assembly.GetExecutingAssembly() };

            // Act
            container.Register(typeof(IService<,>), assemblies);
        }

        [TestMethod]
        public void RegisterManyForOpenGeneric_WithValidTypeDefinitions_RespectsTheSuppliedLifestyle1()
        {
            // Arrange
            var container = ContainerFactory.New();
            container.Register(typeof(IService<,>), new[] { Assembly.GetExecutingAssembly() }, Lifestyle.Transient);

            // Act
            var impl1 = container.GetInstance<IService<int, string>>();
            var impl2 = container.GetInstance<IService<int, string>>();

            // Assert
            Assert.AreNotEqual(impl1, impl2, "The types should be registered as transient.");
        }

        [TestMethod]
        public void RegisterManyForOpenGeneric_WithValidTypeDefinitions_RespectsTheSuppliedLifestyle2()
        {
            // Arrange
            var container = ContainerFactory.New();
            container.Register(typeof(IService<,>), new[] { Assembly.GetExecutingAssembly() }, Lifestyle.Singleton);

            // Act
            var impl1 = container.GetInstance<IService<int, string>>();
            var impl2 = container.GetInstance<IService<int, string>>();

            // Assert
            Assert.AreEqual(impl1, impl2, "The type should be returned as singleton.");
        }

        // This is a regression test for work item 21002
        [TestMethod]
        public void RegisterManyForOpenGenericAssembly_RegistersTypeWithTheeImplementations_ResolvesThatTypeAsExpected()
        {
            // Arrange
            var container = new Container();

            // Just registers RequestGroup in three groups.
            container.Register(typeof(IHandler<,>), new[] { typeof(RequestGroup).Assembly }, Lifestyle.Singleton);

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
        public void RegisterManyForOpenGenericTypes_RegistersTypeWithTheeImplementations_ResolvesThatTypeAsExpected()
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

        private static void Assert_AreEqual<T>(List<T> expectedList, List<T> actualList)
        {
            Assert.IsNotNull(actualList);

            Assert.AreEqual(expectedList.Count, actualList.Count);
            
            for (int i = 0; i < expectedList.Count; i++)
            {
                T expected = expectedList[i];
                T actual = actualList[i];

                Assert.AreEqual(expected, actual, "Items at index " + i + " of list were expected to be the same.");
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

        public class MultiInterfaceHandler : ICommandHandler<int>, ICommandHandler<double>
        {
        }

        public class DecimalHandler : ICommandHandler<decimal>
        {
        }

        public class FloatHandler : ICommandHandler<float>
        {
        }

        public class ObjectHandler : ICommandHandler<object>
        {
        }

        public class GenericHandler<T> : ICommandHandler<T>
        {
        }

        public class GenericStructHandler<T> : ICommandHandler<T> where T : struct
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

    public interface IRequest<TResponse>
    {
    }

    public interface IHandler<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
    }

    public class RequestGroup :
        IHandler<Query1, int>,
        IHandler<Query2, double>,
        IHandler<Query3, double>
    {
    }

    public class Query1 : IRequest<int>
    {
    }

    public class Query2 : IRequest<double>
    {
    }

    public class Query3 : IRequest<double>
    {
    }

    public class RequestDecorator<TRequest, TResponse> : IHandler<TRequest, TResponse> where TRequest : IRequest<TResponse>
    {
        public RequestDecorator(IHandler<TRequest, TResponse> decoratee)
        {
            this.Decoratee = decoratee;
        }

        public IHandler<TRequest, TResponse> Decoratee { get; private set; }
    }
    #pragma warning restore 0618
}