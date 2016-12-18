#region Copyright Simple Injector Contributors
/* The Simple Injector is an easy-to-use Inversion of Control library for .NET
 * 
 * Copyright (c) 2013-2016 Simple Injector Contributors
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
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
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
    /// <see cref="Registration.BuildExpression(InstanceProducer)">BuildExpression</see> method can be 
    /// intercepted by any event registered with <see cref="SimpleInjector.Container.ExpressionBuilding" />, have 
    /// <see cref="SimpleInjector.Container.RegisterInitializer{TService}(Action{TService})">initializers</see> 
    /// applied, and the caching particular to its lifestyle have been applied. Interception using the 
    /// <see cref="SimpleInjector.Container.ExpressionBuilt">Container.ExpressionBuilt</see> will <b>not</b> 
    /// be applied in the <b>Registration</b>, but will be applied in <see cref="InstanceProducer"/>.</remarks>
    /// <example>
    /// See the <see cref="Lifestyle"/> documentation for an example.
    /// </example>
    public abstract class Registration
    {
        private static readonly Action<object> NoOp = instance => { };

        private readonly HashSet<KnownRelationship> knownRelationships = new HashSet<KnownRelationship>();

        private HashSet<DiagnosticType> suppressions;
        private Dictionary<object, OverriddenParameter> overriddenParameters;
        private Action<object> instanceInitializer;

        /// <summary>
        /// Initializes a new instance of the <see cref="Registration"/> class.
        /// </summary>
        /// <param name="lifestyle">The <see cref="Lifestyle"/> this that created this registration.</param>
        /// <param name="container">The <see cref="Container"/> instance for this registration.</param>
        /// <exception cref="ArgumentNullException">Thrown when one of the supplied arguments is a null
        /// reference (Nothing in VB).</exception>
        protected Registration(Lifestyle lifestyle, Container container)
        {
            Requires.IsNotNull(lifestyle, nameof(lifestyle));
            Requires.IsNotNull(container, nameof(container));

            this.Lifestyle = lifestyle;
            this.Container = container;
        }

        /// <summary>Gets the type that this instance will create.</summary>
        /// <value>The type that this instance will create.</value>
        public abstract Type ImplementationType { get; }

        /// <summary>Gets the <see cref="Lifestyle"/> this that created this registration.</summary>
        /// <value>The <see cref="Lifestyle"/> this that created this registration.</value>
        public Lifestyle Lifestyle { get; }

        /// <summary>Gets the <see cref="Container"/> instance for this registration.</summary>
        /// <value>The <see cref="Container"/> instance for this registration.</value>
        public Container Container { get; }

        internal bool IsCollection { get; set; }

        internal virtual bool MustBeVerified => false;

        /// <summary>Gets or sets a value indicating whether this registration object contains a user 
        /// supplied instanceCreator factory delegate.</summary>
        internal bool WrapsInstanceCreationDelegate { get; set; }

        /// <summary>
        /// Builds a new <see cref="Expression"/> with the correct caching (according to the specifications of
        /// its <see cref="Lifestyle"/>) applied.
        /// </summary>
        /// <param name="producer">The  producer that is requesting the construction of the expression.
        /// The value can be null.</param>
        /// <returns>An <see cref="Expression"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="producer"/> is a null 
        /// reference.</exception>
        public abstract Expression BuildExpression(InstanceProducer producer);

        /// <summary>
        /// Gets the list of <see cref="KnownRelationship"/> instances. Note that the list is only available
        /// after calling <see cref="BuildExpression(InstanceProducer)"/>.
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
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="instance"/> is a null reference
        /// (Nothing in VB).</exception>
        /// <exception cref="ArgumentException">Thrown when the supplied <paramref name="instance"/> is not
        /// of type <see cref="ImplementationType"/>.</exception>
        public void InitializeInstance(object instance)
        {
            Requires.IsNotNull(instance, nameof(instance));
            Requires.ServiceIsAssignableFromImplementation(this.ImplementationType, instance.GetType(),
                nameof(instance));

            if (this.instanceInitializer == null)
            {
                this.instanceInitializer = this.BuildInstanceInitializer(null);
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

            if (this.suppressions == null)
            {
                this.suppressions = new HashSet<DiagnosticType>();
            }

            this.suppressions.Add(type);
        }

        internal bool ShouldNotBeSuppressed(DiagnosticType type) =>
            this.suppressions == null || !this.suppressions.Contains(type);

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

        internal Expression InterceptInstanceCreation(Type serviceType, Type implementationType,
            Expression instanceCreatorExpression)
        {
            return this.Container.OnExpressionBuilding(this, serviceType, implementationType,
                instanceCreatorExpression);
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
        internal virtual void SetParameterOverrides(IEnumerable<OverriddenParameter> parameters)
        {
            this.overriddenParameters = parameters.ToDictionary(p => GetParameterKey(p.Parameter));
        }

        // Wraps the expression with a delegate that injects the properties.
        internal Expression WrapWithPropertyInjector(Type serviceType, Type implementationType,
            Expression expressionToWrap)
        {
            if (this.Container.Options.PropertySelectionBehavior is DefaultPropertySelectionBehavior)
            {
                // Performance tweak. DefaultPropertySelectionBehavior never injects any properties.
                // This speeds up the initialization phase.
                return expressionToWrap;
            }

            return this.WrapWithPropertyInjectorInternal(serviceType, implementationType, expressionToWrap);
        }

        internal Expression WrapWithInitializer(InstanceProducer producer, Type serviceType,
            Type implementationType, Expression expression)
        {
            var context = new InitializationContext(producer, this);

            Action<object> initializer = this.Container.GetInitializer(implementationType, context);

            if (initializer != null)
            {
                return Expression.Convert(
                    BuildExpressionWithInstanceInitializer<object>(expression, initializer),
                    implementationType);
            }

            return expression;
        }

        /// <summary>
        /// Builds a <see cref="Func{T}"/> delegate for the creation of the <typeparamref name="TService"/>
        /// using the supplied <paramref name="instanceCreator"/>. The returned <see cref="Func{T}"/> might
        /// be intercepted by a 
        /// <see cref="SimpleInjector.Container.ExpressionBuilding">Container.ExpressionBuilding</see> event, 
        /// and the <paramref name="instanceCreator"/> will have been wrapped with a delegate that executes the
        /// registered <see cref="SimpleInjector.Container.RegisterInitializer{TService}">initializers</see> 
        /// that are applicable to the given <typeparamref name="TService"/> (if any).
        /// </summary>
        /// <typeparam name="TService">The interface or base type that can be used to retrieve instances.</typeparam>
        /// <param name="producer">The instance producer that is requesting the construction of the delegate.</param>
        /// <param name="instanceCreator">
        /// The delegate supplied by the user that allows building or creating new instances.</param>
        /// <returns>A <see cref="Func{T}"/> delegate.</returns>
        /// <exception cref="ArgumentNullException">Thrown when one of the arguments is a null reference.</exception>
        protected Func<TService> BuildTransientDelegate<TService>(InstanceProducer producer,
            Func<TService> instanceCreator)
            where TService : class
        {
            Requires.IsNotNull(producer, nameof(producer));
            Requires.IsNotNull(instanceCreator, nameof(instanceCreator));

            Expression expression = this.BuildTransientExpression(producer, instanceCreator);

            // NOTE: The returned delegate could still return null (caused by the ExpressionBuilding event),
            // but I don't feel like protecting us against such an obscure user bug.
            return (Func<TService>)this.BuildDelegate(expression);
        }

        /// <summary>
        /// Builds a <see cref="Func{T}"/> delegate for the creation of <see cref="ImplementationType"/>.
        /// The returned <see cref="Func{T}"/> might be intercepted by a 
        /// <see cref="SimpleInjector.Container.ExpressionBuilding">Container.ExpressionBuilding</see> event, 
        /// and the creation of the <see cref="ImplementationType"/> will have been wrapped with a 
        /// delegate that executes the registered 
        /// <see cref="SimpleInjector.Container.RegisterInitializer{TService}">initializers</see> 
        /// that are applicable to the given <see cref="ImplementationType"/> (if any).
        /// </summary>
        /// <param name="producer">The instance producer that is requesting the construction of the delegate.</param>
        /// <returns>A <see cref="Func{T}"/> delegate.</returns>
        /// <exception cref="ArgumentNullException">Thrown when one of the arguments is a null reference.</exception>
        protected Func<object> BuildTransientDelegate(InstanceProducer producer)
        {
            Requires.IsNotNull(producer, nameof(producer));

            Expression expression = this.BuildTransientExpression(producer);

            return (Func<object>)this.BuildDelegate(expression);
        }

        /// <summary>
        /// Builds an <see cref="Expression"/> that describes the creation of the <typeparamref name="TService"/>
        /// using the supplied <paramref name="instanceCreator"/>. The returned <see cref="Expression"/> might
        /// be intercepted by a 
        /// <see cref="SimpleInjector.Container.ExpressionBuilding">Container.ExpressionBuilding</see> event, 
        /// and the <paramref name="instanceCreator"/> will have been wrapped with a delegate that executes the
        /// registered <see cref="SimpleInjector.Container.RegisterInitializer">initializers</see> that are 
        /// applicable to the given <typeparamref name="TService"/> (if any).
        /// </summary>
        /// <typeparam name="TService">The interface or base type that can be used to retrieve instances.</typeparam>
        /// <param name="producer">The  producer that is requesting the construction of the expression.</param>
        /// <param name="instanceCreator">
        /// The delegate supplied by the user that allows building or creating new instances.</param>
        /// <returns>An <see cref="Expression"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when one of the arguments is a null reference.</exception>
        protected Expression BuildTransientExpression<TService>(InstanceProducer producer, 
            Func<TService> instanceCreator)
            where TService : class
        {
            Requires.IsNotNull(producer, nameof(producer));
            Requires.IsNotNull(instanceCreator, nameof(instanceCreator));

            Expression expression = Expression.Invoke(Expression.Constant(instanceCreator));

            expression = WrapWithNullChecker<TService>(expression);
            expression = this.WrapWithPropertyInjector(typeof(TService), typeof(TService), expression);
            expression = this.InterceptInstanceCreation(typeof(TService), typeof(TService), expression);
            expression = this.WrapWithInitializer(producer, typeof(TService), typeof(TService), expression);

            return expression;
        }

        /// <summary>
        /// Builds an <see cref="Expression"/> that describes the creation of <see cref="ImplementationType"/>. 
        /// The returned <see cref="Expression"/> might be intercepted
        /// by a <see cref="SimpleInjector.Container.ExpressionBuilding">Container.ExpressionBuilding</see>
        /// event, and the creation of the <see cref="ImplementationType"/> will have been wrapped with
        /// a delegate that executes the registered 
        /// <see cref="SimpleInjector.Container.RegisterInitializer">initializers</see> that are applicable 
        /// to the InstanceProducer's <see cref="InstanceProducer.ServiceType">ServiceType</see> (if any).
        /// </summary>
        /// <param name="producer">The instance producer that is requesting the construction of the expression.</param>
        /// <returns>An <see cref="Expression"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when one of the arguments is a null reference.</exception>
        protected Expression BuildTransientExpression(InstanceProducer producer)
        {
            Requires.IsNotNull(producer, nameof(producer));

            Expression expression = this.BuildNewExpression(producer);

            expression = this.WrapWithPropertyInjector(producer.ServiceType, this.ImplementationType, expression);
            expression = this.InterceptInstanceCreation(producer.ServiceType, this.ImplementationType, expression);
            expression = this.WrapWithInitializer(producer, producer.ServiceType, this.ImplementationType, expression);

            return this.ReplacePlaceHoldersWithOverriddenParameters(expression);
        }

        private Action<object> BuildInstanceInitializer(InstanceProducer producer)
        {
            Type type = this.ImplementationType;

            var parameter = Expression.Parameter(typeof(object));

            var castedParameter = Expression.Convert(parameter, type);

            Expression expression = castedParameter;

            expression = this.WrapWithPropertyInjector(type, type, castedParameter);
            expression = this.InterceptInstanceCreation(type, type, expression);

            // NOTE: We can't wrap with the instance created callback, since the InitializeInstance is called
            // directly by a user.
            expression = this.WrapWithInitializer(producer, type, type, expression);

            if (expression != castedParameter)
            {
                return Expression.Lambda<Action<object>>(expression, parameter).Compile();
            }

            // In this case, no properties and initializers have been applied and the expression wasn't
            // intercepted. Instead of compiling an empty delegate down, we simply return a NoOp.
            return NoOp;
        }

        private Expression BuildNewExpression(InstanceProducer producer)
        {
            ConstructorInfo constructor = this.Container.Options.SelectConstructor(this.ImplementationType);

            NewExpression expression = Expression.New(constructor,
                this.BuildConstructorParameters(producer.ServiceType, this.ImplementationType, constructor));

            this.AddConstructorParametersAsKnownRelationship(producer.ServiceType, this.ImplementationType, constructor);

            return expression;
        }

        private Expression[] BuildConstructorParameters(Type serviceType, Type implementationType,
            ConstructorInfo constructor)
        {
            // NOTE: We used to use a LINQ query here (which is cleaner code), but we reverted back to using
            // a foreach statement to clean up the stack trace, since this is a very common code path to
            // show up in the stack trace and preventing showing up the Enumerable and Buffer`1 calls here
            // makes it easier for developers (and maintainers) to read the stack trace.
            var parameters = new List<Expression>();

            foreach (ParameterInfo parameter in constructor.GetParameters())
            {
                var consumer = new InjectionConsumerInfo(serviceType, implementationType, parameter);
                Expression constructorParameter = this.BuildConstructorParameterFor(consumer);
                parameters.Add(constructorParameter);
            }

            return parameters.ToArray();
        }

        private Expression BuildConstructorParameterFor(InjectionConsumerInfo consumer) =>
            this.GetPlaceHolderFor(consumer) ?? this.Container.Options.BuildParameterExpression(consumer);

        private ConstantExpression GetPlaceHolderFor(InjectionConsumerInfo consumer) =>
            this.GetOverriddenParameterFor(consumer.Target.Parameter).PlaceHolder;

        private Expression WrapWithPropertyInjectorInternal(Type serviceType, Type implementationType,
            Expression expression)
        {
            var properties = this.GetPropertiesToInject(serviceType, implementationType);

            if (properties.Any())
            {
                PropertyInjectionHelper.VerifyProperties(properties);

                expression = PropertyInjectionHelper.BuildPropertyInjectionExpression(
                    this.Container, serviceType, implementationType, properties, expression);

                this.AddPropertiesAsKnownRelationships(serviceType, implementationType, properties);
            }

            return expression;
        }

        private PropertyInfo[] GetPropertiesToInject(Type serviceType, Type implementationType)
        {
            var propertySelector = this.Container.Options.PropertySelectionBehavior;

            var candidates = PropertyInjectionHelper.GetCandidateInjectionPropertiesFor(implementationType);

            return (
                from property in candidates
                where serviceType != typeof(Container)
                where propertySelector.SelectProperty(serviceType, property)
                select property)
                .ToArray();
        }

        private Expression ReplacePlaceHoldersWithOverriddenParameters(Expression expression)
        {
            if (this.overriddenParameters != null)
            {
                foreach (var overriddenParameter in this.overriddenParameters.Values)
                {
                    expression = SubExpressionReplacer.Replace(
                        expressionToAlter: expression,
                        subExpressionToFind: overriddenParameter.PlaceHolder,
                        replacementExpression: overriddenParameter.Expression);
                }
            }

            return expression;
        }

        private OverriddenParameter GetOverriddenParameterFor(ParameterInfo parameter)
        {
            if (this.overriddenParameters != null)
            {
                object key = GetParameterKey(parameter);

                if (this.overriddenParameters.ContainsKey(key))
                {
                    return this.overriddenParameters[key];
                }
            }

            return new OverriddenParameter();
        }

        private void AddConstructorParametersAsKnownRelationship(Type serviceType, Type implementationType,
            ConstructorInfo constructor)
        {
            // We have to suppress the overridden parameter since this might result in a wrong relationship.
            var dependencyTypes =
                from parameter in constructor.GetParameters()
                let type = parameter.ParameterType
                let overriddenProducer = this.GetOverriddenParameterFor(parameter).Producer
                let context = new InjectionConsumerInfo(serviceType, implementationType, parameter)
                let instanceProducer =
                    overriddenProducer ?? this.Container.GetRegistrationEvenIfInvalid(type, context)
                where instanceProducer != null
                select instanceProducer;

            this.AddRelationships(constructor.DeclaringType, dependencyTypes);
        }

        private void AddPropertiesAsKnownRelationships(Type serviceType, Type implementationType,
            IEnumerable<PropertyInfo> properties)
        {
            var dependencies =
                from property in properties
                let dependencyType = property.PropertyType
                let context = new InjectionConsumerInfo(serviceType, implementationType, property)
                let instanceProducer = this.Container.GetRegistrationEvenIfInvalid(dependencyType, context)
                where instanceProducer != null
                select instanceProducer;

            this.AddRelationships(implementationType, dependencies);
        }

        private void AddRelationships(Type implementationType, IEnumerable<InstanceProducer> dependencies)
        {
            var relationships =
                from dependency in dependencies
                select new KnownRelationship(implementationType, this.Lifestyle, dependency);

            foreach (var relationship in relationships)
            {
                this.AddRelationship(relationship);
            }
        }

        private static Expression WrapWithNullChecker<TService>(Expression expression)
        {
            Func<TService, TService> nullChecker = ThrowWhenNull<TService>;

            return Expression.Invoke(Expression.Constant(nullChecker), expression);
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
                return CompilationHelpers.CompileExpression(this.ImplementationType, this.Container, expression);
            }
            catch (Exception ex)
            {
                string message = StringResources.ErrorWhileBuildingDelegateFromExpression(
                    this.ImplementationType, expression, ex);

                throw new ActivationException(message, ex);
            }
        }

        private static TService ThrowWhenNull<TService>(TService instance)
        {
            if (instance == null)
            {
                throw new ActivationException(StringResources.DelegateForTypeReturnedNull(typeof(TService)));
            }

            return instance;
        }

        // HACK: ParameterInfo is not guaranteed to be unique (while Type and MemberBase are). This caused 
        // the bug described in #323. By creating this key, we can match multiple PatereterInfo objects that
        // reference the same parameter.
        private static object GetParameterKey(ParameterInfo param) => new { param.Name, param.Member };
    }
}