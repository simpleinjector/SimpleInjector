#region Copyright (c) 2013 Simple Injector Contributors
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

namespace SimpleInjector
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading;
    using SimpleInjector.Advanced;
    using SimpleInjector.Diagnostics;
    
    /// <summary>
    /// The container. Create an instance of this type for registration of dependencies.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Thread-safety:</b>
    /// Resolving instances can be done safely from multiple threads concurrently, but registration needs to
    /// be done from one single thread.
    /// </para>
    /// <para> 
    /// It is therefore safe to call <see cref="GetInstance"/>, <see cref="GetAllInstances"/>, 
    /// <see cref="IServiceProvider.GetService">GetService</see>, <see cref="GetRegistration(Type)"/> and
    /// <see cref="GetCurrentRegistrations()"/> and anything related to resolving instances from multiple thread 
    /// concurrently. It is however <b>unsafe</b> to call
    /// <see cref="Register{TService, TImplementation}(Lifestyle)">RegisterXXX</see>,
    /// <see cref="ExpressionBuilding"/>, <see cref="ExpressionBuilt"/>, <see cref="ResolveUnregisteredType"/>,
    /// <see cref="AddRegistration"/> or anything related to registering from multiple threads concurrently.
    /// </para>
    /// </remarks>
    public partial class Container
    {
        private static long counter;

        private readonly object locker = new object();
        private readonly List<IInstanceInitializer> instanceInitializers = new List<IInstanceInitializer>();
        private readonly IDictionary items = new Dictionary<object, object>();
        private readonly long containerId;

        // This list contains all instance producers that not yet have been explicitly registered in the container.
        private readonly ConditionalHashSet<InstanceProducer> externalProducers = 
            new ConditionalHashSet<InstanceProducer>();

        private Dictionary<Type, InstanceProducer> registrations = 
            new Dictionary<Type, InstanceProducer>(40, ReferenceEqualityComparer<Type>.Instance);

        private Dictionary<Type, PropertyInjector> propertyInjectorCache = new Dictionary<Type, PropertyInjector>();

        // Flag to signal that the container can't be altered by using any of the Register methods.
        private bool locked;

        // Flag to signal that the container's configuration is currently being verified.
        private bool verifying;

        // Flag to signal that the container's configuration has been verified (at least once).
        private bool succesfullyVerified;

        private EventHandler<UnregisteredTypeEventArgs> resolveUnregisteredType;
        private EventHandler<ExpressionBuildingEventArgs> expressionBuilding;

        private EventHandler<ExpressionBuiltEventArgs> expressionBuilt;

        /// <summary>Initializes a new instance of the <see cref="Container"/> class.</summary>
        public Container()
            : this(new ContainerOptions())
        {
        }

        /// <summary>Initializes a new instance of the <see cref="Container"/> class.</summary>
        /// <param name="options">The container options.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="options"/> is a null
        /// reference.</exception>
        /// <exception cref="ArgumentException">Thrown when supplied <paramref name="options"/> is an instance
        /// that already is supplied to another <see cref="Container"/> instance. Every container must get
        /// its own <see cref="ContainerOptions"/> instance.</exception>
        public Container(ContainerOptions options)
        {
            Requires.IsNotNull(options, "options");

            if (options.Container != null)
            {
                throw new ArgumentException(StringResources.ContainerOptionsBelongsToAnotherContainer(),
                    "options");
            }
            
            options.Container = this;
            this.Options = options;

            this.RegisterSingle<Container>(this);

            this.containerId = Interlocked.Increment(ref counter);

            this.OnCreated();
        }

        // Wrapper for instance initializer delegates
        private interface IInstanceInitializer
        {
            bool AppliesTo(Type implementationType, InitializationContext context);

            Action<T> CreateAction<T>(InitializationContext context);
        }

        /// <summary>Gets the container options.</summary>
        /// <value>The <see cref="ContainerOptions"/> instance for this container.</value>
        public ContainerOptions Options { get; private set; }

        internal object SyncRoot
        {
            get { return this.locker; }
        }

        internal long ContainerId
        {
            get { return this.containerId; }
        }

        internal bool IsLocked
        {
            get
            {
                // By using a lock, we have the certainty that all threads will see the new value for 'locked'
                // immediately.
                lock (this.locker)
                {
                    return this.locked;
                }
            }
        }

        internal bool HasRegistrations
        {
            get { return this.registrations.Count > 1; }
        }

        internal bool IsVerifying
        {
            get
            {
                // By using a lock, we have the certainty that all threads will see the new value for 
                // 'verifying' immediately.
                lock (this.locker)
                {
                    return this.verifying;
                }
            }

            private set
            {
                lock (this.locker)
                {
                    this.verifying = value;
                }
            }
        }

        internal bool SuccesfullyVerified
        {
            get { return this.succesfullyVerified; }
        }

        /// <summary>
        /// Returns an array with the current registrations. This list contains all explicitly registered
        /// types, and all implictly registered instances. Implicit registrations are  all concrete 
        /// unregistered types that have been requested, all types that have been resolved using
        /// unregistered type resolution (using the <see cref="ResolveUnregisteredType"/> event), and
        /// requested unregistered collections. Note that the result of this method may change over time, 
        /// because of these implicit registrations.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This method has a performance caracteristic of O(n). Prevent from calling this in a performance
        /// critical path of the application.
        /// </para>
        /// <para>
        /// <b>Note:</b> This method is <i>not</i> guaranteed to always return the same 
        /// <see cref="InstanceProducer"/> instance for a given registration. It will however either 
        /// always return a producer that is able to return the expected instance. Because of this, do not 
        /// compare sets of instances returned by different calls to <see cref="GetCurrentRegistrations()"/> 
        /// by reference. The way of comparing lists is by the actual type. The type of each instance is 
        /// guaranteed to be unique in the returned list.
        /// </para>
        /// </remarks>
        /// <returns>An array of <see cref="InstanceProducer"/> instances.</returns>
        public InstanceProducer[] GetCurrentRegistrations()
        {
            return this.GetCurrentRegistrations(includeInvalidContainerRegisteredTypes: false);
        }

        /// <summary>Determines whether the specified System.Object is equal to the current System.Object.
        /// </summary>
        /// <param name="obj">The System.Object to compare with the current System.Object.</param>
        /// <returns>
        /// True if the specified System.Object is equal to the current System.Object; otherwise, false.
        /// </returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        /// <summary>Returns the hash code of the current instance.</summary>
        /// <returns>The hash code of the current instance.</returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents the <see cref="Container"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents the <see cref="Container"/>.
        /// </returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override string ToString()
        {
            return base.ToString();
        }

        /// <summary>Gets the <see cref="System.Type"/> of the current instance.</summary>
        /// <returns>The <see cref="System.Type"/> instance that represents the exact runtime 
        /// type of the current instance.</returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = @"
            This FxCop warning is valid, but this method is used to be able to attach an 
            EditorBrowsableAttribute to the GetType method, which will hide the method when the user browses 
            the methods of the Container class with IntelliSense. The GetType method has no value for the user
            who will only use this class for registration.")]
        public new Type GetType()
        {
            return base.GetType();
        }

        internal InstanceProducer[] GetCurrentRegistrations(bool includeInvalidContainerRegisteredTypes,
            bool includeExternalProducers = true)
        {
            IEnumerable<InstanceProducer> registrations = this.registrations.Values;

            if (includeExternalProducers)
            {
                registrations = registrations.Concat(this.externalProducers.Keys);
            }

            // Filter out the invalid registrations (see the IsValid property for more information).
            return (
                from registration in registrations
                where registration != null
                where includeInvalidContainerRegisteredTypes || registration.IsValid
                select registration)
                .ToArray();
        }

        internal object GetItem(object key)
        {
            Requires.IsNotNull(key, "key");

            lock (this.items)
            {
                return this.items[key];
            }
        }

        internal void SetItem(object key, object item)
        {
            Requires.IsNotNull(key, "key");

            lock (this.items)
            {
                if (item == null)
                {
                    this.items.Remove(key);
                }
                else
                {
                    this.items[key] = item;
                }
            }
        }

        internal Expression OnExpressionBuilding(Registration registration, Type serviceType, 
            Type implementationType, Expression instanceCreatorExpression)
        {
            if (this.expressionBuilding != null)
            {
                var e = new ExpressionBuildingEventArgs(serviceType, implementationType,
                    instanceCreatorExpression, registration.Lifestyle);

                var relationships = new KnownRelationshipCollection(registration.GetRelationships().ToList());

                e.KnownRelationships = relationships;

                this.expressionBuilding(this, e);

                // Optimization.
                if (relationships.HasChanged)
                {
                    registration.ReplaceRelationships(e.KnownRelationships);
                }

                return e.Expression;
            }

            return instanceCreatorExpression;
        }

        internal void OnExpressionBuilt(ExpressionBuiltEventArgs e, InstanceProducer instanceProducer)
        {
            if (this.expressionBuilt != null)
            {
                var relationships = 
                    new KnownRelationshipCollection(instanceProducer.GetRelationships().ToList());

                e.KnownRelationships = relationships;

                this.expressionBuilt(this, e);

                if (relationships.HasChanged)
                {
                    instanceProducer.ReplaceRelationships(e.KnownRelationships);
                }
            }
        }
        
        /// <summary>Prevents any new registrations to be made to the container.</summary>
        internal void LockContainer()
        {
            if (!this.locked)
            {
                // By using a lock, we have the certainty that all threads will see the new value for 'locked'
                // immediately, since ThrowWhenContainerIsLocked also locks on 'locker'.
                lock (this.locker)
                {
                    this.locked = true;
                }
            }
        }

        partial void OnCreated();

        private sealed class TypedInstanceInitializer : IInstanceInitializer
        {
            private Type serviceType;
            private object instanceInitializer;

            public bool AppliesTo(Type implementationType, InitializationContext context)
            {
                var typeHierarchy = Helpers.GetTypeHierarchyFor(implementationType);

                return typeHierarchy.Contains(this.serviceType);
            }

            public Action<T> CreateAction<T>(InitializationContext context)
            {
                return Helpers.CreateAction<T>(this.instanceInitializer);
            }

            internal static IInstanceInitializer Create<TImplementation>(
                Action<TImplementation> instanceInitializer)
            {
                return new TypedInstanceInitializer
                {
                    serviceType = typeof(TImplementation),
                    instanceInitializer = instanceInitializer
                };
            }
        }

        private sealed class ContextualInstanceInitializer : IInstanceInitializer
        {
            private Predicate<InitializationContext> predicate;
            private Action<InstanceInitializationData> instanceInitializer;

            public bool AppliesTo(Type implementationType, InitializationContext context)
            {
                if (context == null)
                {
                    // LEGACY: AdvancedExtensions.GetInitializer passes in a null context. We have to support
                    // that.
                    return false;
                }

                return this.predicate(context);
            }

            public Action<T> CreateAction<T>(InitializationContext context)
            {
                return instance =>
                {
                    this.instanceInitializer(new InstanceInitializationData(context, instance));
                };
            }

            internal static IInstanceInitializer Create(
                Action<InstanceInitializationData> instanceInitializer,
                Predicate<InitializationContext> predicate)
            {
                return new ContextualInstanceInitializer
                {
                    instanceInitializer = instanceInitializer,
                    predicate = predicate,
                };
            }
        }
    }
}