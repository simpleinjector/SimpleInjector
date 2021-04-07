// Copyright (c) Simple Injector Contributors. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace SimpleInjector
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using SimpleInjector.Advanced;
    using SimpleInjector.Diagnostics;
    using SimpleInjector.Internals;

    /// <summary>
    /// A <b>Registration</b> implements lifestyle based caching for a single service and allows building an
    /// <see cref="Expression"/> that describes the creation of the service.
    /// </summary>
    /// <remarks>
    /// <see cref="Lifestyle"/> implementations create a new <b>Registration</b> instance for each registered
    /// service type. <see cref="Expression"/>s returned from the
    /// <see cref="Registration.BuildExpression()">BuildExpression</see> method can be
    /// intercepted by any event registered with <see cref="SimpleInjector.Container.ExpressionBuilding" />, have
    /// <see cref="Container.RegisterInitializer{TService}(Action{TService})">initializers</see>
    /// applied, and the caching particular to its lifestyle have been applied. Interception using the
    /// <see cref="Container.ExpressionBuilt">Container.ExpressionBuilt</see> will <b>not</b>
    /// be applied in the <b>Registration</b>, but will be applied in <see cref="InstanceProducer"/>.</remarks>
    /// <example>
    /// See the <see cref="Lifestyle"/> documentation for an example.
    /// </example>
    public abstract class Registration
    {
        private static readonly Action<object> NoOp = _ => { };

        private readonly HashSet<KnownRelationship> knownRelationships = new();

        internal readonly Func<object>? instanceCreator;

        private HashSet<DiagnosticType>? suppressions;
        private ParameterDictionary<OverriddenParameter>? overriddenParameters;
        private Action<object>? instanceInitializer;

        /// <summary>
        /// Initializes a new instance of the <see cref="Registration"/> class.
        /// </summary>
        /// <param name="lifestyle">The <see cref="Lifestyle"/> this that created this registration.</param>
        /// <param name="container">The <see cref="Container"/> instance for this registration.</param>
        /// <param name="implementationType">The type of instance that will be created.</param>
        /// <param name="instanceCreator">
        /// The optional delegate supplied by the user that allows building or creating new instances.
        /// If this argument is supplied, the <see cref="Expression"/> and <see cref="Func{T}"/> instances
        /// build by this <see cref="Registration"/> instance wrap that delegate. When the delegate is omitted,
        /// the built expressions and delegates invoke the <paramref name="implementationType"/>'s constructor.
        /// </param>
        /// <exception cref="ArgumentNullException">Thrown when one of the supplied arguments is a null
        /// reference.</exception>
        protected Registration(
            Lifestyle lifestyle,
            Container container,
            Type implementationType,
            Func<object>? instanceCreator = null)
        {
            Requires.IsNotNull(lifestyle, nameof(lifestyle));
            Requires.IsNotNull(container, nameof(container));
            Requires.IsNotNull(implementationType, nameof(implementationType));

            this.Lifestyle = lifestyle;
            this.Container = container;
            this.ImplementationType = implementationType;
            this.instanceCreator = instanceCreator;
        }

        /// <summary>Gets the type that this instance will create.</summary>
        /// <value>The type that this instance will create.</value>
        public Type ImplementationType { get; }

        /// <summary>Gets the <see cref="Lifestyle"/> this that created this registration.</summary>
        /// <value>The <see cref="Lifestyle"/> this that created this registration.</value>
        public Lifestyle Lifestyle { get; }

        /// <summary>Gets the <see cref="Container"/> instance for this registration.</summary>
        /// <value>The <see cref="Container"/> instance for this registration.</value>
        public Container Container { get; }

        /// <summary>
        /// Gets or sets a value indicating whether the disposal of created instances for this registration
        /// should be suppressed or not. The default is false. Having a value of false, does not force an
        /// instance to be disposed of, though; Transient instances, for instance, will never be disposed of.
        /// </summary>
        /// <value>
        /// Gets or sets a value indicating whether the disposal of created instances for this registration
        /// should be suppressed or not.
        /// </value>
        public bool SuppressDisposal { get; set; }

        internal bool IsCollection { get; set; }

        internal bool? ExpressionIntercepted { get; private set; }

        internal virtual bool MustBeVerified => false;

        internal virtual bool ResolvesExternallyOwnedInstance => false;

        /// <summary>Gets or sets a value indicating whether this registration object contains a user
        /// supplied instanceCreator factory delegate.</summary>
        internal bool WrapsInstanceCreationDelegate { get; set; }

        /// <summary>
        /// Builds a new <see cref="Expression"/> with the correct caching (according to the specifications of
        /// its <see cref="Lifestyle"/>) applied.
        /// </summary>
        /// <returns>An <see cref="Expression"/>.</returns>
        public abstract Expression BuildExpression();

        /// <summary>
        /// Gets the list of <see cref="KnownRelationship"/> instances. Note that the list is only available
        /// after calling <see cref="BuildExpression()"/>.
        /// </summary>
        /// <returns>A new array containing the <see cref="KnownRelationship"/> instances.</returns>
        public KnownRelationship[] GetRelationships() => this.GetRelationshipsCore();

        /// <summary>
        /// Initializes an already created instance and applies properties and initializers to that instance.
        /// </summary>
        /// <remarks>
        /// This method is especially useful in integration scenarios where the given platform is in control
        /// of creating certain types. By passing the instance created by the platform to this method, the
        /// container is still able to apply any properties (as defined using a custom
        /// <see cref="IPropertySelectionBehavior"/>) and by applying any initializers.
        /// </remarks>
        /// <param name="instance">The instance to initialize.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="instance"/> is a null
        /// reference.</exception>
        /// <exception cref="ArgumentException">Thrown when the supplied <paramref name="instance"/> is not
        /// of type <see cref="ImplementationType"/>.</exception>
        public void InitializeInstance(object instance)
        {
            Requires.IsNotNull(instance, nameof(instance));
            Requires.ServiceIsAssignableFromImplementation(
                this.ImplementationType, instance.GetType(), nameof(instance));

            if (this.instanceInitializer is null)
            {
                this.instanceInitializer = this.BuildInstanceInitializer();
            }

            this.instanceInitializer(instance);
        }

        /// <summary>
        /// Suppressing the supplied <see cref="DiagnosticType"/> for the given registration.
        /// </summary>
        /// <param name="type">The <see cref="DiagnosticType"/>.</param>
        /// <param name="justification">The justification of why the warning must be suppressed.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="justification"/> is a null
        /// reference.</exception>
        /// <exception cref="ArgumentException">Thrown when either <paramref name="justification"/> is an
        /// empty string or when <paramref name="type"/> is not a valid value of <see cref="DiagnosticType"/>.
        /// </exception>
        public void SuppressDiagnosticWarning(DiagnosticType type, string justification)
        {
            Requires.IsValidEnum(type, nameof(type));
            Requires.IsNotNullOrEmpty(justification, nameof(justification));

            if (this.suppressions is null)
            {
                this.suppressions = new HashSet<DiagnosticType>();
            }

            this.suppressions.Add(type);
        }

        internal bool ShouldNotBeSuppressed(DiagnosticType type) => this.suppressions?.Contains(type) != true;

        internal virtual KnownRelationship[] GetRelationshipsCore()
        {
            lock (this.knownRelationships)
            {
                return this.knownRelationships.ToArray();
            }
        }

        internal void ReplaceRelationships(IEnumerable<KnownRelationship> relationships)
        {
            lock (this.knownRelationships)
            {
                this.knownRelationships.Clear();

                foreach (var relationship in relationships)
                {
                    this.knownRelationships.Add(relationship);
                }
            }
        }

        internal Expression InterceptInstanceCreation(
            Type implementationType, Expression instanceCreatorExpression)
        {
            var interceptedExpression =
                this.Container.OnExpressionBuilding(this, implementationType, instanceCreatorExpression);

            this.ExpressionIntercepted = interceptedExpression != instanceCreatorExpression;

            return interceptedExpression;
        }

        internal void AddRelationship(KnownRelationship relationship)
        {
            Requires.IsNotNull(relationship, nameof(relationship));

            lock (this.knownRelationships)
            {
                this.knownRelationships.Add(relationship);
            }
        }

        // This method should only be called by the Lifestyle base class and the HybridRegistration.
        internal virtual void SetParameterOverrides(IEnumerable<OverriddenParameter> overrides)
        {
            this.overriddenParameters =
                new ParameterDictionary<OverriddenParameter>(overrides, keySelector: p => p.Parameter);
        }

        // Wraps the expression with a delegate that injects the properties.
        internal Expression WrapWithPropertyInjector(Type implementationType, Expression expressionToWrap)
        {
            if (this.Container.Options.PropertySelectionBehavior is DefaultPropertySelectionBehavior)
            {
                // Performance tweak. DefaultPropertySelectionBehavior never injects any properties.
                // This speeds up the initialization phase.
                return expressionToWrap;
            }

            if (typeof(Container).IsAssignableFrom(implementationType))
            {
                // Don't inject properties on the registration for the Container itself.
                return expressionToWrap;
            }

            return this.WrapWithPropertyInjectorInternal(implementationType, expressionToWrap);
        }

        internal Expression WrapWithInitializer(Type implementationType, Expression expression)
        {
            Action<object>? initializer = this.Container.GetInitializer(implementationType, this);

            if (initializer != null)
            {
                return Expression.Convert(
                    BuildExpressionWithInstanceInitializer(expression, initializer),
                    implementationType);
            }

            return expression;
        }

        /// <summary>
        /// Builds a <see cref="Func{T}"/> delegate for the creation of the <see cref="ImplementationType"/>.
        /// The returned <see cref="Func{T}"/> might be intercepted by a
        /// <see cref="SimpleInjector.Container.ExpressionBuilding">Container.ExpressionBuilding</see> event,
        /// and initializers (if any) (<see cref="SimpleInjector.Container.RegisterInitializer{TService}"/>)
        /// will be applied.
        /// </summary>
        /// <returns>A <see cref="Func{T}"/> delegate.</returns>
        /// <exception cref="ArgumentNullException">Thrown when one of the arguments is a null reference.</exception>
        protected Func<object> BuildTransientDelegate()
        {
            try
            {
                Expression expression = this.BuildTransientExpression();

                // NOTE: The returned delegate could still return null (caused by the ExpressionBuilding event),
                // but I don't feel like protecting us against such an obscure user bug.
                return (Func<object>)this.BuildDelegate(expression);
            }
            catch (CyclicDependencyException ex)
            {
                ex.AddTypeToCycle(this.ImplementationType);
                throw;
            }
        }

        /// <summary>
        /// Builds an <see cref="Expression"/> that describes the creation of <see cref="ImplementationType"/>.
        /// The returned <see cref="Expression"/> might be intercepted by a
        /// <see cref="SimpleInjector.Container.ExpressionBuilding">Container.ExpressionBuilding</see> event,
        /// and initializers (if any) (<see cref="SimpleInjector.Container.RegisterInitializer"/>) can be
        /// applied.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown when one of the arguments is a null reference.
        /// </exception>
        protected Expression BuildTransientExpression()
        {
            try
            {
                Expression expression = this.instanceCreator is null
                    ? this.BuildNewExpression()
                    : this.BuildInvocationExpression(this.instanceCreator);

                expression = this.WrapWithPropertyInjector(this.ImplementationType, expression);
                expression = this.InterceptInstanceCreation(this.ImplementationType, expression);
                expression = this.WrapWithInitializer(this.ImplementationType, expression);
                expression = this.ReplacePlaceHoldersWithOverriddenParameters(expression);

                return expression;
            }
            catch (CyclicDependencyException ex)
            {
                ex.AddTypeToCycle(this.ImplementationType);
                throw;
            }
        }

        private Action<object> BuildInstanceInitializer()
        {
            Type type = this.ImplementationType;

            var parameter = Expression.Parameter(typeof(object));

            var castedParameter = Expression.Convert(parameter, type);

            Expression expression = castedParameter;

            expression = this.WrapWithPropertyInjector(type, expression);
            expression = this.InterceptInstanceCreation(type, expression);

            // NOTE: We can't wrap with the instance created callback, since the InitializeInstance is called
            // directly by a user.
            expression = this.WrapWithInitializer(type, expression);

            if (expression != castedParameter)
            {
                return Expression.Lambda<Action<object>>(expression, parameter).Compile();
            }

            // In this case, no properties and initializers have been applied and the expression wasn't
            // intercepted. Instead of compiling an empty delegate down, we simply return a NoOp.
            return NoOp;
        }

        private Expression BuildNewExpression()
        {
            this.EnsureImplementationTypeInitialized();

            ConstructorInfo constructor = this.Container.Options.SelectConstructor(this.ImplementationType);

            ParameterDictionary<DependencyData> parameters = this.BuildConstructorParameters(constructor);

            var arguments = parameters.Values.Select(v => v.Expression);

            NewExpression expression = Expression.New(constructor, arguments);

            this.AddRelationships(constructor, parameters);

            return expression;
        }

        // Implements #812.
        // The exceptions thrown by the runtime in case of a type initialization error are unproductive.
        // Using this method we can throw a more expressive exception. This is done by appending the inner
        // exception's message to the exception message and replacing the type name with a friendly type name,
        // which is especially useful in the case of generic types. Unfortunately, we can only reliably do
        // this by triggering type initialization, which means we are slightly changing the behavior
        // of Simple Injector. Type initialiation now runs earlier in the pipeline, and could even potentially
        // run in cases where it would normally not run (in case the type is a stand-in where the user
        // replaces the type by altering the expression tree). This, however, is such a rare scenario that I
        // consider this to be a fair trade off.
        private void EnsureImplementationTypeInitialized()
        {
            try
            {
                // The Class Constructor of a type is guaranteed to be to be called just once.
                RuntimeHelpers.RunClassConstructor(this.ImplementationType.TypeHandle);
            }
            catch (TypeInitializationException ex)
            {
                Type type = this.ImplementationType;

                // When the type in question is nested, the exception will contain just the simple
                // type name, while for non-nested types, CLR uses the full name (because of... reasons?).
                string message = ex.Message
                    .Replace($"'{type.FullName}'", type.ToFriendlyName())
                    .Replace($"'{type.Name}'", type.ToFriendlyName());

                throw new ActivationException(message + " " + ex.InnerException?.Message, ex);
            }
        }

        private Expression BuildInvocationExpression(Func<object> instanceCreator)
        {
            Expression expression = Expression.Invoke(Expression.Constant(instanceCreator));

            var funcType = typeof(Func<>).MakeGenericType(this.ImplementationType);

            if (!funcType.IsAssignableFrom(instanceCreator.GetType()))
            {
                // The supplied Func<T> is not a Func{ImplementationType}, which means it needs a cast.
                expression = Expression.Convert(expression, this.ImplementationType);
            }

            return this.WrapWithNullCheck(expression);
        }

        private ParameterDictionary<DependencyData> BuildConstructorParameters(ConstructorInfo constructor)
        {
            // NOTE: We used to use a LINQ query here (which is cleaner code), but we reverted back to using
            // a foreach statement to clean up the stack trace, since this is a very common code path to
            // show up in the stack trace and preventing showing up the Enumerable and Buffer`1 calls here
            // makes it easier for developers (and maintainers) to read the stack trace.
            var parameters = new ParameterDictionary<DependencyData>();

            foreach (ParameterInfo parameter in constructor.GetParameters())
            {
                var consumer = new InjectionConsumerInfo(parameter);
                Expression expression = this.GetPlaceHolderFor(parameter);
                InstanceProducer? producer = null;

                if (expression is null)
                {
                    producer = this.Container.Options.GetInstanceProducerFor(consumer);
                    expression = producer.BuildExpression();
                }

                parameters.Add(parameter, new DependencyData(parameter, expression, producer));
            }

            return parameters;
        }

        private ConstantExpression GetPlaceHolderFor(ParameterInfo parameter) =>
            this.GetOverriddenParameterFor(parameter).PlaceHolder;

        private Expression WrapWithPropertyInjectorInternal(
            Type implementationType, Expression expressionToWrap)
        {
            PropertyInfo[] properties = this.GetPropertiesToInject(implementationType);

            if (properties.Length > 0)
            {
                PropertyInjectionHelper.VerifyProperties(properties);

                var data = PropertyInjectionHelper.BuildPropertyInjectionExpression(
                    this.Container, implementationType, properties, expressionToWrap);

                expressionToWrap = data.Expression;

                var knownRelationships =
                    from pair in data.Producers.Zip(data.Properties, (prod, prop) => new { prod, prop })
                    select new KnownRelationship(
                        implementationType: implementationType,
                        lifestyle: this.Lifestyle,
                        consumer: new InjectionConsumerInfo(implementationType, pair.prop!),
                        dependency: pair.prod!);

                foreach (var knownRelationship in knownRelationships)
                {
                    this.AddRelationship(knownRelationship);
                }
            }

            return expressionToWrap;
        }

        private PropertyInfo[] GetPropertiesToInject(Type implementationType)
        {
            var propertySelector = this.Container.Options.PropertySelectionBehavior;

            var candidates = PropertyInjectionHelper.GetCandidateInjectionPropertiesFor(implementationType);

            // Optimization: Safes creation of multiple objects in case there are no candidates.
            return candidates.Length == 0
                ? candidates
                : candidates.Where(p => propertySelector.SelectProperty(implementationType, p)).ToArray();
        }

        private Expression ReplacePlaceHoldersWithOverriddenParameters(Expression expression)
        {
            if (this.overriddenParameters != null)
            {
                foreach (var overriddenParameter in this.overriddenParameters.Values)
                {
                    expression = SubExpressionReplacer.Replace(
                        expressionToAlter: expression,
                        nodeToFind: overriddenParameter.PlaceHolder,
                        replacementNode: overriddenParameter.Expression);
                }
            }

            return expression;
        }

        private OverriddenParameter GetOverriddenParameterFor(ParameterInfo parameter)
        {
            if (this.overriddenParameters != null
                && this.overriddenParameters.TryGetValue(parameter, out OverriddenParameter p))
            {
                return p;
            }
            else
            {
                return default;
            }
        }

        private void AddRelationships(
            ConstructorInfo constructor, ParameterDictionary<DependencyData> parameters)
        {
            var knownRelationships =
                from dependency in parameters.Values
                let producer = dependency.Producer
                    ?? this.GetOverriddenParameterFor(dependency.Parameter).Producer
                select new KnownRelationship(
                    implementationType: constructor.DeclaringType,
                    lifestyle: this.Lifestyle,
                    consumer: new InjectionConsumerInfo(dependency.Parameter),
                    dependency: producer);

            foreach (var knownRelationship in knownRelationships)
            {
                this.AddRelationship(knownRelationship);
            }
        }

        private Expression WrapWithNullCheck(Expression expression)
        {
            if (expression.Type.IsValueType())
            {
                return expression;
            }

            Func<object> thrower = () => throw new ActivationException(
                StringResources.DelegateForTypeReturnedNull(this.ImplementationType));

            // Build the follwoing expression: "instanceCreator() ?? thrower()"
            return Expression.Coalesce(
                left: expression,
                right: Expression.Convert(
                    Expression.Invoke(Expression.Constant(thrower)),
                    this.ImplementationType));
        }

        private static Expression BuildExpressionWithInstanceInitializer<TImplementation>(
            Expression newExpression, Action<TImplementation> instanceInitializer)
            where TImplementation : class
        {
            Func<TImplementation, TImplementation> instanceCreatorWithInitializer = instance =>
            {
                instanceInitializer(instance);

                return instance;
            };

            return Expression.Invoke(Expression.Constant(instanceCreatorWithInitializer), newExpression);
        }

        private Delegate BuildDelegate(Expression expression)
        {
            try
            {
                return CompilationHelpers.CompileExpression(this.Container, expression);
            }
            catch (Exception ex)
            {
                string message = StringResources.ErrorWhileBuildingDelegateFromExpression(
                    this.ImplementationType, expression, ex);

                throw new ActivationException(message, ex);
            }
        }

        private struct DependencyData
        {
            public readonly ParameterInfo Parameter;
            public readonly InstanceProducer? Producer;
            public readonly Expression Expression;

            public DependencyData(ParameterInfo parameter, Expression expression, InstanceProducer? producer)
            {
                this.Parameter = parameter;
                this.Expression = expression;
                this.Producer = producer;
            }
        }
    }
}