namespace SimpleInjector.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;

    using SimpleInjector;

    /// <summary>
    /// Provides a set of static (Shared in Visual Basic) methods for registration of open generic service
    /// types in the <see cref="Container"/>.
    /// </summary>
    public static class OpenGenericRegistrationExtensions
    {
        /// <summary>
        /// Registers that a new instance of <paramref name="openGenericImplementation"/> will be returned 
        /// every time a <paramref name="openGenericServiceType"/> is requested.
        /// </summary>
        /// <example>
        /// The following example shows the definition of a generic <code>IValidator&lt;T&gt;</code> interface
        /// and, a <code>NullValidator&lt;T&gt;</code> implementation and a specific validator for Orders.
        /// The registration ensures a <code>OrderValidator</code> is returned when a 
        /// <code>IValidator&lt;Order&gt;</code> is requested. For all requests for a 
        /// <code>IValidator&lt;T&gt;</code> other than a <code>IValidator&lt;Order&gt;</code>, an 
        /// implementation of <code>NullValidator&lt;T&gt;</code> will be returned.
        /// <code lang="cs"><![CDATA[
        /// public interface IValidator<T>
        /// { 
        ///     void Validate(T instance);
        /// }
        /// 
        /// public class NullValidator<T> : IValidator<T>
        /// {
        ///     public void Validate(T instance)
        ///     {
        ///     }
        /// }
        /// 
        /// public class OrderValidator : IValidator<Order>
        /// {
        ///     public void Validate(Order instance)
        ///     {
        ///         if (instance.Total < 0)
        ///         {
        ///             throw new ValidationException("Total can not be negative.");
        ///         }
        ///     }
        /// }
        /// 
        /// [TestMethod]
        /// public static void TestRegisterInitializer()
        /// {
        ///     // Arrange
        ///     var container = new Container();
        ///     
        ///     container.Register<IValidator<Order>, OrderValidator>();
        ///     container.RegisterOpenGeneric(typeof(IValidator<>), typeof(NullValidator<>));
        ///     
        ///     // Act
        ///     var orderValidator = container.GetInstance<IValidator<Order>>();
        ///     var customerValidator = container.GetInstance<IValidator<Customer>>();
        ///     var productValidator = container.GetInstance<IValidator<Product>>();
        /// 
        ///     // Assert
        ///     Assert.IsInstanceOfType(orderValidator, typeof(OrderValidator));
        ///     Assert.IsInstanceOfType(customerValidator, typeof(NullValidator<Customer>));
        ///     Assert.IsInstanceOfType(productValidator, typeof(NullValidator<Product>));
        /// }
        /// ]]></code>
        /// </example>
        /// <param name="container">The container to make the registrations in.</param>
        /// <param name="openGenericServiceType">The definition of the open generic service type that can be 
        /// used to retrieve instances..</param>
        /// <param name="openGenericImplementation">The definition of the open generic implementation type
        /// that will be returned when a <typeparamref name="openGenericServiceType"/> is requested.</param>
        public static void RegisterOpenGeneric(this Container container,
            Type openGenericServiceType, Type openGenericImplementation)
        {
            Requires.TypeIsOpenGeneric(openGenericServiceType, "openGenericServiceType");
            Requires.TypeIsOpenGeneric(openGenericImplementation, "openGenericImplementation");
            Requires.ServiceIsAssignableFromImplementation(openGenericServiceType, openGenericImplementation,
                "openGenericServiceType");

            container.ResolveUnregisteredType += (s, e) =>
            {
                if (e.UnregisteredServiceType.IsGenericType &&
                    e.UnregisteredServiceType.GetGenericTypeDefinition() == openGenericServiceType)
                {
                    var closedGenericImplementation = openGenericImplementation.MakeGenericType(
                        e.UnregisteredServiceType.GetGenericArguments());

                    Func<object> instanceCreator = () => container.GetInstance(closedGenericImplementation);

                    e.Register(instanceCreator);
                }
            };
        }

        /// <summary>
        /// Registers that the same instance of <paramref name="openGenericImplementation"/> will be returned 
        /// every time a <paramref name="openGenericServiceType"/> is requested.
        /// </summary>
        /// <example>
        /// The following example shows the definition of a generic <code>IValidator&lt;T&gt;</code> interface
        /// and, a <code>NullValidator&lt;T&gt;</code> implementation and a specific validator for Orders.
        /// The registration ensures a <code>OrderValidator</code> is returned when a 
        /// <code>IValidator&lt;Order&gt;</code> is requested. For all requests for a 
        /// <code>IValidator&lt;T&gt;</code> other than a <code>IValidator&lt;Order&gt;</code>, an 
        /// implementation of <code>NullValidator&lt;T&gt;</code> will be returned.
        /// <code lang="cs"><![CDATA[
        /// public interface IValidator<T>
        /// { 
        ///     void Validate(T instance);
        /// }
        /// 
        /// public class NullValidator<T> : IValidator<T>
        /// {
        ///     public void Validate(T instance)
        ///     {
        ///     }
        /// }
        /// 
        /// public class OrderValidator : IValidator<Order>
        /// {
        ///     public void Validate(Order instance)
        ///     {
        ///         if (instance.Total < 0)
        ///         {
        ///             throw new ValidationException("Total can not be negative.");
        ///         }
        ///     }
        /// }
        /// 
        /// [TestMethod]
        /// public static void TestRegisterInitializer()
        /// {
        ///     // Arrange
        ///     var container = new Container();
        ///     
        ///     container.RegisterSingle<IValidator<Order>, OrderValidator>();
        ///     container.RegisterSingleOpenGeneric(typeof(IValidator<>), typeof(NullValidator<>));
        ///     
        ///     // Act
        ///     var orderValidator = container.GetInstance<IValidator<Order>>();
        ///     var customerValidator = container.GetInstance<IValidator<Customer>>();
        ///     var productValidator = container.GetInstance<IValidator<Product>>();
        /// 
        ///     // Assert
        ///     Assert.IsInstanceOfType(orderValidator, typeof(OrderValidator));
        ///     Assert.IsInstanceOfType(customerValidator, typeof(NullValidator<Customer>));
        ///     Assert.IsInstanceOfType(productValidator, typeof(NullValidator<Product>));
        /// }
        /// ]]></code>
        /// </example>
        /// <param name="container">The container to make the registrations in.</param>
        /// <param name="openGenericServiceType">The definition of the open generic service type that can be 
        /// used to retrieve instances..</param>
        /// <param name="openGenericImplementation">The definition of the open generic implementation type
        /// that will be returned when a <typeparamref name="openGenericServiceType"/> is requested.</param>
        public static void RegisterSingleOpenGeneric(this Container container,
            Type openGenericServiceType, Type openGenericImplementation)
        {
            Requires.TypeIsOpenGeneric(openGenericServiceType, "openGenericServiceType");
            Requires.TypeIsOpenGeneric(openGenericImplementation, "openGenericImplementation");
            Requires.ServiceIsAssignableFromImplementation(openGenericServiceType, openGenericImplementation,
                "openGenericServiceType");
            
            object locker = new object();
            Dictionary<Type, object> singletons = new Dictionary<Type, object>();

            container.ResolveUnregisteredType += (s, e) =>
            {
                if (e.UnregisteredServiceType.IsGenericType &&
                    e.UnregisteredServiceType.GetGenericTypeDefinition() == openGenericServiceType)
                {
                    var closedGenericImplementation = openGenericImplementation.MakeGenericType(
                        e.UnregisteredServiceType.GetGenericArguments());

                    object singleton;

                    lock (locker)
                    {
                        if (!singletons.TryGetValue(closedGenericImplementation, out singleton))
                        {
                            singleton = container.GetInstance(closedGenericImplementation);
                            singletons[closedGenericImplementation] = singleton;
                        }
                    }

                    e.Register(() => singleton);
                }
            };
        }
    }
}