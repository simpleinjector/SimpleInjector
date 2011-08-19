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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace SimpleInjector.Extensions
{
    /// <summary>
    /// Extension methods with non-generic method overloads.
    /// </summary>
    public static class NonGenericRegistrationsExtensions
    {
        private static readonly MethodInfo register = 
            Helpers.GetGenericMethod(c => c.Register<object, object>());

        private static readonly MethodInfo registerSingle = 
            Helpers.GetGenericMethod(c => c.RegisterSingle<object, object>());

        private static readonly MethodInfo registerByFunc = 
            Helpers.GetGenericMethod(c => c.Register<object>((Func<object>)null));

        private static readonly MethodInfo registerAll = 
            Helpers.GetGenericMethod(c => c.RegisterAll<object>((IEnumerable<object>)null));

        private static readonly MethodInfo registerSingleByFunc =
            Helpers.GetGenericMethod(c => c.RegisterSingle<object>((Func<object>)null));

        private static readonly MethodInfo registerSingleByT = 
            Helpers.GetGenericMethod(c => c.RegisterSingle<object>((object)null));

        /// <summary>
        /// Registers that the same instance of type <paramref name="implementation"/> will be returned every 
        /// time a <paramref name="serviceType"/> type is requested.
        /// </summary>
        /// <param name="container">The container to make the registrations in.</param>
        /// <param name="serviceType">The base type or interface to register.</param>
        /// <param name="implementation">The actual type that will be returned when requested.</param>
        /// <exception cref="ArgumentNullException">Thrown when either <paramref name="container"/>,
        /// <paramref name="serviceType"/> or <paramref name="implementation"/> are null references (Nothing in
        /// VB).</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="serviceType"/> and 
        /// <paramref name="implementation"/> represent the same type, or <paramref name="implementation"/> is
        /// no sub type from <paramref name="serviceType"/>, or when one of them represents an open generic
        /// type.</exception>
        public static void RegisterSingle(this Container container, Type serviceType, Type implementation)
        {
            Requires.IsNotNull(container, "container");
            Requires.IsNotNull(serviceType, "serviceType");
            Requires.IsNotNull(implementation, "implementation");
            Requires.TypeIsNotOpenGeneric(serviceType, "serviceType");
            Requires.TypeIsNotOpenGeneric(serviceType, "implementation");
            Requires.ServiceIsAssignableFromImplementation(serviceType, implementation, "serviceType");
            Requires.ServiceTypeDiffersFromImplementationType(serviceType, implementation, "serviceType",
                "implementation");

            var method = registerSingle.MakeGenericMethod(serviceType, implementation);

            try
            {
                method.Invoke(container, null);
            }
            catch (MemberAccessException ex)
            {
                // This happens when the user tries to resolve an internal type inside a (Silverlight) sandbox.
                ThrowUnableToResolveTypeDueToSecurityConfigurationException(serviceType, implementation, ex);
            }
        }

        /// <summary>
        /// Registers the specified delegate that allows constructing a single instance of 
        /// <typeparamref name="TService"/>. This delegate will be called at most once during the lifetime of 
        /// the application.
        /// </summary>
        /// <param name="container">The container to make the registrations in.</param>
        /// <param name="serviceType">The base type or interface to register.</param>
        /// <param name="instanceCreator">The delegate that will be used for creating that single instance.</param>
        /// <exception cref="ArgumentException">Thrown when <paramref name="serviceType"/> represents an open
        /// generic type.</exception>
        /// <exception cref="ArgumentNullException">Thrown when either <paramref name="container"/>,
        /// <paramref name="serviceType"/> or <paramref name="instanceCreator"/> or null references (Nothing in
        /// VB).</exception>
        public static void RegisterSingle(this Container container, Type serviceType,
            Func<object> instanceCreator)
        {
            Requires.IsNotNull(container, "container");
            Requires.IsNotNull(serviceType, "serviceType");
            Requires.IsNotNull(instanceCreator, "instanceCreator");
            Requires.TypeIsNotOpenGeneric(serviceType, "serviceType");
            
            try
            {
                // Build the following delegate: () => (ServiceType)instanceCreator();
                var typeSafeInstanceCreator = ConvertDelegateToTypeSafeDelegate(serviceType, instanceCreator);

                var method = registerSingleByFunc.MakeGenericMethod(serviceType);

                method.Invoke(container, new[] { typeSafeInstanceCreator });
            }
            catch (MemberAccessException ex)
            {
                // This happens when the user tries to resolve an internal type inside a (Silverlight) sandbox.
                ThrowUnableToResolveTypeDueToSecurityConfigurationException(serviceType, ex, "serviceType");
            }
        }

        /// <summary>
        /// Registers a single instance. This <paramref name="instance"/> must be thread-safe.
        /// </summary>
        /// <param name="container">The container to make the registrations in.</param>
        /// <param name="serviceType">The base type or interface to register.</param>
        /// <param name="instance">The instance to register.</param>
        /// <exception cref="ArgumentNullException">Thrown when either <paramref name="container"/>,
        /// <paramref name="serviceType"/> or <paramref name="instance"/> or null references (Nothing in
        /// VB).</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="instance"/> is
        /// no sub type from <paramref name="serviceType"/>.</exception>
        public static void RegisterSingle(this Container container, Type serviceType, object instance)
        {
            Requires.IsNotNull(container, "container");
            Requires.IsNotNull(serviceType, "serviceType");
            Requires.IsNotNull(instance, "instance");
            Requires.TypeIsNotOpenGeneric(serviceType, "serviceType");
            Requires.ServiceIsAssignableFromImplementation(serviceType, instance.GetType(), "serviceType");

            var method = registerSingleByT.MakeGenericMethod(serviceType);

            try
            {
                method.Invoke(container, new[] { instance });
            }
            catch (MemberAccessException ex)
            {
                // This happens when the user tries to resolve an internal type inside a (Silverlight) sandbox.
                ThrowUnableToResolveTypeDueToSecurityConfigurationException(serviceType, ex, "serviceType");
            }
        }

        /// <summary>
        /// Registers that a new instance of <paramref name="implementation"/> will be returned every time a
        /// <paramref name="serviceType"/> is requested.
        /// </summary>
        /// <param name="container">The container to make the registrations in.</param>
        /// <param name="serviceType">The base type or interface to register.</param>
        /// <param name="implementation">The actual type that will be returned when requested.</param>
        /// <exception cref="ArgumentNullException">Thrown when either <paramref name="container"/>,
        /// <paramref name="serviceType"/> or <paramref name="implementation"/> or null references (Nothing in
        /// VB).</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="serviceType"/> and 
        /// <paramref name="implementation"/> represent the same type, or <paramref name="implementation"/> is
        /// no sub type from <paramref name="serviceType"/>, or one of them represents an open generic type.
        /// </exception>
        public static void Register(this Container container, Type serviceType, Type implementation)
        {
            Requires.IsNotNull(container, "container");
            Requires.IsNotNull(serviceType, "serviceType");
            Requires.IsNotNull(implementation, "implementation");
            Requires.TypeIsNotOpenGeneric(serviceType, "serviceType");
            Requires.TypeIsNotOpenGeneric(serviceType, "implementation");
            Requires.ServiceIsAssignableFromImplementation(serviceType, implementation, "serviceType");
            Requires.ServiceTypeDiffersFromImplementationType(serviceType, implementation, "serviceType",
                "implementation");

            var method = register.MakeGenericMethod(serviceType, implementation);

            try
            {
                method.Invoke(container, null);
            }
            catch (MemberAccessException ex)
            {
                // This happens when the user tries to resolve an internal type inside a (Silverlight) sandbox.
                ThrowUnableToResolveTypeDueToSecurityConfigurationException(serviceType, ex, "serviceType");
            }
        }

        /// <summary>
        /// Registers the specified delegate that allows returning instances of <paramref name="serviceType"/>.
        /// </summary>
        /// <param name="container">The container to make the registrations in.</param>
        /// <param name="serviceType">The base type or interface to register.</param>
        /// <param name="instanceCreator">The delegate that will be used for creating new instances.</param>
        /// <exception cref="ArgumentNullException">Thrown when either <paramref name="container"/>,
        /// <paramref name="serviceType"/> or <paramref name="instanceCreator"/> or null references (Nothing in
        /// VB).</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="serviceType"/> represents an
        /// open generic type.</exception>
        public static void Register(this Container container, Type serviceType, Func<object> instanceCreator)
        {
            Requires.IsNotNull(container, "container");
            Requires.IsNotNull(serviceType, "serviceType");
            Requires.IsNotNull(instanceCreator, "instanceCreator");
            Requires.TypeIsNotOpenGeneric(serviceType, "serviceType");
            
            try
            {
                // Build the following delegate: () => (ServiceType)instanceCreator();
                var typeSafeInstanceCreator = ConvertDelegateToTypeSafeDelegate(serviceType, instanceCreator);

                var method = registerByFunc.MakeGenericMethod(serviceType);

                method.Invoke(container, new[] { typeSafeInstanceCreator });
            }
            catch (MemberAccessException ex)
            {
                // This happens when the user tries to resolve an internal type inside a (Silverlight) sandbox.
                ThrowUnableToResolveTypeDueToSecurityConfigurationException(serviceType, ex, "serviceType");
            }
        }

        /// <summary>
        /// Registers an collection of <paramref name="serviceTypes"/>, which instances will be resolved when
        /// enumerating the set returned when a collection of <paramref name="serviceType"/> objects is 
        /// requested. On enumeration the container is called for each type in the list.
        /// </summary>
        /// <typeparam name="TService">The base type or interface for elements in the collection.</typeparam>
        /// <param name="container">The container to make the registrations in.</param>
        /// <param name="serviceTypes">The collection of <see cref="Type"/> objects whose instances
        /// will be requested from the container.</param>
        /// <exception cref="ArgumentNullException">Thrown when either <paramref name="container"/>,
        /// <paramref name="serviceType"/>, or <paramref name="implementationTypes"/> are null references
        /// (Nothing in VB).
        /// </exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="typesToRegister"/> contains a null
        /// (Nothing in VB) element.
        /// </exception>
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter",
            Justification = "A method without the type parameter already exists. This extension method " +
                "is more intuitive to developers.")]
        public static void RegisterAll<TService>(this Container container, params Type[] serviceTypes)
        {
            RegisterAll(container, typeof(TService), serviceTypes);
        }

        /// <summary>
        /// Registers a collection of instances of <paramref name="implementationTypes"/> to be returned when
        /// a collection of <paramref name="serviceType"/> objects is requested.
        /// </summary>
        /// <typeparam name="TService">The base type or interface for elements in the collection.</typeparam>
        /// <param name="container">The container to make the registrations in.</param>
        /// <param name="implementationTypes">The collection of <see cref="Type"/> objects whose instances
        /// will be requested from the container.</param>
        /// <exception cref="ArgumentNullException">Thrown when either <paramref name="container"/>,
        /// <paramref name="serviceType"/>, or <paramref name="implementationTypes"/> are null references
        /// (Nothing in VB).
        /// </exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="serviceTypes"/> contains a null
        /// (Nothing in VB) element, when the <paramref name="openGenericServiceType"/> is not an open generic
        /// type, or one of the types supplied in <paramref name="typesToRegister"/> does not implement a 
        /// closed version of <paramref name="openGenericServiceType"/>.
        /// </exception>
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter",
            Justification = "A method without the type parameter already exists. This extension method " +
                "is more intuitive to developers.")]
        public static void RegisterAll<TService>(this Container container, 
            IEnumerable<Type> implementationTypes)
        {
            RegisterAll(container, typeof(TService), implementationTypes);
        }

        /// <summary>
        /// Registers an collection of <paramref name="serviceTypes"/>, which instances will be resolved when
        /// enumerating the set returned when a collection of <paramref name="serviceType"/> objects is 
        /// requested. On enumeration the container is called for each type in the list.
        /// </summary>
        /// <param name="container">The container to make the registrations in.</param>
        /// <param name="serviceType">The base type or interface for elements in the collection.</param>
        /// <param name="serviceTypes">The collection of <see cref="Type"/> objects whose instances
        /// will be requested from the container.</param>
        /// <exception cref="ArgumentNullException">Thrown when either <paramref name="container"/>,
        /// <paramref name="serviceType"/>, or <paramref name="implementationTypes"/> are null references
        /// (Nothing in VB).
        /// </exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="serviceTypes"/> contains a null
        /// (Nothing in VB) element, when the <paramref name="openGenericServiceType"/> is not an open generic
        /// type, or one of the types supplied in <paramref name="typesToRegister"/> does not implement a 
        /// closed version of <paramref name="openGenericServiceType"/>.
        /// </exception>
        public static void RegisterAll(this Container container, Type serviceType,
            params Type[] serviceTypes)
        {
            RegisterAll(container, serviceType, (IEnumerable<Type>)serviceTypes);
        }

        /// <summary>
        /// Registers an collection of <paramref name="serviceTypes"/>, which instances will be resolved when
        /// enumerating the set returned when a collection of <paramref name="serviceType"/> objects is 
        /// requested. On enumeration the container is called for each type in the list.
        /// </summary>
        /// <param name="container">The container to make the registrations in.</param>
        /// <param name="serviceType">The base type or interface for elements in the collection.</param>
        /// <param name="serviceTypes">The collection of <see cref="Type"/> objects whose instances
        /// will be requested from the container.</param>
        /// <exception cref="ArgumentNullException">Thrown when either <paramref name="container"/>,
        /// <paramref name="serviceType"/>, or <paramref name="implementationTypes"/> are null references
        /// (Nothing in VB).
        /// </exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="serviceTypes"/> contains a null
        /// (Nothing in VB) element, when the <paramref name="openGenericServiceType"/> is not an open generic
        /// type, or one of the types supplied in <paramref name="typesToRegister"/> does not implement a 
        /// closed version of <paramref name="openGenericServiceType"/>.
        /// </exception>
        public static void RegisterAll(this Container container, Type serviceType,
            IEnumerable<Type> serviceTypes)
        {
            // Make a copy for correctness and performance.
            serviceTypes = serviceTypes != null ? serviceTypes.ToArray() : null;

            Requires.IsNotNull(container, "container");
            Requires.IsNotNull(serviceType, "serviceType");
            Requires.IsNotNull(serviceTypes, "serviceTypes");
            Requires.DoesNotContainNullValues(serviceTypes, "serviceTypes");
            Requires.DoesNotContainOpenGenericTypes(serviceTypes, "serviceTypes");
            Requires.ServiceIsAssignableFromImplementations(serviceType, serviceTypes, "serviceTypes");

            IEnumerable<object> instances = new AllIterator(container, serviceTypes);

            RegisterAll(container, serviceType, instances);
        }

        /// <summary>
        /// Registers a <paramref name="collection"/> of elements of type <paramref name="serviceType"/>.
        /// </summary>
        /// <param name="container">The container to make the registrations in.</param>
        /// <param name="serviceType">The base type or interface for elements in the collection.</param>
        /// <param name="collection">The collection of items to register.</param>
        /// <exception cref="ArgumentNullException">Thrown when either <paramref name="container"/>,
        /// <paramref name="serviceType"/> or <paramref name="collection"/> are null references (Nothing in
        /// VB).</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="serviceType"/> represents an
        /// open generic type.</exception>
        public static void RegisterAll(this Container container, Type serviceType, IEnumerable collection)
        {
            Requires.IsNotNull(container, "container");
            Requires.IsNotNull(serviceType, "serviceType");
            Requires.IsNotNull(collection, "collection");
            Requires.TypeIsNotOpenGeneric(serviceType, "serviceType");

            RegisterAllInternal(container, serviceType, collection);
        }

        private static void RegisterAllInternal(Container container, Type serviceType, IEnumerable collection)
        {
            object castedCollection;

            if (typeof(IEnumerable<>).MakeGenericType(serviceType).IsAssignableFrom(collection.GetType()))
            {
                // The collection is a IEnumerable<[ServiceType]>. We can simply cast it. 
                // Better for performance
                castedCollection = collection;
            }
            else
            {
                // The collection is not a IEnumerable<[ServiceType]>. We wrap it in a 
                // CastEnumerator<[ServiceType]> to be able to supply it to the RegisterAll<T> method.
                var castMethod = typeof(Enumerable).GetMethod("Cast").MakeGenericMethod(serviceType);

                castedCollection = castMethod.Invoke(null, new[] { collection });
            }

            var method = registerAll.MakeGenericMethod(serviceType);

            try
            {
                method.Invoke(container, new[] { castedCollection });
            }
            catch (MemberAccessException ex)
            {
                // This happens when the user tries to resolve an internal type inside a (Silverlight) sandbox.
                ThrowUnableToResolveTypeDueToSecurityConfigurationException(serviceType, ex, "serviceType");
            }
        }

        // Gets called when the user tries to resolve an internal type inside a (Silverlight) sandbox.
        private static void ThrowUnableToResolveTypeDueToSecurityConfigurationException(Type serviceType,
            MemberAccessException innerException, string paramName)
        {
            string exceptionMessage =
                StringResources.UnableToResolveTypeDueToSecurityConfiguration(serviceType, innerException);

#if SILVERLIGHT
            throw new ArgumentException(exceptionMessage, paramName);
#else
            throw new ArgumentException(exceptionMessage, paramName, innerException);
#endif
        }

        private static void ThrowUnableToResolveTypeDueToSecurityConfigurationException(Type serviceType,
            Type implementation, MemberAccessException innerException)
        {
            if (!serviceType.IsPublic)
            {
                ThrowUnableToResolveTypeDueToSecurityConfigurationException(serviceType, innerException,
                     "serviceType");
            }
            else
            {
                ThrowUnableToResolveTypeDueToSecurityConfigurationException(implementation, innerException,
                     "implementation");
            }
        }

        private static object ConvertDelegateToTypeSafeDelegate(Type serviceType, Func<object> instanceCreator)
        {
            // Build the following delegate: () => (ServiceType)instanceCreator();
            var invocationExpression =
                Expression.Invoke(Expression.Constant(instanceCreator), new Expression[0]);

            var convertExpression = Expression.Convert(invocationExpression, serviceType);

            var parameters = new ParameterExpression[0];

            // This might throw an MemberAccessException when serviceType is internal while we're running in
            // a Silverlight sandbox.
            return Expression.Lambda(convertExpression, parameters).Compile();
        }

        /// <summary>Allows iterating a set of services.</summary>
        private sealed class AllIterator : IEnumerable<object>
        {
            private readonly Container container;
            private readonly IEnumerable<Type> serviceTypes;

            private IInstanceProducer[] instanceProducers;

            internal AllIterator(Container container, IEnumerable<Type> serviceTypes)
            {
                this.container = container;
                this.serviceTypes = serviceTypes;
            }

            /// <summary>Returns an enumerator that iterates through the collection.</summary>
            /// <returns>A IEnumerator that can be used to iterate through the collection.</returns>
            public IEnumerator<object> GetEnumerator()
            {
                if (this.instanceProducers == null)
                {
                    this.instanceProducers = this.serviceTypes.Select(t => this.GetRegistration(t)).ToArray();
                }

                return this.GetIterator();
            }

            /// <summary>Returns an enumerator that iterates through a collection.</summary>
            /// <returns>An IEnumerator object that can be used to iterate through the collection.</returns>
            IEnumerator IEnumerable.GetEnumerator()
            {
                return this.GetEnumerator();
            }

            private IEnumerator<object> GetIterator()
            {
                var producers = this.instanceProducers;

                for (int i = 0; i < producers.Length; i++)
                {
                    yield return producers[i].GetInstance();
                }
            }

            private IInstanceProducer GetRegistration(Type serviceType)
            {
                var producer = this.container.GetRegistration(serviceType);

                if (producer == null)
                {
                    // This will throw an exception, because there is no registration for the service type.
                    // By calling GetInstnce we reuse the descriptive exception messages of the container.
                    this.container.GetInstance(serviceType);
                }

                return producer;
            }
        }
    }
}