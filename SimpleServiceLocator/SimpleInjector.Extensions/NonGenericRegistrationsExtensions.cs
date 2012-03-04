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
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;

    /// <summary>
    /// Extension methods with non-generic method overloads.
    /// </summary>
    public static class NonGenericRegistrationsExtensions
    {
        private static readonly MethodInfo register =
            Helpers.GetGenericMethod(c => c.Register<object, object>());

        private static readonly MethodInfo registerConcrete =
            Helpers.GetGenericMethod(c => c.Register<object>());

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
            Requires.TypeIsReferenceType(serviceType, "serviceType");
            Requires.TypeIsReferenceType(implementation, "implementation");
            Requires.TypeIsNotOpenGeneric(serviceType, "serviceType");
            Requires.TypeIsNotOpenGeneric(serviceType, "implementation");
            Requires.ServiceIsAssignableFromImplementation(serviceType, implementation, "serviceType");
            Requires.ServiceTypeDiffersFromImplementationType(serviceType, implementation, "serviceType",
                "implementation");

            var method = registerSingle.MakeGenericMethod(serviceType, implementation);

            SafeInvoke(serviceType, implementation, () => method.Invoke(container, null));
        }

        /// <summary>
        /// Registers the specified delegate that allows constructing a single <paramref name="serviceType"/> 
        /// instance. The container will call this delegate at most once during the lifetime of the application.
        /// </summary>
        /// <param name="container">The container to make the registrations in.</param>
        /// <param name="serviceType">The base type or interface to register.</param>
        /// <param name="instanceCreator">The delegate that will be used for creating that single instance.</param>
        /// <exception cref="ArgumentException">Thrown when <paramref name="serviceType"/> represents an open
        /// generic type.</exception>
        /// <exception cref="ArgumentNullException">Thrown when either <paramref name="container"/>,
        /// <paramref name="serviceType"/> or <paramref name="instanceCreator"/> are null references (Nothing in
        /// VB).</exception>
        public static void RegisterSingle(this Container container, Type serviceType,
            Func<object> instanceCreator)
        {
            Requires.IsNotNull(container, "container");
            Requires.IsNotNull(serviceType, "serviceType");
            Requires.IsNotNull(instanceCreator, "instanceCreator");
            Requires.TypeIsReferenceType(serviceType, "serviceType");
            Requires.TypeIsNotOpenGeneric(serviceType, "serviceType");

            SafeInvoke(serviceType, "serviceType", () =>
            {
                // Build the following delegate: () => (ServiceType)instanceCreator();
                var typeSafeInstanceCreator = ConvertDelegateToTypeSafeDelegate(serviceType, instanceCreator);

                var method = registerSingleByFunc.MakeGenericMethod(serviceType);

                method.Invoke(container, new[] { typeSafeInstanceCreator });
            });
        }

        /// <summary>
        /// Registers a single instance. This <paramref name="instance"/> must be thread-safe.
        /// </summary>
        /// <param name="container">The container to make the registrations in.</param>
        /// <param name="serviceType">The base type or interface to register.</param>
        /// <param name="instance">The instance to register.</param>
        /// <exception cref="ArgumentNullException">Thrown when either <paramref name="container"/>,
        /// <paramref name="serviceType"/> or <paramref name="instance"/> are null references (Nothing in
        /// VB).</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="instance"/> is
        /// no sub type from <paramref name="serviceType"/>.</exception>
        public static void RegisterSingle(this Container container, Type serviceType, object instance)
        {
            Requires.IsNotNull(container, "container");
            Requires.IsNotNull(serviceType, "serviceType");
            Requires.IsNotNull(instance, "instance");
            Requires.TypeIsReferenceType(serviceType, "serviceType");
            Requires.TypeIsReferenceType(instance.GetType(), "instance");
            Requires.TypeIsNotOpenGeneric(serviceType, "serviceType");
            Requires.ServiceIsAssignableFromImplementation(serviceType, instance.GetType(), "serviceType");

            var method = registerSingleByT.MakeGenericMethod(serviceType);

            SafeInvoke(serviceType, "serviceType", () => method.Invoke(container, new[] { instance }));
        }

        /// <summary>
        /// Registers that a new instance of <paramref name="concreteType"/> will be returned every time it 
        /// is requested (transient).
        /// </summary>
        /// <param name="container">The container to make the registrations in.</param>
        /// <param name="concreteType">The concrete type that will be registered.</param>
        /// <exception cref="ArgumentNullException">Thrown when either <paramref name="container"/> or 
        /// <paramref name="concreteType"/> are null references (Nothing in VB).</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="concreteType"/> represents an 
        /// open generic type or is a type that can not be created by the container.
        /// </exception>
        public static void Register(this Container container, Type concreteType)
        {
            Requires.IsNotNull(container, "container");
            Requires.IsNotNull(concreteType, "concreteType");
            Requires.TypeIsReferenceType(concreteType, "serviceType");
            Requires.TypeIsNotOpenGeneric(concreteType, "concreteType");

            var method = registerConcrete.MakeGenericMethod(concreteType);

            SafeInvoke(concreteType, "concreteType", () => method.Invoke(container, null));
        }

        /// <summary>
        /// Registers that a new instance of <paramref name="implementation"/> will be returned every time a
        /// <paramref name="serviceType"/> is requested.
        /// </summary>
        /// <param name="container">The container to make the registrations in.</param>
        /// <param name="serviceType">The base type or interface to register.</param>
        /// <param name="implementation">The actual type that will be returned when requested.</param>
        /// <exception cref="ArgumentNullException">Thrown when either <paramref name="container"/>,
        /// <paramref name="serviceType"/> or <paramref name="implementation"/> are null references (Nothing in
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
            Requires.TypeIsReferenceType(serviceType, "serviceType");
            Requires.TypeIsReferenceType(implementation, "implementation");
            Requires.TypeIsNotOpenGeneric(serviceType, "serviceType");
            Requires.TypeIsNotOpenGeneric(serviceType, "implementation");
            Requires.ServiceIsAssignableFromImplementation(serviceType, implementation, "serviceType");
            Requires.ServiceTypeDiffersFromImplementationType(serviceType, implementation, "serviceType",
                "implementation");

            var method = register.MakeGenericMethod(serviceType, implementation);

            SafeInvoke(serviceType, "serviceType", () => method.Invoke(container, null));
        }

        /// <summary>
        /// Registers the specified delegate that allows returning instances of <paramref name="serviceType"/>.
        /// </summary>
        /// <param name="container">The container to make the registrations in.</param>
        /// <param name="serviceType">The base type or interface to register.</param>
        /// <param name="instanceCreator">The delegate that will be used for creating new instances.</param>
        /// <exception cref="ArgumentNullException">Thrown when either <paramref name="container"/>,
        /// <paramref name="serviceType"/> or <paramref name="instanceCreator"/> are null references (Nothing in
        /// VB).</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="serviceType"/> represents an
        /// open generic type.</exception>
        public static void Register(this Container container, Type serviceType, Func<object> instanceCreator)
        {
            Requires.IsNotNull(container, "container");
            Requires.IsNotNull(serviceType, "serviceType");
            Requires.IsNotNull(instanceCreator, "instanceCreator");
            Requires.TypeIsReferenceType(serviceType, "serviceType");
            Requires.TypeIsNotOpenGeneric(serviceType, "serviceType");

            SafeInvoke(serviceType, "serviceType", () =>
            {
                // Build the following delegate: () => (ServiceType)instanceCreator();
                var typeSafeInstanceCreator = ConvertDelegateToTypeSafeDelegate(serviceType, instanceCreator);

                var method = registerByFunc.MakeGenericMethod(serviceType);

                method.Invoke(container, new[] { typeSafeInstanceCreator });
            });
        }

        /// <summary>
        /// Registers an collection of <paramref name="serviceTypes"/>, which instances will be resolved when
        /// enumerating the set returned when a collection of <typeparamref name="TService"/> objects is 
        /// requested. On enumeration the container is called for each type in the list.
        /// </summary>
        /// <typeparam name="TService">The base type or interface for elements in the collection.</typeparam>
        /// <param name="container">The container to make the registrations in.</param>
        /// <param name="serviceTypes">The collection of <see cref="Type"/> objects whose instances
        /// will be requested from the container.</param>
        /// <exception cref="ArgumentNullException">Thrown when either <paramref name="container"/> or 
        /// <paramref name="serviceTypes"/> are null references (Nothing in VB).
        /// </exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="serviceTypes"/> contains a null
        /// (Nothing in VB) element, a generic type definition, or the <typeparamref name="TService"/> is
        /// not assignable from one of the given <paramref name="serviceTypes"/> elements.
        /// </exception>
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter",
            Justification = "A method without the type parameter already exists. This extension method " +
                "is more intuitive to developers.")]
        public static void RegisterAll<TService>(this Container container, params Type[] serviceTypes)
        {
            RegisterAll(container, typeof(TService), serviceTypes);
        }

        /// <summary>
        /// Registers a collection of instances of <paramref name="serviceTypes"/> to be returned when
        /// a collection of <typeparamref name="TService"/> objects is requested.
        /// </summary>
        /// <typeparam name="TService">The base type or interface for elements in the collection.</typeparam>
        /// <param name="container">The container to make the registrations in.</param>
        /// <param name="serviceTypes">The collection of <see cref="Type"/> objects whose instances
        /// will be requested from the container.</param>
        /// <exception cref="ArgumentNullException">Thrown when either <paramref name="container"/> or 
        /// <paramref name="serviceTypes"/> are null references (Nothing in VB).
        /// </exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="serviceTypes"/> contains a null
        /// (Nothing in VB) element, a generic type definition, or the <typeparamref name="TService"/> is
        /// not assignable from one of the given <paramref name="serviceTypes"/> elements.
        /// </exception>
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter",
            Justification = "A method without the type parameter already exists. This extension method " +
                "is more intuitive to developers.")]
        public static void RegisterAll<TService>(this Container container, IEnumerable<Type> serviceTypes)
        {
            RegisterAll(container, typeof(TService), serviceTypes);
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
        /// <paramref name="serviceType"/>, or <paramref name="serviceTypes"/> are null references
        /// (Nothing in VB).
        /// </exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="serviceTypes"/> contains a null
        /// (Nothing in VB) element, a generic type definition, or the <paramref name="serviceType"/> is
        /// not assignable from one of the given <paramref name="serviceTypes"/> elements.
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

            SafeInvoke(serviceType, "serviceType", () => method.Invoke(container, new[] { castedCollection }));
        }

        private static void SafeInvoke(Type serviceType, Type implementation, Action action)
        {
            try
            {
                action();
            }
            catch (MemberAccessException ex)
            {
                // This happens when the user tries to resolve an internal type inside a (Silverlight) sandbox.
                ThrowUnableToResolveTypeDueToSecurityConfigurationException(serviceType, implementation, ex);
            }
            catch (TargetInvocationException ex)
            {
                if (ex.InnerException != null)
                {
                    throw ex.InnerException;
                }

                throw;
            }
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

        private static void SafeInvoke(Type serviceType, string paramName, Action action)
        {
            try
            {
                action();
            }
            catch (MemberAccessException ex)
            {
                // This happens when the user tries to resolve an internal type inside a (Silverlight) sandbox.
                ThrowUnableToResolveTypeDueToSecurityConfigurationException(serviceType, ex, paramName);
            }
            catch (TargetInvocationException ex)
            {
                if (ex.InnerException != null)
                {
                    throw ex.InnerException;
                }

                throw;
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