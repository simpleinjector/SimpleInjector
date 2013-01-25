#region Copyright (c) 2010 S. van Deursen
/* The Simple Injector is an easy-to-use Inversion of Control library for .NET
 * 
 * Copyright (C) 2010 S. van Deursen
 * 
 * To contact me, please visit my blog at http://www.cuttingedge.it/blogs/steven/ or mail to steven at 
 * cuttingedge.it.
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this software and 
 * associated documentation files (the "Software"), to deal in the Software without restriction, including 
 * without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell 
 * copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the 
 * following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all copies or substantial 
 * portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT 
 * LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO 
 * EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER 
 * IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE 
 * USE OR OTHER DEALINGS IN THE SOFTWARE.
*/
#endregion

namespace SimpleInjector.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq.Expressions;
    using SimpleInjector;
    using SimpleInjector.Lifestyles;

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
        /// The following example shows the definition of a generic <b>IValidator&lt;T&gt;</b> interface
        /// and, a <b>NullValidator&lt;T&gt;</b> implementation and a specific validator for Orders.
        /// The registration ensures a <b>OrderValidator</b> is returned when a 
        /// <b>IValidator&lt;Order&gt;</b> is requested. For all requests for a 
        /// <b>IValidator&lt;T&gt;</b> other than a <b>IValidator&lt;Order&gt;</b>, an 
        /// implementation of <b>NullValidator&lt;T&gt;</b> will be returned.
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
        /// public static void TestRegisterOpenGeneric()
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
        /// used to retrieve instances.</param>
        /// <param name="openGenericImplementation">The definition of the open generic implementation type
        /// that will be returned when a <paramref name="openGenericServiceType"/> is requested.</param>
        public static void RegisterOpenGeneric(this Container container,
            Type openGenericServiceType, Type openGenericImplementation)
        {
            RegisterOpenGeneric(container, openGenericServiceType, openGenericImplementation,
                Lifestyle.Transient);
        }

        /// <summary>
        /// Registers that the same instance of <paramref name="openGenericImplementation"/> will be returned 
        /// every time a <paramref name="openGenericServiceType"/> is requested.
        /// </summary>
        /// <example>
        /// The following example shows the definition of a generic <b>IValidator&lt;T&gt;</b> interface
        /// and, a <b>NullValidator&lt;T&gt;</b> implementation and a specific validator for Orders.
        /// The registration ensures a <b>OrderValidator</b> is returned when a 
        /// <b>IValidator&lt;Order&gt;</b> is requested. For all requests for a 
        /// <b>IValidator&lt;T&gt;</b> other than a <b>IValidator&lt;Order&gt;</b>, an 
        /// implementation of <b>NullValidator&lt;T&gt;</b> will be returned.
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
        /// public static void TestRegisterOpenGeneric()
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
        /// that will be returned when a <paramref name="openGenericServiceType"/> is requested.</param>
        public static void RegisterSingleOpenGeneric(this Container container,
            Type openGenericServiceType, Type openGenericImplementation)
        {
            RegisterOpenGeneric(container, openGenericServiceType, openGenericImplementation,
                Lifestyle.Singleton);
        }

        public static void RegisterOpenGeneric(this Container container,
            Type openGenericServiceType, Type openGenericImplementation, Lifestyle lifestyle)
        {
            Requires.IsNotNull(container, "container");
            Requires.IsNotNull(openGenericServiceType, "openGenericServiceType");
            Requires.IsNotNull(openGenericImplementation, "openGenericImplementation");
            Requires.IsNotNull(lifestyle, "lifestyle");
            Requires.TypeIsOpenGeneric(openGenericServiceType, "openGenericServiceType");
            Requires.TypeIsOpenGeneric(openGenericImplementation, "openGenericImplementation");
            Requires.ServiceIsAssignableFromImplementation(openGenericServiceType, openGenericImplementation,
                "openGenericServiceType");
            Requires.ImplementationHasSelectableConstructor(container, openGenericServiceType,
                openGenericImplementation, "openGenericImplementation");

            var resolver = new UnregisteredOpenGenericResolver
            {
                OpenGenericServiceType = openGenericServiceType,
                OpenGenericImplementation = openGenericImplementation,
                Container = container,
                Lifestyle = lifestyle
            };

            container.ResolveUnregisteredType += resolver.ResolveUnregisteredType;
        }

        /// <summary>Resolves a given open generic type.</summary>
        private sealed class UnregisteredOpenGenericResolver
        {
            private readonly Dictionary<Type, Registration> lifestyleRegistrationCache =
                new Dictionary<Type, Registration>();

            internal Type OpenGenericServiceType { get; set; }

            internal Type OpenGenericImplementation { get; set; }

            internal Container Container { get; set; }

            internal Lifestyle Lifestyle { get; set; }

            internal void ResolveUnregisteredType(object sender, UnregisteredTypeEventArgs e)
            {
                if (!this.OpenGenericServiceType.IsGenericTypeDefinitionOf(e.UnregisteredServiceType))
                {
                    return;
                }

                var builder = new GenericTypeBuilder(e.UnregisteredServiceType, this.OpenGenericImplementation);

                var results = builder.BuildClosedGenericImplementation();

                if (results.ClosedServiceTypeSatisfiesAllTypeConstraints)
                {
                    this.RegisterType(e, results.ClosedGenericImplementation);
                }
            }

            private void RegisterType(UnregisteredTypeEventArgs e, Type closedGenericImplementation)
            {
                var registration =
                    this.GetRegistrationFromCache(e.UnregisteredServiceType, closedGenericImplementation);

                this.ThrowWhenExpressionCanNotBeBuilt(registration, closedGenericImplementation);

                e.Register(registration);
            }

            private void ThrowWhenExpressionCanNotBeBuilt(Registration registration, Type implementationType)
            {
                try
                {
                    // The core library will also throw a quite expressive exception if we don't do it here,
                    // but we can do better and explain that the type is registered as open generic type
                    // (instead of it being registered using open generic batch registration).
                    registration.BuildExpression();
                }
                catch (Exception ex)
                {
                    throw new ActivationException(StringResources.ErrorInRegisterOpenGenericRegistration(
                        this.OpenGenericServiceType, implementationType, ex.Message), ex);
                }
            }

            private Registration GetRegistrationFromCache(Type serviceType, Type implementationType)
            {
                // We must cache the returned lifestyles to prevent any multi-threading issues in case the
                // returned lifestyle does some caching internally (as the singleton lifestyle does).
                lock (this.lifestyleRegistrationCache)
                {
                    Registration registration;

                    if (!this.lifestyleRegistrationCache.TryGetValue(serviceType, out registration))
                    {
                        registration = this.GetRegistration(serviceType, implementationType);

                        this.lifestyleRegistrationCache[serviceType] = registration;
                    }

                    return registration;
                }
            }

            private Registration GetRegistration(Type serviceType, Type implementationType)
            {
                try
                {
                    return this.Lifestyle.CreateRegistration(serviceType, implementationType, this.Container);
                }
                catch (ArgumentException ex)
                {
                    throw new ActivationException(ex.Message);
                }
            }
        }
    }
}