// Copyright (c) Simple Injector Contributors. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace SimpleInjector
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Linq.Expressions;
    using System.Reflection;
    using SimpleInjector.Advanced;

    /// <summary>
    /// Delegate that allows intercepting calls to <see cref="Container.GetInstance"/> and
    /// <see cref="InstanceProducer.GetInstance"/>.
    /// </summary>
    /// <param name="context">Contextual information about the to be created object.</param>
    /// <param name="instanceProducer">A delegate that produces the actual instance according to its
    /// lifestyle settings.</param>
    /// <returns>The instance that is returned from <paramref name="instanceProducer"/> or an intercepted instance.</returns>
    public delegate object ResolveInterceptor(InitializationContext context, Func<object> instanceProducer);

    /// <summary>Configuration options for the <see cref="SimpleInjector.Container">Container</see>.</summary>
    /// <example>
    /// The following example shows the typical usage of the <b>ContainerOptions</b> class.
    /// <code lang="cs"><![CDATA[
    /// var container = new Container();
    ///
    /// container.Register<ITimeProvider, DefaultTimeProvider>();
    ///
    /// // Use of ContainerOptions class here.
    /// container.Options.AllowOverridingRegistrations = true;
    ///
    /// // Replaces the previous registration of ITimeProvider
    /// container.Register<ITimeProvider, CustomTimeProvider>();
    /// ]]></code>
    /// </example>
    [DebuggerDisplay("{" + nameof(ContainerOptions.DebuggerDisplayDescription) + ", nq}")]
    public class ContainerOptions : ApiObject
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private EventHandler<ContainerLockingEventArgs>? containerLocking;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private IConstructorResolutionBehavior resolutionBehavior;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private IDependencyInjectionBehavior injectionBehavior;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private IPropertySelectionBehavior propertyBehavior;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private ILifestyleSelectionBehavior lifestyleBehavior;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private IExpressionCompilationBehavior compilationBehavior;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private Lifestyle defaultLifestyle = Lifestyle.Transient;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private ScopedLifestyle? defaultScopedLifestyle;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private bool resolveUnregisteredConcreteTypes;

        internal ContainerOptions(Container container)
        {
            Requires.IsNotNull(container, nameof(container));

            this.Container = container;
            this.resolutionBehavior = new DefaultConstructorResolutionBehavior();
            this.injectionBehavior = new DefaultDependencyInjectionBehavior(container);
            this.propertyBehavior = new DefaultPropertySelectionBehavior();
            this.lifestyleBehavior = new DefaultLifestyleSelectionBehavior(this);
            this.compilationBehavior = new DefaultExpressionCompilationBehavior();
        }

        /// <summary>
        /// Occurs just before the container is about to be locked, giving the developer a last change to
        /// interact and change the unlocked container before it is sealed for further modifications. Locking
        /// typically occurs by a call to <b>Container.GetInstance</b>, <b>Container.Verify</b>, or any other
        /// method that causes the construction and resolution of registered instances.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The <b>ContainerLocking</b> event is called exactly once by the container, allowing a developer to
        /// register types, hook unregistered type resolution events that need to be applied last, or see
        /// who is responsible for locking the container.
        /// </para>
        /// <para>
        /// A registered event handler delegate is allowed to make a call that locks the container, e.g.
        /// calling <b>Container.GetInstance</b>; this will not cause any new <b>ContainerLocking</b> event to
        /// be raised. Doing so, however, is not advised, as that might cause any following executed handlers
        /// to break, in case they require an unlocked container.
        /// </para>
        /// </remarks>
        public event EventHandler<ContainerLockingEventArgs> ContainerLocking
        {
            add
            {
                this.Container.ThrowWhenContainerIsLockedOrDisposed();

                this.containerLocking += value;
            }

            remove
            {
                this.Container.ThrowWhenContainerIsLockedOrDisposed();

                this.containerLocking -= value;
            }
        }

        /// <summary>
        /// Gets the container to which this <b>ContainerOptions</b> instance belongs to.
        /// </summary>
        /// <value>The current <see cref="SimpleInjector.Container">Container</see>.</value>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public Container Container { get; }

        /// <summary>
        /// Gets or sets a value indicating whether the container allows overriding registrations. The default
        /// is false.
        /// </summary>
        /// <value>The value indicating whether the container allows overriding registrations.</value>
        public bool AllowOverridingRegistrations { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the container should suppress checking for lifestyle
        /// mismatches (see: https://simpleinjector.org/dialm) when a component is resolved. The default
        /// is false. This setting will have no effect when <see cref="EnableAutoVerification"/> is true.
        /// </summary>
        /// <value>The value indicating whether the container should suppress checking for lifestyle
        /// mismatches.</value>
        public bool SuppressLifestyleMismatchVerification { get; set; }

        /// <summary>
        /// <para>
        /// Gets or sets a value indicating whether the container should use a loosened (i.e. less strict)
        /// behavior for detecting lifestyle mismatches (see: https://simpleinjector.org/dialm). In short,
        /// when <see cref="UseLoosenedLifestyleMismatchBehavior"/> is set to <b>true</b>
        /// <see cref="Lifestyle.Transient"/> dependencies are allowed to be injected into
        /// <see cref="Lifestyle.Scoped"/> components. When disabled, a warning would be given in that case.
        /// The default value is <b>true</b>.
        /// </para>
        /// <para>
        /// Simple Injector allows custom lifestyles to be created and this loosened behavior works on custom
        /// lifestyles as well. The loosened behavior will ignore any lifestyle mismatch checks on any
        /// component with a lifestyle that has a <see cref="Lifestyle.Length"/> that is equal or shorter than
        /// the length of <see cref="Lifestyle.Scoped"/>.
        /// </para>
        /// </summary>
        /// <value>
        /// The value indicating whether the container uses loosened or strict behavior when validating
        /// mismatches on  <see cref="Lifestyle.Scoped"/> components.
        /// </value>
        public bool UseLoosenedLifestyleMismatchBehavior { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether the container should automatically trigger verification
        /// and diagnostics of its configuration when the first service is resolved (e.g. the first call to
        /// GetInstance). The behavior is identical to calling <see cref="Container.Verify()">Verify()</see>
        /// manually. The default is false.
        /// </summary>
        /// <value>The value indicating whether the container should automatically trigger verification.</value>
        public bool EnableAutoVerification { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether all the containers in the current AppDomain should throw
        /// exceptions that contain fully qualified type name. The default is <c>false</c> which means
        /// the type's namespace is omitted.
        /// </summary>
        /// <value>The value indicating whether exception message should emit full type names.</value>
        public bool UseFullyQualifiedTypeNames
        {
            get { return StringResources.UseFullyQualifiedTypeNames; }
            set { StringResources.UseFullyQualifiedTypeNames = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the container should resolve unregistered concrete types.
        /// The default value is <c>true</c>. Consider changing the value to <c>false</c> to prevent
        /// accidental creation of types you haven't registered explicitly.
        /// </summary>
        /// <value>The value indicating whether the container should resolve unregistered concrete types.</value>
        /// <exception cref="InvalidOperationException">
        /// Thrown when this container instance is locked and can not be altered.
        /// </exception>
        public bool ResolveUnregisteredConcreteTypes
        {
            get
            {
                return this.resolveUnregisteredConcreteTypes;
            }

            set
            {
                this.Container.ThrowWhenContainerIsLockedOrDisposed();

                this.resolveUnregisteredConcreteTypes = value;
            }
        }

        /// <summary>
        /// Gets or sets the constructor resolution behavior. By default, the container only supports types
        /// that have a single public constructor.
        /// </summary>
        /// <value>The constructor resolution behavior.</value>
        /// <exception cref="NullReferenceException">Thrown when the supplied value is a null reference.</exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the container already contains registrations.
        /// </exception>
        public IConstructorResolutionBehavior ConstructorResolutionBehavior
        {
            get
            {
                return this.resolutionBehavior;
            }

            set
            {
                Requires.IsNotNull(value, nameof(value));

                this.ThrowWhenContainerHasRegistrations(nameof(this.ConstructorResolutionBehavior));

                this.resolutionBehavior = value;
            }
        }

        /// <summary>Gets or sets the dependency injection behavior.</summary>
        /// <value>The constructor injection behavior.</value>
        /// <exception cref="NullReferenceException">Thrown when the supplied value is a null reference.</exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the container already contains registrations.
        /// </exception>
        public IDependencyInjectionBehavior DependencyInjectionBehavior
        {
            get
            {
                return this.injectionBehavior;
            }

            set
            {
                Requires.IsNotNull(value, nameof(value));

                this.ThrowWhenContainerHasRegistrations(nameof(this.DependencyInjectionBehavior));

                this.injectionBehavior = value;
            }
        }

        /// <summary>
        /// Gets or sets the property selection behavior. The container's default behavior is to do no
        /// property injection.
        /// </summary>
        /// <value>The property selection behavior.</value>
        /// <exception cref="NullReferenceException">Thrown when the supplied value is a null reference.</exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the container already contains registrations.
        /// </exception>
        public IPropertySelectionBehavior PropertySelectionBehavior
        {
            get
            {
                return this.propertyBehavior;
            }

            set
            {
                Requires.IsNotNull(value, nameof(value));

                this.ThrowWhenContainerHasRegistrations(nameof(this.PropertySelectionBehavior));

                this.propertyBehavior = value;
            }
        }

        /// <summary>
        /// Gets or sets the lifestyle selection behavior. The container's default behavior is to make
        /// registrations using the <see cref="Lifestyle.Transient"/> lifestyle.</summary>
        /// <value>The lifestyle selection behavior.</value>
        /// <exception cref="NullReferenceException">Thrown when the supplied value is a null reference.</exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the container already contains registrations.
        /// </exception>
        public ILifestyleSelectionBehavior LifestyleSelectionBehavior
        {
            get
            {
                return this.lifestyleBehavior;
            }

            set
            {
                Requires.IsNotNull(value, nameof(value));

                this.ThrowWhenContainerHasRegistrations(nameof(this.LifestyleSelectionBehavior));

                this.lifestyleBehavior = value;
            }
        }

        /// <summary>
        /// Gets or sets the expression compilation behavior. Changing this behavior allows interception of
        /// the compilation of delegates, for instance debugging purposes.</summary>
        /// <value>The expression compilation behavior.</value>
        /// <exception cref="NullReferenceException">Thrown when the supplied value is a null reference.</exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the container already contains registrations.
        /// </exception>
        public IExpressionCompilationBehavior ExpressionCompilationBehavior
        {
            get
            {
                return this.compilationBehavior;
            }

            set
            {
                Requires.IsNotNull(value, nameof(value));

                this.ThrowWhenContainerHasRegistrations(nameof(this.ExpressionCompilationBehavior));

                this.compilationBehavior = value;
            }
        }

        /// <summary>
        /// Gets or sets the default lifestyle that the container will use when a registration is
        /// made when no lifestyle is supplied.</summary>
        /// <value>The default lifestyle.</value>
        /// <exception cref="NullReferenceException">Thrown when the supplied value is a null reference.</exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the container already contains registrations.
        /// </exception>
        public Lifestyle DefaultLifestyle
        {
            get
            {
                return this.defaultLifestyle;
            }

            set
            {
                Requires.IsNotNull(value, nameof(value));

                this.ThrowWhenContainerHasRegistrations(nameof(this.DefaultLifestyle));

                this.defaultLifestyle = value;
            }
        }

        /// <summary>
        /// Gets or sets the default scoped lifestyle that the container should use when a registration is
        /// made using <see cref="Lifestyle.Scoped">Lifestyle.Scoped</see>.</summary>
        /// <value>The default scoped lifestyle.</value>
        /// <exception cref="NullReferenceException">Thrown when the supplied value is a null reference.</exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the container already contains registrations.
        /// </exception>
        public ScopedLifestyle? DefaultScopedLifestyle
        {
            get
            {
                return this.defaultScopedLifestyle;
            }

            set
            {
                Requires.IsNotNull(value, nameof(value));

                if (object.ReferenceEquals(value, Lifestyle.Scoped))
                {
                    throw new ArgumentException(
                        StringResources.DefaultScopedLifestyleCanNotBeSetWithLifetimeScoped(),
                        nameof(value));
                }

                this.ThrowWhenContainerHasRegistrations(nameof(this.DefaultScopedLifestyle));

                this.defaultScopedLifestyle = value;
            }
        }

        /// <summary>
        /// This property is obsolete and setting it has no effect. To use dynamic assembly compilation, set
        /// the <see cref="ContainerOptions.ExpressionCompilationBehavior"/> property with the custom
        /// <see cref="IExpressionCompilationBehavior"/> implementation from the
        /// SimpleInjector.DynamicAssemblyCompilation package.
        /// </summary>
        /// <value>A boolean indicating whether the container should use a dynamic assembly for compilation.
        /// </value>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        [Obsolete(
            "Changing this value to true has no effect. To use dynamic assembly compilation, set the " +
            nameof(ExpressionCompilationBehavior) + " property with a new " +
            "DynamicAssemblyExpressionCompilationBehavior instance that is located in the " +
            "SimpleInjector.DynamicAssemblyCompilation package. " +
            "Will be treated as an error from version 5.5. Will be removed in version 6.0.",
            error: false)]
        public bool EnableDynamicAssemblyCompilation { get; set; }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        internal int MaximumNumberOfNodesPerDelegate { get; set; } = 350;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        internal string DebuggerDisplayDescription => this.ToString();

        /// <summary>
        /// Registers an <see cref="ResolveInterceptor"/> delegate that allows intercepting calls to
        /// <see cref="SimpleInjector.Container.GetInstance">GetInstance</see> and
        /// <see cref="InstanceProducer.GetInstance()"/>.
        /// </summary>
        /// <remarks>
        /// If multiple registered <see cref="ResolveInterceptor"/> instances must be applied, they will be
        /// applied/wrapped in the order of registration, i.e. the first registered interceptor will call the
        /// original instance producer delegate, the second interceptor will call the first interceptor, etc.
        /// The last registered interceptor will become the outermost method in the chain and will be called
        /// first.
        /// </remarks>
        /// <param name="interceptor">The <see cref="ResolveInterceptor"/> delegate to register.</param>
        /// <param name="predicate">The predicate that will be used to check whether the given delegate must
        /// be applied to a registration or not. The given predicate will be called once for each registration
        /// in the container.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when either the <paramref name="interceptor"/> or <paramref name="predicate"/> are
        /// null references.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when this container instance is locked and can not be altered.
        /// </exception>
        /// <example>
        /// The following example shows the usage of the <see cref="RegisterResolveInterceptor" /> method:
        /// <code lang="cs"><![CDATA[
        /// var container = new Container();
        ///
        /// container.Options.RegisterResolveInterceptor((context, producer) =>
        ///     {
        ///         object instance = producer.Invoke();
        ///         Console.WriteLine(instance.GetType().Name + " resolved for " + context.Producer.ServiceType.Name);
        ///         return instance;
        ///     },
        ///     context => context.Producer.ServiceType.Name.EndsWith("Controller"));
        ///
        /// container.Register<IHomeViewModel, HomeViewModel>();
        /// container.Register<IUserRepository, SqlUserRepository>();
        ///
        /// // This line will write "HomeViewModel resolved for IHomeViewModel" to the console.
        /// container.GetInstance<IHomeViewModel>();
        /// ]]></code>
        /// </example>
        public void RegisterResolveInterceptor(
            ResolveInterceptor interceptor, Predicate<InitializationContext> predicate)
        {
            Requires.IsNotNull(interceptor, nameof(interceptor));
            Requires.IsNotNull(predicate, nameof(predicate));

            this.Container.ThrowWhenContainerIsLockedOrDisposed();

            this.Container.RegisterResolveInterceptor(interceptor, predicate);
        }

        /// <summary>Returns a string that represents the current object.</summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            var descriptions = new List<string>(capacity: 1);

            if (this.AllowOverridingRegistrations)
            {
                descriptions.Add("Allows Overriding Registrations");
            }

            if (!(this.ConstructorResolutionBehavior is DefaultConstructorResolutionBehavior))
            {
                descriptions.Add("Custom Constructor Resolution");
            }

            if (!(this.DependencyInjectionBehavior is DefaultDependencyInjectionBehavior))
            {
                descriptions.Add("Custom Dependency Injection");
            }

            if (!(this.PropertySelectionBehavior is DefaultPropertySelectionBehavior))
            {
                descriptions.Add("Custom Property Selection");
            }

            if (!(this.LifestyleSelectionBehavior is DefaultLifestyleSelectionBehavior))
            {
                descriptions.Add("Custom Lifestyle Selection");
            }

            if (descriptions.Count == 0)
            {
                descriptions.Add("Default Configuration");
            }

            return string.Join(", ", descriptions);
        }

        internal bool IsConstructableType(Type implementationType, out string? errorMessage)
        {
            if (!Types.IsConcreteType(implementationType))
            {
                errorMessage = StringResources.TypeShouldBeConcreteToBeUsedOnThisMethod(implementationType);
                return false;
            }

            var constructor =
                this.ConstructorResolutionBehavior.TryGetConstructor(implementationType, out errorMessage);

            if (constructor is null && errorMessage is null)
            {
                throw new InvalidOperationException(
                    StringResources.TypeHasNoInjectableConstructorAccordingToCustomResolutionBehavior(
                        this.ConstructorResolutionBehavior, implementationType));
            }

            if (constructor != null)
            {
                errorMessage = this.DependencyInjectionBehavior.VerifyConstructor(constructor);
            }

            return errorMessage == null;
        }

        internal InstanceProducer GetInstanceProducerFor(InjectionConsumerInfo consumer)
        {
            var producer = this.DependencyInjectionBehavior.GetInstanceProducer(consumer, throwOnFailure: true);

            // Producer will only be null if a user created a custom IConstructorInjectionBehavior that
            // returned null.
            if (producer == null)
            {
                throw new ActivationException(StringResources.DependencyInjectionBehaviorReturnedNull(
                    this.DependencyInjectionBehavior));
            }

            return producer;
        }

        internal Lifestyle SelectLifestyle(Type implementationType)
        {
            var lifestyle = this.LifestyleSelectionBehavior.SelectLifestyle(implementationType);

            if (lifestyle == null)
            {
                throw new ActivationException(StringResources.LifestyleSelectionBehaviorReturnedNull(
                    this.LifestyleSelectionBehavior, implementationType));
            }

            return lifestyle;
        }

        internal void RaiseContainerLockingAndReset()
        {
            var locking = this.containerLocking;

            if (locking != null)
            {
                // Prevent re-entry.
                this.containerLocking = null;

                locking(this.Container, new ContainerLockingEventArgs());
            }
        }

        internal ConstructorInfo? SelectConstructorOrNull(Type implementationType) =>
            this.ConstructorResolutionBehavior.TryGetConstructor(implementationType, out string? _);

        internal ConstructorInfo SelectConstructor(Type implementatioType) =>
            this.ConstructorResolutionBehavior.GetConstructor(implementatioType);

        private void ThrowWhenContainerHasRegistrations(string propertyName)
        {
            if (this.Container.IsLocked || this.Container.HasRegistrations)
            {
                throw new InvalidOperationException(
                    StringResources.PropertyCanNotBeChangedAfterTheFirstRegistration(propertyName));
            }
        }
    }
}
