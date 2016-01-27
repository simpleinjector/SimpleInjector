﻿#region Copyright Simple Injector Contributors
/* The Simple Injector is an easy-to-use Inversion of Control library for .NET
 * 
 * Copyright (c) 2013-2015 Simple Injector Contributors
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
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
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
    [DebuggerDisplay("{" + nameof(DebuggerDisplayDescription) + ", nq}")]
    public class ContainerOptions
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private IConstructorResolutionBehavior resolutionBehavior;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private IDependencyInjectionBehavior injectionBehavior;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private IPropertySelectionBehavior propertyBehavior;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private ILifestyleSelectionBehavior lifestyleBehavior;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private ScopedLifestyle defaultScopedLifestyle;

        /// <summary>Initializes a new instance of the <see cref="ContainerOptions"/> class.</summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete(
            "This method is not supported anymore. Please use Container.Options to configure the container.",
            error: true)]
        [ExcludeFromCodeCoverage]
        public ContainerOptions()
        {
            throw new InvalidOperationException(
                "This method is not supported anymore. Please use Container.Options to configure the container.");
        }

        internal ContainerOptions(Container container)
        {
            Requires.IsNotNull(container, nameof(container));

            this.Container = container;
            this.resolutionBehavior = new DefaultConstructorResolutionBehavior();
            this.injectionBehavior = new DefaultDependencyInjectionBehavior(container);
            this.propertyBehavior = new DefaultPropertySelectionBehavior();
            this.lifestyleBehavior = new DefaultLifestyleSelectionBehavior(Lifestyle.Transient);
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
        /// is false.
        /// </summary>
        /// <value>The value indicating whether the container should suppress checking for lifestyle
        /// mismatches.</value>
        public bool SuppressLifestyleMismatchVerification { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the container will return an empty collection when
        /// a collection is requested that hasn't been explicitly registered. The default value is <b>false</b>,
        /// which means that the container will throw an exception. Set the value to <b>true</b> to get the
        /// old behavior of Simple Injector v1.x and v2.x.
        /// </summary>
        /// <value>The value indicating whether the container will return an empty collection.</value>
        public bool ResolveUnregisteredCollections { get; set; }

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

                this.ThrowWhenContainerHasRegistrations(nameof(ConstructorResolutionBehavior));

                this.resolutionBehavior = value;
            }
        }

        /// <summary>
        /// Gets or sets the constructor verification behavior. The container's default behavior is to
        /// disallow constructors with value types and strings.
        /// <b>NOTE:</b> This property has been removed. Please use the <see cref="ConstructorInjectionBehavior"/> 
        /// property to override Simple Injector's verification behavior.
        /// </summary>
        /// <value>The constructor resolution behavior.</value>
        [Obsolete("In v3, the IConstructorVerificationBehavior and IConstructorInjectionBehavior interfaces " +
            "have been replaced with the single IDependencyInjectionBehavior interface. Please use the " +
            "DependencyInjectionBehavior property to override Simple Injector's verification behavior.",
            error: true)]
        [ExcludeFromCodeCoverage]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public IConstructorVerificationBehavior ConstructorVerificationBehavior { get; set; }

        /// <summary>Gets or sets the constructor injection behavior.</summary>
        /// <value>The constructor injection behavior.</value>
        /// <exception cref="NullReferenceException">Thrown when the supplied value is a null reference.</exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the container already contains registrations.
        /// </exception>
        [Obsolete("In v3, the IConstructorVerificationBehavior and IConstructorInjectionBehavior interfaces " +
            "have been replaced with the single IDependencyInjectionBehavior interface. Please use the " +
            "DependencyInjectionBehavior property to override Simple Injector's constructor injection behavior.",
            error: true)]
        [ExcludeFromCodeCoverage]
        public IConstructorInjectionBehavior ConstructorInjectionBehavior { get; set; }

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

                this.ThrowWhenContainerHasRegistrations(nameof(DependencyInjectionBehavior));

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

                this.ThrowWhenContainerHasRegistrations(nameof(PropertySelectionBehavior));

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

                this.ThrowWhenContainerHasRegistrations(nameof(LifestyleSelectionBehavior));

                this.lifestyleBehavior = value;
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
        public ScopedLifestyle DefaultScopedLifestyle
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

                this.ThrowWhenContainerHasRegistrations(nameof(DefaultScopedLifestyle));

                this.defaultScopedLifestyle = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the container will use dynamic assemblies for compilation. 
        /// By default, this value is <b>true</b> for the first few containers that are created in an AppDomain 
        /// and <b>false</b> for all other containers. You can set this value explicitly to <b>false</b>
        /// to prevent the use of dynamic assemblies or you can set this value explicitly to <b>true</b> to
        /// force more container instances to use dynamic assemblies. Note that creating an infinite number
        /// of <see cref="SimpleInjector.Container">Container</see> instances (for instance one per web request) 
        /// with this property set to <b>true</b> will result in a memory leak; dynamic assemblies take up 
        /// memory and will only be unloaded when the AppDomain is unloaded.
        /// </summary>
        /// <value>A boolean indicating whether the container should use a dynamic assembly for compilation.
        /// </value>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public bool EnableDynamicAssemblyCompilation { get; set; }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        internal int MaximumNumberOfNodesPerDelegate { get; set; } = 350;
        
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        internal string DebuggerDisplayDescription
        {
            get { return this.ToString(); }
        }

        // This property enables a hidden hook to allow to get notified just before expression trees get
        // compiled. It isn't used internally, but enables debugging in case compiling expressions crashes 
        // the process (which has happened in the past). A user can add the hook using reflection to find out 
        // which expression crashes the system. This property is internal, its not part of the official API, 
        // and we might remove it again in the future.
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        internal Action<Expression> ExpressionCompiling { get; set; } = _ => { };

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
        public void RegisterResolveInterceptor(ResolveInterceptor interceptor,
            Predicate<InitializationContext> predicate)
        {
            Requires.IsNotNull(interceptor, nameof(interceptor));
            Requires.IsNotNull(predicate, nameof(predicate));

            this.Container.ThrowWhenContainerIsLocked();

            this.Container.RegisterResolveInterceptor(interceptor, predicate);
        }

        /// <summary>Returns a string that represents the current object.</summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            var descriptions = new List<string>();

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

        internal bool IsConstructableType(Type serviceType, Type implementationType, out string errorMessage)
        {
            errorMessage = null;

            try
            {
                var constructor = this.SelectConstructor(serviceType, implementationType);

                this.DependencyInjectionBehavior.Verify(serviceType, constructor);
            }
            catch (ActivationException ex)
            {
                errorMessage = ex.Message;
            }

            return errorMessage == null;
        }

        internal ConstructorInfo SelectConstructor(Type serviceType, Type implementationType)
        {
            var constructor = this.ConstructorResolutionBehavior.GetConstructor(serviceType, implementationType);

            if (constructor == null)
            {
                throw new ActivationException(StringResources.ConstructorResolutionBehaviorReturnedNull(
                    this.ConstructorResolutionBehavior, serviceType, implementationType));
            }

            return constructor;
        }

        internal Expression BuildParameterExpression(Type serviceType, Type implementationType, 
            ParameterInfo parameter)
        {
            var consumer = new InjectionConsumerInfo(serviceType, implementationType, parameter);

            Expression expression = this.DependencyInjectionBehavior.BuildExpression(consumer);

            // Expression will only be null if a user created a custom IConstructorInjectionBehavior that
            // returned null.
            if (expression == null)
            {
                throw new ActivationException(StringResources.DependencyInjectionBehaviorReturnedNull(
                    this.DependencyInjectionBehavior));
            }

            return expression;
        }

        internal Lifestyle SelectLifestyle(Type serviceType, Type implementationType)
        {
            var lifestyle = this.LifestyleSelectionBehavior.SelectLifestyle(serviceType, implementationType);

            if (lifestyle == null)
            {
                throw new ActivationException(StringResources.LifestyleSelectionBehaviorReturnedNull(
                    this.LifestyleSelectionBehavior, serviceType, implementationType));
            }

            return lifestyle;
        }

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