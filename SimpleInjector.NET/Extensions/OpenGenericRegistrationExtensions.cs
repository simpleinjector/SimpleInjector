#region Copyright Simple Injector Contributors
/* The Simple Injector is an easy-to-use Inversion of Control library for .NET
 * 
 * Copyright (c) 2013 Simple Injector Contributors
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
    using System.Linq;
    using System.Linq.Expressions;
    using SimpleInjector;
    using SimpleInjector.Advanced;
    using SimpleInjector.Extensions.Decorators;
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
        /// Please see the 
        /// <see cref="OpenGenericRegistrationExtensions.RegisterOpenGeneric(Container,Type,Type,Lifestyle)">RegisterOpenGeneric(Container,Type,Type,Lifestyle)</see>
        /// overload for an example.
        /// </example>
        /// <param name="container">The container to make the registrations in.</param>
        /// <param name="openGenericServiceType">The definition of the open generic service type that can be 
        /// used to retrieve instances.</param>
        /// <param name="openGenericImplementation">The definition of the open generic implementation type
        /// that will be returned when a <paramref name="openGenericServiceType"/> is requested.</param>
        public static void RegisterOpenGeneric(this Container container,
            Type openGenericServiceType, Type openGenericImplementation)
        {
            Requires.IsNotNull(container, "container");

            RegisterOpenGeneric(container, openGenericServiceType, openGenericImplementation,
                container.SelectionBasedLifestyle);
        }

        /// <summary>
        /// Registers that the same instance of <paramref name="openGenericImplementation"/> will be returned 
        /// every time a <paramref name="openGenericServiceType"/> is requested.
        /// </summary>
        /// <example>
        /// Please see the 
        /// <see cref="OpenGenericRegistrationExtensions.RegisterOpenGeneric(Container,Type,Type,Lifestyle)">RegisterOpenGeneric(Container,Type,Type,Lifestyle)</see>
        /// overload for an example.
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

        /// <summary>
        /// Registers that a closed generic instance of the supplied 
        /// <paramref name="openGenericImplementation"/> will be returned when a closed generic version of
        /// the <paramref name="openGenericServiceType"/> is requested. The instance will be cached 
        /// according to the specified <paramref name="lifestyle"/>.
        /// </summary>
        /// <remarks>
        /// Types registered using the <b>RegisterOpenGeneric</b> are resolved using unregistered type
        /// resolution. This means that an explicit registration made for a closed generic version of the
        /// <paramref name="openGenericServiceType"/> always gets resolved first and the given
        /// <paramref name="openGenericImplementation"/> only gets resolved when there is no such registration.
        /// </remarks>
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
        ///     container.Register<IValidator<Order>, OrderValidator>(Lifestyle.Transient);
        ///     container.RegisterOpenGeneric(typeof(IValidator<>), typeof(NullValidator<>), Lifestyle.Singleton);
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
        /// <param name="lifestyle">The lifestyle that defines how returned instances are cached.</param>
        public static void RegisterOpenGeneric(this Container container,
            Type openGenericServiceType, Type openGenericImplementation, Lifestyle lifestyle)
        {
            RegisterOpenGeneric(container, openGenericServiceType, openGenericImplementation,
                lifestyle, c => true);
        }

        /// <summary>
        /// Registers that the same instance of <paramref name="openGenericImplementationType"/> will be returned 
        /// every time a <paramref name="openGenericServiceType"/> is requested.
        /// </summary>
        /// <example>
        /// Please see the 
        /// <see cref="OpenGenericRegistrationExtensions.RegisterOpenGeneric(Container,Type,Type,Lifestyle)">RegisterOpenGeneric(Container,Type,Type,Lifestyle)</see>
        /// overload for an example.
        /// </example>
        /// <param name="container">The container to make the registrations in.</param>
        /// <param name="openGenericServiceType">The definition of the open generic service type that can be 
        /// used to retrieve instances..</param>
        /// <param name="openGenericImplementationType">The definition of the open generic implementation type
        /// that will be returned when a <paramref name="openGenericServiceType"/> is requested.</param>
        /// <param name="lifestyle">The lifestyle that defines how returned instances are cached.</param>
        /// <param name="predicate">The predicate that determines whether the 
        /// <paramref name="openGenericImplementationType"/> can implement the service type.</param>
        public static void RegisterOpenGeneric(this Container container,
            Type openGenericServiceType, Type openGenericImplementationType, Lifestyle lifestyle,
            Predicate<OpenGenericPredicateContext> predicate)
        {
            container.ValidateRegisterOpenGenericRequirements(openGenericServiceType,
                openGenericImplementationType, lifestyle, predicate);

            var resolver = new UnregisteredOpenGenericResolver
            {
                OpenGenericServiceType = openGenericServiceType,
                OpenGenericImplementation = openGenericImplementationType,
                Container = container,
                Lifestyle = lifestyle,
                Predicate = predicate
            };

            container.ResolveUnregisteredType += resolver.ResolveUnregisteredType;
        }

        /// <summary>
        /// Registers that instances of <paramref name="openGenericImplementations"/> will be returned 
        /// when a collection of <paramref name="openGenericServiceType"/> is requested. New instances of 
        /// the registered <paramref name="openGenericImplementations"/> will be returned whenever the
        /// resolved collection is iterated.
        /// </summary>
        /// <example>
        /// Please see the 
        /// <see cref="OpenGenericRegistrationExtensions.RegisterAllOpenGeneric(Container,Type,Lifestyle,IEnumerable{Type})">RegisterAllOpenGeneric(Container,Type,Lifestyle,IEnumerable&lt;Type&gt;)</see>
        /// overload for an example.
        /// </example>
        /// <param name="container">The container to make the registrations in.</param>
        /// <param name="openGenericServiceType">The definition of the open generic service type that can be 
        /// used to retrieve instances.</param>
        /// <param name="openGenericImplementations">The list of open generic implementation types
        /// that will be returned when a collection of <paramref name="openGenericServiceType"/> is requested.
        /// </param>
        public static void RegisterAllOpenGeneric(this Container container,
            Type openGenericServiceType, params Type[] openGenericImplementations)
        {
            RegisterAllOpenGeneric(container, openGenericServiceType,
                (IEnumerable<Type>)openGenericImplementations);
        }

        /// <summary>
        /// Registers that instances of <paramref name="openGenericImplementations"/> will be returned 
        /// when a collection of <paramref name="openGenericServiceType"/> is requested. New instances of 
        /// the registered <paramref name="openGenericImplementations"/> will be returned whenever the
        /// resolved collection is iterated.
        /// </summary>
        /// <example>
        /// Please see the 
        /// <see cref="OpenGenericRegistrationExtensions.RegisterAllOpenGeneric(Container,Type,Lifestyle,IEnumerable{Type})">RegisterAllOpenGeneric(Container,Type,Lifestyle,IEnumerable&lt;Type&gt;)</see>
        /// overload for an example.
        /// </example>
        /// <param name="container">The container to make the registrations in.</param>
        /// <param name="openGenericServiceType">The definition of the open generic service type that can be 
        /// used to retrieve instances.</param>
        /// <param name="openGenericImplementations">The list of open generic implementation types
        /// that will be returned when a collection of <paramref name="openGenericServiceType"/> is requested.
        /// </param>
        public static void RegisterAllOpenGeneric(this Container container,
            Type openGenericServiceType, IEnumerable<Type> openGenericImplementations)
        {
            Requires.IsNotNull(container, "container");

            RegisterAllOpenGeneric(container, openGenericServiceType, container.SelectionBasedLifestyle,
                (IEnumerable<Type>)openGenericImplementations);
        }

        /// <summary>
        /// Registers that instances of <paramref name="openGenericImplementations"/> will be returned 
        /// when a collection of <paramref name="openGenericServiceType"/> is requested. The instances will be 
        /// cached according to the specified <paramref name="lifestyle"/>.
        /// </summary>
        /// <example>
        /// Please see the 
        /// <see cref="OpenGenericRegistrationExtensions.RegisterAllOpenGeneric(Container,Type,Lifestyle,IEnumerable{Type})">RegisterAllOpenGeneric(Container,Type,Lifestyle,IEnumerable&lt;Type&gt;)</see>
        /// overload for an example.
        /// </example>
        /// <param name="container">The container to make the registrations in.</param>
        /// <param name="openGenericServiceType">The definition of the open generic service type that can be 
        /// used to retrieve instances.</param>
        /// <param name="lifestyle">The lifestyle that defines how returned instances are cached.</param>
        /// <param name="openGenericImplementations">The list of open generic implementation types
        /// that will be returned when a collection of <paramref name="openGenericServiceType"/> is requested.
        /// </param>
        public static void RegisterAllOpenGeneric(this Container container,
          Type openGenericServiceType, Lifestyle lifestyle, params Type[] openGenericImplementations)
        {
            RegisterAllOpenGeneric(container, openGenericServiceType, lifestyle,
                (IEnumerable<Type>)openGenericImplementations);
        }

        /// <summary>
        /// Registers that instances of <paramref name="openGenericImplementations"/> will be returned 
        /// when a collection of <paramref name="openGenericServiceType"/> is requested. The instances will be 
        /// cached according to the specified <paramref name="lifestyle"/>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Collections registered using the <b>RegisterAllOpenGeneric</b> are resolved using unregistered type
        /// resolution. This means that an explicit registration made for a collection of the closed generic 
        /// version of the <paramref name="openGenericServiceType"/> always gets resolved first and a 
        /// collection of <paramref name="openGenericImplementations"/> only gets resolved when there is no 
        /// such registration.
        /// </para>
        /// </remarks>
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
        /// public class DefaultValidator<T> : IValidator<T>
        /// {
        ///     public void Validate(T instance)
        ///     {
        ///         // some default validation
        ///     }
        /// }
        /// 
        /// [TestMethod]
        /// public static void TestRegisterAllOpenGeneric()
        /// {
        ///     // Arrange
        ///     var container = new Container();
        ///     
        ///     Type[] types = new[] { typeof(OrderValidator), typeof(DefaultValidator<>) };
        ///     
        ///     container.RegisterManyForOpenGeneric(typeof(IValidator<>),
        ///         (serviceType, implementationTypes) => container.RegisterAll(serviceType, implementationTypes), 
        ///         types);
        ///     
        ///     container.RegisterAllOpenGeneric(typeof(IValidator<>), typeof(DefaultValidator<>));
        ///     
        ///     // Act
        ///     var orderValidators = container.GetAllInstances<IValidator<Order>>();
        ///     var customerValidators = container.GetAllInstances<IValidator<Customer>>();
        /// 
        ///     // Assert
        ///     Assert.IsTrue(orderValidators.SequenceEqual(
        ///         new[] { typeof(OrderValidator), typeof(DefaultValidator<Order>) }));
        ///     
        ///     // Without the call to RegisterAllOpenGeneric this customerValidators would be empty.
        ///     Assert.IsTrue(customerValidators.SequenceEqual(new[] { typeof(DefaultValidator<Customer>) }));
        /// }
        /// ]]></code>
        /// </example>
        /// <param name="container">The container to make the registrations in.</param>
        /// <param name="openGenericServiceType">The definition of the open generic service type that can be 
        /// used to retrieve instances.</param>
        /// <param name="lifestyle">The lifestyle that defines how returned instances are cached.</param>
        /// <param name="openGenericImplementations">The list of open generic implementation types
        /// that will be returned when a collection of <paramref name="openGenericServiceType"/> is requested.
        /// </param>
        public static void RegisterAllOpenGeneric(this Container container,
            Type openGenericServiceType, Lifestyle lifestyle, IEnumerable<Type> openGenericImplementations)
        {
            Requires.IsNotNull(container, "container");
            Requires.IsNotNull(openGenericServiceType, "openGenericServiceType");
            Requires.IsNotNull(lifestyle, "lifestyle");
            Requires.IsNotNull(openGenericImplementations, "openGenericImplementations");

            // Make a copy of the collection for performance and correctness.
            openGenericImplementations = openGenericImplementations.ToArray();

            Requires.CollectionIsNotEmpty(openGenericImplementations, "openGenericImplementations");

            Requires.TypeIsOpenGeneric(openGenericServiceType, "openGenericServiceType");
            Requires.DoesNotContainNullValues(openGenericImplementations, "openGenericImplementations");
            Requires.ServiceIsAssignableFromImplementations(openGenericServiceType, 
                openGenericImplementations, "openGenericImplementations", typeCanBeServiceType: true);

            Requires.ImplementationsAllHaveSelectableConstructor(container, openGenericServiceType,
                openGenericImplementations, "openGenericImplementations");

            var resolver = new UnregisteredAllOpenGenericResolver
            {
                OpenGenericServiceType = openGenericServiceType,
                OpenGenericImplementations = openGenericImplementations,
                Container = container,
                Lifestyle = lifestyle
            };

            container.ResolveUnregisteredType += resolver.ResolveUnregisteredType;
        }
        
        private static void ValidateRegisterOpenGenericRequirements(this Container container,
            Type openGenericServiceType, Type openGenericImplementation, Lifestyle lifestyle,
            Predicate<OpenGenericPredicateContext> predicate)
        {
            Requires.IsNotNull(container, "container");
            Requires.IsNotNull(openGenericServiceType, "openGenericServiceType");
            Requires.IsNotNull(openGenericImplementation, "openGenericImplementation");
            Requires.IsNotNull(lifestyle, "lifestyle");
            Requires.IsNotNull(predicate, "predicate");

            Requires.TypeIsOpenGeneric(openGenericServiceType, "openGenericServiceType");
            Requires.TypeIsOpenGeneric(openGenericImplementation, "openGenericImplementation");
            Requires.ServiceOrItsGenericTypeDefinitionIsAssignableFromImplementation(openGenericServiceType,
                openGenericImplementation, "openGenericServiceType");
            Requires.ImplementationHasSelectableConstructor(container, openGenericServiceType,
                openGenericImplementation, "openGenericImplementation");
            Requires.OpenGenericTypeDoesNotContainUnresolvableTypeArguments(openGenericServiceType,
                openGenericImplementation, "openGenericImplementation");
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

            internal Predicate<OpenGenericPredicateContext> Predicate { get; set; }

            internal void ResolveUnregisteredType(object sender, UnregisteredTypeEventArgs e)
            {
                if (!this.OpenGenericServiceType.IsGenericTypeDefinitionOf(e.UnregisteredServiceType))
                {
                    return;
                }

                var builder = new GenericTypeBuilder(e.UnregisteredServiceType, this.OpenGenericImplementation);

                var result = builder.BuildClosedGenericImplementation();

                if (result.ClosedServiceTypeSatisfiesAllTypeConstraints && 
                    this.ClosedServiceTypeSatisfiesPredicate(e.UnregisteredServiceType, 
                        result.ClosedGenericImplementation, e.Handled))
                {
                    this.RegisterType(e, result.ClosedGenericImplementation);
                }
            }

            private bool ClosedServiceTypeSatisfiesPredicate(Type service, Type implementation, bool handled)
            {
                var context = new OpenGenericPredicateContext(service, implementation, handled);
                return this.Predicate(context);
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
        
        private sealed class UnregisteredAllOpenGenericResolver
        {
            private readonly Dictionary<Type, Registration> lifestyleRegistrationCache =
                new Dictionary<Type, Registration>();

            internal Type OpenGenericServiceType { get; set; }

            internal IEnumerable<Type> OpenGenericImplementations { get; set; }

            internal Container Container { get; set; }

            internal Lifestyle Lifestyle { get; set; }

            internal void ResolveUnregisteredType(object sender, UnregisteredTypeEventArgs e)
            {
                if (typeof(IEnumerable<>).IsGenericTypeDefinitionOf(e.UnregisteredServiceType))
                {
                    Type closedServiceType = e.UnregisteredServiceType.GetGenericArguments().Single();

                    if (this.OpenGenericServiceType.IsGenericTypeDefinitionOf(closedServiceType))
                    {
                        var closedGenericImplementations =
                            this.GetClosedGenericImplementationsFor(closedServiceType);

                        if (closedGenericImplementations.Any())
                        {
                            var registration = this.GetContainerControlledRegistrationFromCache(
                                closedServiceType, closedGenericImplementations);

                            e.Register(registration);
                        }
                    }
                }
            }

            private Type[] GetClosedGenericImplementationsFor(Type closedGenericServiceType)
            {
                return ExtensionHelpers.GetClosedGenericImplementationsFor(closedGenericServiceType,
                    this.OpenGenericImplementations);
            }

            private Registration GetContainerControlledRegistrationFromCache(
                Type closedServiceType, Type[] closedGenericImplementations)
            {
                lock (this.lifestyleRegistrationCache)
                {
                    Registration registration;

                    if (!this.lifestyleRegistrationCache.TryGetValue(closedServiceType, out registration))
                    {
                        registration = this.BuildContainerControlledRegistration(closedServiceType,
                            closedGenericImplementations);

                        this.lifestyleRegistrationCache[closedServiceType] = registration;
                    }

                    return registration;
                }
            }

            private Registration BuildContainerControlledRegistration(Type closedServiceType,
                Type[] closedGenericImplementations)
            {
                var registrations = (
                    from closedGenericImplementation in closedGenericImplementations
                    select this.CreateRegistrationForClosedGenericImplementation(
                        closedServiceType,
                        closedGenericImplementation))
                    .ToArray();

                IContainerControlledCollection collection = 
                    DecoratorHelpers.CreateContainerControlledCollection(closedServiceType, this.Container, 
                        registrations);

                return DecoratorHelpers.CreateRegistrationForContainerControlledCollection(closedServiceType,
                    collection, this.Container);
            }

            private Registration CreateRegistrationForClosedGenericImplementation(Type closedServiceType,
                Type closedGenericImplementation)
            {
                return this.Lifestyle.CreateRegistration(closedServiceType, closedGenericImplementation, 
                    this.Container);
            }
        }
    }
}