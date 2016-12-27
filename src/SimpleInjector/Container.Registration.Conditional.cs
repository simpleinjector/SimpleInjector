#region Copyright Simple Injector Contributors
/* The Simple Injector is an easy-to-use Inversion of Control library for .NET
 * 
 * Copyright (c) 2015 Simple Injector Contributors
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

namespace SimpleInjector
{
    using System;
    using System.Reflection;

#if !PUBLISH
    /// <summary>Methods for conditional registrations.</summary>
    /// <design>
    /// These conditional registration methods lack a Func{PredicateContext, TService} predicate
    /// method. This is deliberate, because would force the factory to be registered as transient, forcing
    /// the whole parent structure to become transient as well. Besides this, it would blind the diagnostic
    /// system, because it will stop at the delegate, instead of being able to analyze the object graph as
    /// a whole.
    /// </design>
#endif
    public partial class Container
    {
        /// <summary>
        /// Conditionally registers that a new instance of <typeparamref name="TImplementation"/> will be 
        /// returned every time a <typeparamref name="TService"/> is requested (transient) and where the
        /// supplied <paramref name="predicate"/> returns true. The predicate will only be evaluated a finite
        /// number of times; the predicate is unsuited for making decisions based on runtime conditions.
        /// </summary>
        /// <remarks>
        /// This method uses the container's 
        /// <see cref="ContainerOptions.LifestyleSelectionBehavior">LifestyleSelectionBehavior</see> to select
        /// the exact lifestyle for the specified type. By default this will be 
        /// <see cref="Lifestyle.Transient">Transient</see>.
        /// </remarks>
        /// <typeparam name="TService">The interface or base type that can be used to retrieve the instances.</typeparam>
        /// <typeparam name="TImplementation">The concrete type that will be registered.</typeparam>
        /// <param name="predicate">The predicate that determines whether the <typeparamref name="TImplementation"/> 
        /// can be applied for the requested service type. This predicate
        /// can be used to build a fallback mechanism where multiple registrations for the same service type
        /// are made.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when one of the arguments is a null reference (Nothing in VB).
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when this container instance is locked and can not be altered.
        /// </exception>
        public void RegisterConditional<TService, TImplementation>(Predicate<PredicateContext> predicate)
            where TImplementation : class, TService
            where TService : class
        {
            this.RegisterConditional<TService, TImplementation>(this.SelectionBasedLifestyle, predicate);
        }

        /// <summary>
        /// Conditionally registers that an instance of <typeparamref name="TImplementation"/> will be 
        /// returned every time a <typeparamref name="TService"/> is requested and where the supplied 
        /// <paramref name="predicate"/> returns true. The instance is cached according to the supplied 
        /// <paramref name="lifestyle"/>. The predicate will only be evaluated a finite number of times; the 
        /// predicate is unsuited for making decisions based on runtime conditions.
        /// </summary>
        /// <typeparam name="TService">The interface or base type that can be used to retrieve the instances.</typeparam>
        /// <typeparam name="TImplementation">The concrete type that will be registered.</typeparam>
        /// <param name="lifestyle">The lifestyle that specifies how the returned instance will be cached.</param>
        /// <param name="predicate">The predicate that determines whether the 
        /// <typeparamref name="TImplementation"/> can be applied for the requested service type. This predicate
        /// can be used to build a fallback mechanism where multiple registrations for the same service type
        /// are made.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when one of the arguments is a null reference (Nothing in VB).
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when this container instance is locked and can not be altered.
        /// </exception>
        public void RegisterConditional<TService, TImplementation>(Lifestyle lifestyle, 
            Predicate<PredicateContext> predicate)
            where TImplementation : class, TService
            where TService : class
        {
            this.RegisterConditional(typeof(TService), typeof(TImplementation), lifestyle, predicate);
        }

        /// <summary>
        /// Conditionally registers that a new instance of <paramref name="implementationType"/> will be 
        /// returned every time a <paramref name="serviceType"/> is requested (transient) and where the
        /// supplied <paramref name="predicate"/> returns true. The predicate will only be evaluated a finite
        /// number of times; the predicate is unsuited for making decisions based on runtime conditions.
        /// </summary>
        /// <remarks>
        /// This method uses the container's 
        /// <see cref="ContainerOptions.LifestyleSelectionBehavior">LifestyleSelectionBehavior</see> to select
        /// the exact lifestyle for the specified type. By default this will be 
        /// <see cref="Lifestyle.Transient">Transient</see>.
        /// </remarks>
        /// <param name="serviceType">The base type or interface to register. This can be an open-generic type.</param>
        /// <param name="implementationType">The actual type that will be returned when requested.</param>
        /// <param name="predicate">The predicate that determines whether the 
        /// <paramref name="implementationType"/> can be applied for the requested service type. This predicate
        /// can be used to build a fallback mechanism where multiple registrations for the same service type
        /// are made.</param>
        /// <exception cref="ArgumentNullException">Thrown when one of the arguments is a null reference
        /// (Nothing in VB).</exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when this container instance is locked and can not be altered.
        /// </exception>
        public void RegisterConditional(Type serviceType, Type implementationType, Predicate<PredicateContext> predicate)
        {
            this.RegisterConditional(serviceType, implementationType, this.SelectionBasedLifestyle, predicate);
        }

        /// <summary>
        /// Conditionally registers that an instance of <paramref name="implementationType"/> will be 
        /// returned every time a <paramref name="serviceType"/> is requested and where the supplied 
        /// <paramref name="predicate"/> returns true. The instance is cached according to the supplied 
        /// <paramref name="lifestyle"/>. The predicate will only be evaluated a finite number of times; the 
        /// predicate is unsuited for making decisions based on runtime conditions.
        /// </summary>
        /// <param name="serviceType">The base type or interface to register. This can be an open-generic type.</param>
        /// <param name="implementationType">The actual type that will be returned when requested.</param>
        /// <param name="lifestyle">The lifestyle that defines how returned instances are cached.</param>
        /// <param name="predicate">The predicate that determines whether the 
        /// <paramref name="implementationType"/> can be applied for the requested service type. This predicate
        /// can be used to build a fallback mechanism where multiple registrations for the same service type
        /// are made.</param>
        /// <exception cref="ArgumentNullException">Thrown when one of the arguments is a null reference
        /// (Nothing in VB).</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="serviceType"/> and 
        /// <paramref name="implementationType"/> are not a generic type or when <paramref name="serviceType"/>
        /// is a partially-closed generic type.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when this container instance is locked and can not be altered.
        /// </exception>
        public void RegisterConditional(Type serviceType, Type implementationType, Lifestyle lifestyle,
            Predicate<PredicateContext> predicate)
        {
            Requires.IsNotNull(serviceType, nameof(serviceType));
            Requires.IsNotNull(implementationType, nameof(implementationType));
            Requires.IsNotNull(lifestyle, nameof(lifestyle));
            Requires.IsNotNull(predicate, nameof(predicate));
            Requires.IsNotPartiallyClosed(serviceType, nameof(serviceType), nameof(implementationType));

            Requires.ServiceOrItsGenericTypeDefinitionIsAssignableFromImplementation(serviceType, implementationType, nameof(serviceType));
            Requires.ImplementationHasSelectableConstructor(this, implementationType, nameof(implementationType));
            Requires.OpenGenericTypeDoesNotContainUnresolvableTypeArguments(serviceType, implementationType, nameof(implementationType));

            if (serviceType.ContainsGenericParameters())
            {
                this.RegisterOpenGeneric(serviceType, implementationType, lifestyle, predicate);
            }
            else
            {
                var registration = lifestyle.CreateRegistration(implementationType, this);
                this.RegisterConditional(serviceType, registration, predicate);
            }
        }

        /// <summary>
        /// Conditionally registers that an instance of the type returned from 
        /// <paramref name="implementationTypeFactory"/> will be returned every time a 
        /// <paramref name="serviceType"/> is requested and where the supplied <paramref name="predicate"/> 
        /// returns true. The instance is cached according to the supplied 
        /// <paramref name="lifestyle"/>. Both the <paramref name="predicate"/> and 
        /// <paramref name="implementationTypeFactory"/> will only be evaluated a finite number of times; 
        /// they unsuited for making decisions based on runtime conditions.
        /// </summary>
        /// <param name="serviceType">The base type or interface to register. This can be an open-generic type.</param>
        /// <param name="implementationTypeFactory">A factory that allows building Type objects that define the
        /// implementation type to inject, based on the given contextual information. The delegate is allowed 
        /// to return (partially) open-generic types.</param>
        /// <param name="lifestyle">The lifestyle that defines how returned instances are cached.</param>
        /// <param name="predicate">The predicate that determines whether the registration can be applied for
        /// the requested service type. This predicate can be used to build a fallback mechanism where 
        /// multiple registrations for the same service type are made.</param>
        /// <exception cref="ArgumentNullException">Thrown when one of the arguments is a null reference
        /// (Nothing in VB).</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="serviceType"/> is a 
        /// partially-closed generic type.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when this container instance is locked and can not be altered.
        /// </exception>
        public void RegisterConditional(
            Type serviceType, 
            Func<TypeFactoryContext, Type> implementationTypeFactory,
            Lifestyle lifestyle,
            Predicate<PredicateContext> predicate)
        {
            Requires.IsNotNull(serviceType, nameof(serviceType));
            Requires.IsNotNull(implementationTypeFactory, nameof(implementationTypeFactory));
            Requires.IsNotNull(lifestyle, nameof(lifestyle));
            Requires.IsNotNull(predicate, nameof(predicate));
            Requires.IsNotPartiallyClosed(serviceType, nameof(serviceType));
            
            this.GetOrCreateRegistrationalEntry(serviceType)
                .Add(serviceType, implementationTypeFactory, lifestyle, predicate);
        }

        /// <summary>
        /// Conditionally registers that <paramref name="registration"/> will be used every time a 
        /// <paramref name="serviceType"/> is requested and where the supplied <paramref name="predicate"/> 
        /// returns true. The predicate will only be evaluated a finite number of times; the predicate is 
        /// unsuited for making decisions based on runtime conditions.
        /// </summary>
        /// <param name="serviceType">The base type or interface to register. This can be an open-generic type.</param>
        /// <param name="registration">The <see cref="Registration"/> instance to register.</param>
        /// <param name="predicate">The predicate that determines whether the 
        /// <paramref name="registration"/> can be applied for the requested service type. This predicate
        /// can be used to build a fallback mechanism where multiple registrations for the same service type
        /// are made.</param>
        /// <exception cref="ArgumentNullException">Thrown when one of the arguments is a null reference
        /// (Nothing in VB).</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="serviceType"/> is open generic or
        /// <paramref name="registration" /> is not assignable to <paramref name="serviceType"/>.</exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when this container instance is locked and can not be altered.
        /// </exception>
        public void RegisterConditional(Type serviceType, Registration registration, 
            Predicate<PredicateContext> predicate)
        {
            Requires.IsNotNull(serviceType, nameof(serviceType));
            Requires.IsNotNull(registration, nameof(registration));
            Requires.IsNotNull(predicate, nameof(predicate));
            Requires.IsNotOpenGenericType(serviceType, nameof(serviceType));
            Requires.ServiceIsAssignableFromImplementation(serviceType, registration.ImplementationType,
                nameof(serviceType));

            this.ThrowWhenContainerIsLockedOrDisposed();
            
            var producer = new InstanceProducer(serviceType, registration, predicate);

            this.AddInstanceProducer(producer);
        }
    }
}