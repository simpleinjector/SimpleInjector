#region Copyright (c) 2013 S. van Deursen
/* The Simple Injector is an easy-to-use Inversion of Control library for .NET
 * 
 * Copyright (C) 2013 S. van Deursen
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

namespace SimpleInjector
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Threading;
    using SimpleInjector.Advanced;

    /// <summary>
    /// A <b>Registration</b> implements lifestyle based caching for a single service and allows building an
    /// <see cref="Expression"/> that describes the creation of the service.
    /// </summary>
    /// <remarks>
    /// <see cref="Lifestyle"/> implementations create a new <b>Registration</b> instance for each registered
    /// service type. <see cref="Expression"/>s returned from the 
    /// <see cref="Registration.BuildExpression()">BuildExpression</see> method can be intercepted by any event
    /// registered with <see cref="SimpleInjector.Container.ExpressionBuilding" />, have 
    /// <see cref="SimpleInjector.Container.RegisterInitializer{TService}(Action{TService})">initializers</see> 
    /// applied, and the caching particular to its lifestyle have been applied. Interception using the 
    /// <see cref="SimpleInjector.Container.ExpressionBuilt">Container.ExpressionBuilt</see> will <b>not</b> 
    /// be applied in the <b>Registration</b>, but will be applied in <see cref="InstanceProducer"/>.</remarks>
    /// <example>
    /// See the <see cref="Lifestyle"/> documentation for an example.
    /// </example>
    [SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable",
        // I'm still wondering whether it was better to use Dictionary<Thread, InstanceProducer> instead of
        // ThreadLocal<InstanceProducer>. That would have prevented me from having to write all this :-)
        Justification = @"This class references ThreadLocal<InstanceProducer> and ThreadLocal<T> implements
            IDisposable. Not letting Registration implement IDisposable however is not a problem, because:
            -Unless a user registers the InstanceCreated event, no ThreadLocal<T> will get created. 
            -Registration objects will typically live as long as the AppDomain, so for those instances   
             disposing is not an issue.                                                                  
            -The InstanceProducers stored in the ThreadLocal<T> also live long and will be removed from the 
             ThreadLocal<T> during the call to BuildExpression(InstanceProducer) and the ThreadLocal will 
             because of this not keep those instances alive unnecessary.                             
            -ThreadLocal<T> implements a finalizer and the GC will eventually clean up those few instances
             that are referenced in a Registration that gets dereferenced.")]
    public abstract class Registration
    {
        private static readonly Action<object> NoOp = instance => { };

        private readonly HashSet<KnownRelationship> dependencies = new HashSet<KnownRelationship>();
        private readonly ThreadLocal<InstanceProducer> currentProducer = new ThreadLocal<InstanceProducer>();
        
        private Dictionary<ParameterInfo, OverriddenParameter> overriddenParameters;
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
            Requires.IsNotNull(lifestyle, "lifestyle");
            Requires.IsNotNull(container, "container");
            
            this.Lifestyle = lifestyle;
            this.Container = container;
        }
        
        /// <summary>Gets the type that this instance will create.</summary>
        /// <value>The type that this instance will create.</value>
        public abstract Type ImplementationType { get; }

        /// <summary>Gets the <see cref="Lifestyle"/> this that created this registration.</summary>
        /// <value>The <see cref="Lifestyle"/> this that created this registration.</value>
        public Lifestyle Lifestyle { get; private set; }

        internal bool IsCollection { get; set; }

        /// <summary>Gets the <see cref="Container"/> instance for this registration.</summary>
        /// <value>The <see cref="Container"/> instance for this registration.</value>
        protected internal Container Container { get; private set; }

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
        public KnownRelationship[] GetRelationships()
        {
            return this.GetRelationshipsCore();
        }

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
            Requires.IsNotNull(instance, "instance");
            Requires.ServiceIsAssignableFromImplementation(this.ImplementationType, instance.GetType(), 
                "instance");

            if (this.instanceInitializer == null)
            {
                this.instanceInitializer = this.BuildInstanceInitializer();
            }

            this.instanceInitializer(instance);
        }

        internal Expression BuildExpression(InstanceProducer producer)
        {
            // This is an ugly hack. We can't pass on the supplied producer down the callstack, since that would
            // mean we'd have to introduce a breaking change in the API. Instead we pass the producer through
            // using a ThreadLocal<T>. This isn't pretty, but it does the trick.
            try
            {
                this.SetCurrentProducer(producer);

                return this.BuildExpression();
            }
            finally
            {
                this.ResetCurrentProducer();
            }
        }

        internal virtual KnownRelationship[] GetRelationshipsCore()
        {
            lock (this.dependencies)
            {
                return this.dependencies.ToArray();
            }
        }

        internal void ReplaceRelationships(IEnumerable<KnownRelationship> dependencies)
        {
            lock (this.dependencies)
            {
                this.dependencies.Clear();

                foreach (var dependency in dependencies)
                {
                    this.dependencies.Add(dependency);
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
            Requires.IsNotNull(relationship, "relationship");

            lock (this.dependencies)
            {
                this.dependencies.Add(relationship);
            }
        }

        // This method should only be called by the Lifestyle base class and the HybridRegistration.
        internal virtual void SetParameterOverrides(IEnumerable<OverriddenParameter> overriddenParameters)
        {
            this.overriddenParameters = overriddenParameters.ToDictionary(p => p.Parameter);
        }
   
        // Wraps the expression with a delegate that injects the properties.
        internal Expression WrapWithPropertyInjector(Type serviceType, Type implementationType,
            Expression expressionToWrap)
        {
            if (this.Container.Options.PropertySelectionBehavior is DefaultPropertySelectionBehavior)
            {
                // Performance tweak. DefaultPropertySelectionBehavior never injects any properties.
                // This speeds up the the initialization phase.
                return expressionToWrap;
            }

            return this.WrapWithPropertyInjectorInternal(serviceType, implementationType, expressionToWrap);
        }

        internal Expression WrapWithInitializer(Type serviceType, Type implementationType, Expression expression)
        {
            var context = new InitializationContext(this.GetCurrentProducer(), this);

            Action<object> instanceInitializer = this.Container.GetInitializer(implementationType, context);

            if (instanceInitializer != null)
            {
                return Expression.Convert(
                    BuildExpressionWithInstanceInitializer<object>(expression, instanceInitializer),
                    serviceType);
            }

            return expression;
        }

        internal InstanceProducer GetCurrentProducer()
        {
            return this.currentProducer.Value;
        }

        /// <summary>
        /// Builds a <see cref="Func{T}"/> delegate for the creation of the <typeparamref name="TService"/>
        /// using the supplied <paramref name="instanceCreator"/>. The returned <see cref="Func{T}"/> might
        /// be intercepted by a 
        /// <see cref="SimpleInjector.Container.ExpressionBuilding">Container.ExpressionBuilding</see> event, 
        /// and the <paramref name="instanceCreator"/> will have been wrapped with a delegate that executes the
        /// registered <see cref="SimpleInjector.Container.RegisterInitializer{TService}">initializers</see> 
        /// that are appliable to the given <typeparamref name="TService"/> (if any).
        /// </summary>
        /// <typeparam name="TService">The interface or base type that can be used to retrieve instances.</typeparam>
        /// <param name="instanceCreator">
        /// The delegate supplied by the user that allows building or creating new instances.</param>
        /// <returns>A <see cref="Func{T}"/> delegate.</returns>
        protected Func<TService> BuildTransientDelegate<TService>(Func<TService> instanceCreator)
            where TService : class
        {
            Requires.IsNotNull(instanceCreator, "instanceCreator");

            Expression expression = this.BuildTransientExpression<TService>(instanceCreator);

            // NOTE: The returned delegate could still return null (caused by the ExpressionBuilding event),
            // but I don't feel like protecting us against such an obscure user bug.
            return BuildDelegate<TService>(expression);
        }

        /// <summary>
        /// Builds a <see cref="Func{T}"/> delegate for the creation of <typeparamref name="TImplementation"/>.
        /// The returned <see cref="Func{T}"/> might be intercepted by a 
        /// <see cref="SimpleInjector.Container.ExpressionBuilding">Container.ExpressionBuilding</see> event, 
        /// and the creation of the <typeparamref name="TImplementation"/> will have been wrapped with a 
        /// delegate that executes the registered 
        /// <see cref="SimpleInjector.Container.RegisterInitializer{TService}">initializers</see> 
        /// that are appliable to the given <typeparamref name="TService"/> (if any).
        /// </summary>
        /// <typeparam name="TService">The interface or base type that can be used to retrieve instances.</typeparam>
        /// <typeparam name="TImplementation">The concrete type that will be registered.</typeparam>
        /// <returns>A <see cref="Func{T}"/> delegate.</returns>
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter",
            Justification = @"
                Supplying the generic type arguments is needed, since internal types can not be created using 
                the non-generic overloads in a sandbox.")]
        protected Func<TImplementation> BuildTransientDelegate<TService, TImplementation>()
            where TImplementation : class, TService
            where TService : class
        {
            Expression expression = this.BuildTransientExpression<TService, TImplementation>();

            return BuildDelegate<TImplementation>(expression);
        }

        /// <summary>
        /// Builds an <see cref="Expression"/> that describes the creation of the <typeparamref name="TService"/>
        /// using the supplied <paramref name="instanceCreator"/>. The returned <see cref="Expression"/> might
        /// be intercepted by a 
        /// <see cref="SimpleInjector.Container.ExpressionBuilding">Container.ExpressionBuilding</see> event, 
        /// and the <paramref name="instanceCreator"/> will have been wrapped with a delegate that executes the
        /// registered <see cref="SimpleInjector.Container.RegisterInitializer">initializers</see> that are 
        /// appliable to the given <typeparamref name="TService"/> (if any).
        /// </summary>
        /// <typeparam name="TService">The interface or base type that can be used to retrieve instances.</typeparam>
        /// <param name="instanceCreator">
        /// The delegate supplied by the user that allows building or creating new instances.</param>
        /// <returns>An <see cref="Expression"/>.</returns>
        protected Expression BuildTransientExpression<TService>(Func<TService> instanceCreator)
            where TService : class
        {
            Requires.IsNotNull(instanceCreator, "instanceCreator");

            Expression expression = Expression.Invoke(Expression.Constant(instanceCreator));
            
            expression = WrapWithNullChecker<TService>(expression);

            expression = this.WrapWithPropertyInjector(typeof(TService), typeof(TService), expression);

            expression = this.InterceptInstanceCreation(typeof(TService), typeof(TService), expression);
            
            expression = this.WrapWithInitializer<TService>(expression);

            return expression;
        }

        /// <summary>
        /// Builds an <see cref="Expression"/> that describes the creation of 
        /// <typeparamref name="TImplementation"/>. The returned <see cref="Expression"/> might be intercepted
        /// by a <see cref="SimpleInjector.Container.ExpressionBuilding">Container.ExpressionBuilding</see>
        /// event, and the creation of the <typeparamref name="TImplementation"/> will have been wrapped with
        /// a delegate that executes the registered 
        /// <see cref="SimpleInjector.Container.RegisterInitializer">initializers</see> 
        /// that are appliable to the given <typeparamref name="TService"/> (if any).
        /// </summary>
        /// <typeparam name="TService">The interface or base type that can be used to retrieve instances.</typeparam>
        /// <typeparam name="TImplementation">The concrete type that will be registered.</typeparam>
        /// <returns>An <see cref="Expression"/>.</returns>
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter",
            Justification = @"
                Supplying the generic type arguments is needed, since internal types can not be created using 
                the non-generic overloads in a sandbox.")]
        protected Expression BuildTransientExpression<TService, TImplementation>()
            where TImplementation : class, TService
            where TService : class
        {
            Expression expression = this.BuildNewExpression(typeof(TService), typeof(TImplementation));

            expression = this.WrapWithPropertyInjector(typeof(TService), typeof(TImplementation), expression);

            expression = this.InterceptInstanceCreation(typeof(TService), typeof(TImplementation), expression);

            expression = this.WrapWithInitializer<TImplementation>(expression);

            return this.ReplacePlaceHoldersWithOverriddenParameters(expression);
        }

        private Action<object> BuildInstanceInitializer()
        {
            Type type = this.ImplementationType;

            var parameter = Expression.Parameter(typeof(object));

            var castedParameter = Expression.Convert(parameter, type);

            Expression expression = castedParameter;

            expression = this.WrapWithPropertyInjector(type, type, castedParameter);

            expression = this.InterceptInstanceCreation(type, type, expression);

            // NOTE: We can't wrap with the instance created callback, since the InitializeInstance is called
            // directly by a user.
            expression = this.WrapWithInitializer(type, type, expression);

            if (expression != castedParameter)
            {
                return Expression.Lambda<Action<object>>(expression, parameter).Compile();
            }

            // In this case, no properties and initializers have been applied and the expression wasn't
            // intercepted. Instead of compiling an empty delegate down, we simply return a NoOp.
            return NoOp;
        }

        private Expression BuildNewExpression(Type serviceType, Type implementationType)
        {
            var resolutionBehavior = this.Container.Options.ConstructorResolutionBehavior;

            ConstructorInfo constructor = resolutionBehavior.GetConstructor(serviceType, implementationType);

            this.AddConstructorParametersAsKnownRelationship(constructor);

            return Expression.New(constructor, this.BuildConstructorParameters(constructor));
        }

        private Expression[] BuildConstructorParameters(ConstructorInfo constructor)
        {
            // NOTE: We used to use a LINQ query here (which is cleaner code), but we reverted back to using
            // a foreach statement to clean up the stack trace, since this is a very common code path to
            // show up in the stack trace and preventing showing up the Enumerable and Buffer`1 calls here
            // makes it easier for developers (and maintainers) to read the stack trace.
            var parameters = new List<Expression>();

            foreach (var parameter in constructor.GetParameters())
            {
                var overriddenExpression = this.GetOverriddenParameterFor(parameter).PlaceHolder;

                parameters.Add(overriddenExpression ?? this.BuildParameterExpressionFor(parameter));
            }

            return parameters.ToArray();
        }

        private Expression WrapWithPropertyInjectorInternal(Type serviceType, Type implementationType, 
            Expression expression)
        {
            var properties = this.GetPropertiesToInject(serviceType, implementationType);

            if (properties.Any())
            {
                PropertyInjectionHelper.VerifyProperties(properties);

                expression = PropertyInjectionHelper.BuildPropertyInjectionExpression(this.Container,
                    this.ImplementationType, properties, expression);

                this.AddPropertiesAsKnownRelationships(implementationType, properties);
            }

            return expression;
        }

        private PropertyInfo[] GetPropertiesToInject(Type serviceType, Type implementationType)
        {
            var propertySelector = this.Container.Options.PropertySelectionBehavior;

            var candidates = PropertyInjectionHelper.GetCandidateInjectionPropertiesFor(implementationType);

            return (
                from property in candidates
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
            if (this.overriddenParameters != null && this.overriddenParameters.ContainsKey(parameter))
            {
                return this.overriddenParameters[parameter];
            }

            return new OverriddenParameter();
        }

        private void AddConstructorParametersAsKnownRelationship(ConstructorInfo constructor)
        {
            // We have to suppress the overridden parameter since this might result in a wrong relationship.
            var dependencyTypes = 
                from parameter in constructor.GetParameters()
                let overriddenProducer = this.GetOverriddenParameterFor(parameter).Producer
                let instanceProducer = 
                    overriddenProducer ?? this.Container.GetRegistrationEvenIfInvalid(parameter.ParameterType)
                where instanceProducer != null
                select instanceProducer;

            this.AddRelationships(constructor.DeclaringType, dependencyTypes);
        }

        private void AddPropertiesAsKnownRelationships(Type implementationType, 
            IEnumerable<PropertyInfo> properties)
        {
            var dependencies = 
                from dependencyType in properties.Select(p => p.PropertyType)
                let instanceProducer = this.Container.GetRegistrationEvenIfInvalid(dependencyType)
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

        private Expression BuildParameterExpressionFor(ParameterInfo parameter)
        {
            var injectionBehavior = this.Container.Options.ConstructorInjectionBehavior;

            var expression = injectionBehavior.BuildParameterExpression(parameter);

            if (expression == null)
            {
                throw new ActivationException(
                    StringResources.ConstructorInjectionBehaviorReturnedNull(injectionBehavior, parameter));
            }

            return expression;
        }

        private static Expression WrapWithNullChecker<TService>(Expression expression)
        {
            Func<TService, TService> nullChecker = ThrowWhenNull<TService>;

            return Expression.Invoke(Expression.Constant(nullChecker), expression);
        }

        private Expression WrapWithInitializer<TImplementation>(Expression expression)
            where TImplementation : class
        {
            var context = new InitializationContext(this.GetCurrentProducer(), this);

            Action<TImplementation> instanceInitializer = this.Container.GetInitializer<TImplementation>(context);

            if (instanceInitializer != null)
            {
                // It's not possible to return a Expression that is as heavily optimized as the newExpression
                // simply is, because the instance initializer must be called as well.
                return BuildExpressionWithInstanceInitializer<TImplementation>(expression, instanceInitializer);
            }

            return expression;
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

            try
            {
                return Expression.Invoke(Expression.Constant(instanceCreatorWithInitializer), newExpression);
            }
            catch (Exception ex)
            {
                throw new ActivationException(
                    StringResources.TheInitializersCouldNotBeApplied(typeof(TImplementation), ex), ex);
            }
        }

        private static Func<TService> BuildDelegate<TService>(Expression expression)
            where TService : class
        {
            try
            {
                var newInstanceMethod =
                    Expression.Lambda<Func<TService>>(expression, new ParameterExpression[0]);

                // We can't optimize this delegate using Helpers.CompileAndRun, since we don't have the ability
                // to return the created instance.
                return newInstanceMethod.Compile();
            }
            catch (Exception ex)
            {
                string message = StringResources.ErrorWhileBuildingDelegateFromExpression(
                    typeof(TService), expression, ex);

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

        private void SetCurrentProducer(InstanceProducer producer)
        {
            this.currentProducer.Value = producer;
        }

        private void ResetCurrentProducer()
        {
            this.currentProducer.Value = null;
        }
    }
}