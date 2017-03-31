#region Copyright Simple Injector Contributors
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
    using System.Linq.Expressions;
    using System.Reflection;
    using SimpleInjector.Advanced;
    using SimpleInjector.Lifestyles;

#if !PUBLISH
    /// <summary>Methods for registration.</summary>
#endif
    public partial class Container
    {
        /// <summary>
        /// Occurs when an instance of a type is requested that has not been registered explicitly, allowing 
        /// resolution of unregistered types before the container tries to create the type.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The <see cref="ResolveUnregisteredType"/> event is called by the container every time an 
        /// unregistered type is requested for the first time, allowing a developer to do unregistered type 
        /// resolution. By calling the 
        /// <see cref="UnregisteredTypeEventArgs.Register(Registration)">Register</see> method on the
        /// <see cref="UnregisteredTypeEventArgs"/>, a <see cref="Registration"/>, <see cref="Expression"/> or
        /// <see cref="Func{TResult}"/> delegate can be registered allowing the container to retrieve 
        /// instances of the requested type. This registration is cached and it prevents the 
        /// <b>ResolveUnregisteredType</b> event from being called again for the same type.
        /// </para>
        /// <para>
        /// When no registered event handled the registration of an unregistered type, the container will try
        /// to create the type when this type is either concrete or is the <see cref="IEnumerable{T}"/>
        /// interface. Concrete types will be registered with the <see cref="Lifestyle.Transient">Transient</see>
        /// lifestyle and <see cref="IEnumerable{T}"/> registrations will return an empty collection. When no 
        /// even handled the registration and the container could not create it, an exception is thrown.
        /// </para>
        /// <para>
        /// <b>Thread-safety:</b> Please note that the container will not ensure that the hooked delegates
        /// are executed only once. While the calls to <see cref="ResolveUnregisteredType" /> for a given type
        /// are finite (and will in most cases happen just once), a container can call the delegate multiple 
        /// times and make parallel calls to the delegate. You must make sure that the code can be called 
        /// multiple times and is thread-safe.
        /// </para>
        /// </remarks>
        /// <example>
        /// The following example shows the usage of the <see cref="ResolveUnregisteredType" /> event:
        /// <code lang="cs"><![CDATA[
        /// public interface IValidator<T>
        /// {
        ///     void Validate(T instance);
        /// }
        ///
        /// // Implementation of the null object pattern.
        /// public class EmptyValidator<T> : IValidator<T>
        /// {
        ///     public void Validate(T instance)
        ///     {
        ///         // Does nothing.
        ///     }
        /// }
        /// 
        /// [TestMethod]
        /// public void TestResolveUnregisteredType()
        /// {
        ///     // Arrange
        ///     var container = new Container();
        /// 
        ///     // Register an EmptyValidator<T> to be returned when a IValidator<T> is requested:
        ///     container.ResolveUnregisteredType += (sender, e) =>
        ///     {
        ///         if (e.UnregisteredServiceType.IsGenericType &&
        ///             e.UnregisteredServiceType.GetGenericTypeDefinition() == typeof(IValidator<>))
        ///         {
        ///             var validatorType = typeof(EmptyValidator<>).MakeGenericType(
        ///                 e.UnregisteredServiceType.GetGenericArguments());
        ///     
        ///             object emptyValidator = container.GetInstance(validatorType);
        ///     
        ///             // Register the instance as singleton.
        ///             e.Register(() => emptyValidator);
        ///         }
        ///     };
        ///     
        ///     // Act
        ///     var orderValidator = container.GetInstance<IValidator<Order>>();
        ///     var customerValidator = container.GetInstance<IValidator<Customer>>();
        /// 
        ///     // Assert
        ///     Assert.IsInstanceOfType(orderValidator, typeof(EmptyValidator<Order>));
        ///     Assert.IsInstanceOfType(customerValidator, typeof(EmptyValidator<Customer>));
        /// }
        /// ]]></code>
        /// <para>
        /// The example above registers a delegate that is raised every time an unregistered type is requested
        /// from the container. The delegate checks whether the requested type is a closed generic
        /// implementation of the <b>IValidator&lt;T&gt;</b> interface (such as 
        /// <b>IValidator&lt;Order&gt;</b> or <b>IValidator&lt;Customer&gt;</b>). In that case it
        /// will request the container for a concrete <b>EmptyValidator&lt;T&gt;</b> implementation that
        /// implements the given 
        /// <see cref="UnregisteredTypeEventArgs.UnregisteredServiceType">UnregisteredServiceType</see>, and
        /// registers a delegate that will return this created instance. The <b>e.Register</b> call
        /// registers the method in the container, preventing the <see cref="ResolveUnregisteredType"/> from
        /// being called again for the exact same service type, preventing any performance penalties.
        /// </para>
        /// <para>
        /// Please note that given example is just an uhhmm... example. In the case of the example the
        /// <b>EmptyValidator&lt;T&gt;</b> can be better registered using of the built-in 
        /// <see cref="Register(System.Type, System.Type, Lifestyle)">Register</see> methods instead. These methods take 
        /// care of any given generic type constraint and allow the implementation to be integrated into the 
        /// container's pipeline, which allows it to be intercepted using the <see cref="ExpressionBuilding"/> 
        /// event and allow any registered <see cref="RegisterInitializer{TService}">initializers</see> to be 
        /// applied.
        /// </para>
        /// </example>
        public event EventHandler<UnregisteredTypeEventArgs> ResolveUnregisteredType
        {
            add
            {
                this.ThrowWhenContainerIsLockedOrDisposed();

                this.resolveUnregisteredType += value;
            }

            remove
            {
                this.ThrowWhenContainerIsLockedOrDisposed();

                this.resolveUnregisteredType -= value;
            }
        }

        /// <summary>
        /// Occurs after the creation of the <see cref="Expression" /> of a registered type is complete (the 
        /// lifestyle has been applied), allowing the created <see cref="Expression" /> to be wrapped, 
        /// changed, or replaced. Multiple delegates may handle the same service type.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The <b>ExpressionBuilt</b> event is called by the container every time an registered type is 
        /// getting compiled, allowing a developer to change the way the type is created. The delegate that
        /// hooks to the <b>ExpressionBuilt</b> event, can change the 
        /// <see cref="ExpressionBuiltEventArgs.Expression" /> property on the 
        /// <see cref="ExpressionBuiltEventArgs"/>, which allows changing the way the type is constructed.
        /// </para>
        /// <para>
        /// <b>Thread-safety:</b> Please note that the container will not ensure that the hooked delegates
        /// are executed only once per service type. While the calls to <see cref="ExpressionBuilt" /> for a 
        /// given type are finite (and will in most cases happen just once), a container can call the delegate 
        /// multiple times and make parallel calls to the delegate. You must make sure that the code can be 
        /// called multiple times and is thread-safe.
        /// </para>
        /// </remarks>
        /// <example>
        /// The following example shows the usage of the <b>ExpressionBuilt</b> event:
        /// <code lang="cs"><![CDATA[
        /// public interface IValidator<T>
        /// {
        ///     void Validate(T instance);
        /// }
        ///
        /// public interface ILogger
        /// {
        ///     void Write(string message);
        /// }
        ///
        /// // Implementation of the decorator pattern.
        /// public class MonitoringValidator<T> : IValidator<T>
        /// {
        ///     private readonly IValidator<T> validator;
        ///     private readonly ILogger logger;
        ///
        ///     public MonitoringValidator(IValidator<T> validator, ILogger logger)
        ///     {
        ///         this.validator = validator;
        ///         this.logger = logger;
        ///     }
        ///
        ///     public void Validate(T instance)
        ///     {
        ///         this.logger.Write("Validating " + typeof(T).Name);
        ///         this.validator.Validate(instance);
        ///         this.logger.Write("Validated " + typeof(T).Name);
        ///     }
        /// }
        ///
        /// [TestMethod]
        /// public void TestExpressionBuilt()
        /// {
        ///     // Arrange
        ///     var container = new Container();
        ///
        ///     container.Register<ILogger, ConsoleLogger>(Lifestyle.Singleton);
        ///     container.Register<IValidator<Order>, OrderValidator>();
        ///     container.Register<IValidator<Customer>, CustomerValidator>();
        ///
        ///     // Intercept the creation of IValidator<T> instances and wrap them in a MonitoringValidator<T>:
        ///     container.ExpressionBuilt += (sender, e) =>
        ///     {
        ///         if (e.RegisteredServiceType.IsGenericType &&
        ///             e.RegisteredServiceType.GetGenericTypeDefinition() == typeof(IValidator<>))
        ///         {
        ///             var decoratorType = typeof(MonitoringValidator<>)
        ///                 .MakeGenericType(e.RegisteredServiceType.GetGenericArguments());
        ///
        ///             // Wrap the IValidator<T> in a MonitoringValidator<T>.
        ///             e.Expression = Expression.New(decoratorType.GetConstructors()[0], new Expression[]
        ///             {
        ///                 e.Expression,
        ///                 container.GetRegistration(typeof(ILogger)).BuildExpression(),
        ///             });
        ///         }
        ///     };
        ///
        ///     // Act
        ///     var orderValidator = container.GetInstance<IValidator<Order>>();
        ///     var customerValidator = container.GetInstance<IValidator<Customer>>();
        ///
        ///     // Assert
        ///     Assert.IsInstanceOfType(orderValidator, typeof(MonitoringValidator<Order>));
        ///     Assert.IsInstanceOfType(customerValidator, typeof(MonitoringValidator<Customer>));
        /// }
        /// ]]></code>
        /// <para>
        /// The example above registers a delegate that is raised every time the container compiles the
        /// expression for an registered type. The delegate checks whether the requested type is a closed generic
        /// implementation of the <b>IValidator&lt;T&gt;</b> interface (such as 
        /// <b>IValidator&lt;Order&gt;</b> or <b>IValidator&lt;Customer&gt;</b>). In that case it
        /// will changes the current <see cref="ExpressionBuiltEventArgs.Expression"/> with a new one that creates
        /// a new <b>MonitoringValidator&lt;T&gt;</b> that takes the current validator (and an <b>ILogger</b>)
        /// as an dependency.
        /// </para>
        /// <para>
        /// Please note that given example is just an uhhmm... example. In the case of the example the
        /// <b>MonitoringValidator&lt;T&gt;</b> is a decorator and instead of manually writing this code that
        /// many limitations, you can use one of the built-in 
        /// <see cref="Container.RegisterDecorator(System.Type, System.Type, Lifestyle)">RegisterDecorator</see> methods instead.
        /// These extension methods take care of any given generic type constraint, allow to register decorators
        /// conditionally and allow the decorator to be integrated into the container's pipeline, which allows
        /// it to be intercepted using the <see cref="ExpressionBuilding"/> event and allow any registered
        /// <see cref="RegisterInitializer{TService}">initializers</see> to be applied.
        /// </para>
        /// </example>
        public event EventHandler<ExpressionBuiltEventArgs> ExpressionBuilt
        {
            add
            {
                this.ThrowWhenContainerIsLockedOrDisposed();

                this.expressionBuilt += value;
            }

            remove
            {
                this.ThrowWhenContainerIsLockedOrDisposed();

                this.expressionBuilt -= value;
            }
        }

        /// <summary>
        /// Occurs directly after the creation of the <see cref="Expression" /> of a registered type is made,
        /// but before any <see cref="RegisterInitializer">initializer</see> and lifestyle specific caching
        /// has been applied, allowing the created <see cref="Expression" /> to be altered. Multiple delegates 
        /// may handle the same service type.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The <b>ExpressionBuilding</b> event is called by the container every time an registered type is 
        /// getting compiled, allowing a developer to change the way the type is created. The delegate that
        /// hooks to the <b>ExpressionBuilding</b> event, can change the 
        /// <see cref="ExpressionBuildingEventArgs.Expression" /> property on the 
        /// <see cref="ExpressionBuildingEventArgs"/>, which allows changing the way the type is constructed.
        /// </para>
        /// <para>
        /// The exact <see cref="Expression"/> type supplied depends on the type of registration. 
        /// Registrations that explicitly supply the implementation type (such as 
        /// <see cref="Register{TService, TImplementation}()">Register&lt;TService, TImplementation&gt;()</see>)
        /// will result in an <see cref="NewExpression"/>, while registrations that take a delegate (such as
        /// <see cref="Register{TService}(Func{TService})">Register&lt;TService&gt;(Func&lt;TService&gt;)</see>)
        /// will result in an <see cref="InvocationExpression"/>. Singletons that are passed in using their
        /// value (<see cref="RegisterSingleton{TService}(TService)">RegisterSingleton&lt;TService&gt;(TService)</see>)
        /// will result in an <see cref="ConstantExpression"/>. Note that other <b>ExpressionBuilding</b> 
        /// registrations might have changed the <see cref="ExpressionBuildingEventArgs.Expression" /> 
        /// property and might have supplied an <see cref="Expression"/> of a different type. The order in
        /// which these events are registered might be of importance to you.
        /// </para>
        /// <para>
        /// <b>Thread-safety:</b> Please note that the container will not ensure that the hooked delegates
        /// are executed only once per service type. While the calls to registered <b>ExpressionBuilding</b>
        /// events for a  given type are finite (and will in most cases happen just once), a container can 
        /// call the delegate multiple times and make parallel calls to the delegate. You must make sure that 
        /// the code can be called multiple times and is thread-safe.
        /// </para>
        /// </remarks>
        /// <example>
        /// The following example shows the usage of the <b>ExpressionBuilding</b> event:
        /// <code lang="cs"><![CDATA[
        /// public class MyInjectPropertyAttribute : Attribute { }
        /// 
        /// public static void Bootstrap()
        /// {
        ///     var container = new Container();
        ///     
        ///     container.ExpressionBuilding += (sender, e) =>
        ///     {
        ///         var expression = e.Expression as NewExpression;
        ///     
        ///         if (expression != null)
        ///         {
        ///             var propertiesToInject =
        ///                 from property in expression.Constructor.DeclaringType.GetProperties()
        ///                 where property.GetCustomAttributes(typeof(MyInjectPropertyAttribute), true).Any()
        ///                 let registration = container.GetRegistration(property.PropertyType, true)
        ///                 select Tuple.Create(property, registration);
        ///     
        ///             if (propertiesToInject.Any())
        ///             {
        ///                 Func<object, Tuple<PropertyInfo, InstanceProducer>[], object> injectorDelegate =
        ///                     (instance, dependencies) =>
        ///                     {
        ///                         foreach (var dependency in dependencies)
        ///                         {
        ///                             dependency.Item1.SetValue(instance, dependency.Item2.GetInstance(), null);
        ///                         }
        ///     
        ///                         return instance;
        ///                     };
        ///     
        ///                 e.Expression = Expression.Convert(
        ///                     Expression.Invoke(
        ///                         Expression.Constant(injectorDelegate),
        ///                         e.Expression,
        ///                         Expression.Constant(propertiesToInject.ToArray())),
        ///                     expression.Constructor.DeclaringType);
        ///             }
        ///         }
        ///     };
        /// }
        /// ]]></code>
        /// <para>
        /// The example above registers a delegate that is raised every time the container compiles the
        /// expression for an registered type. The delegate checks if the type contains properties that are
        /// decorated with the supplied <b>MyInjectPropertyAttribute</b>. If decorated properties are found,
        /// the given expression is replaced with an expression that injects decorated properties.
        /// </para>
        /// </example>
        public event EventHandler<ExpressionBuildingEventArgs> ExpressionBuilding
        {
            add
            {
                this.ThrowWhenContainerIsLockedOrDisposed();

                this.expressionBuilding += value;
            }

            remove
            {
                this.ThrowWhenContainerIsLockedOrDisposed();

                this.expressionBuilding -= value;
            }
        }

        // Allows getting notified when Verify() is called.
        // This event is currently only used by RegisterCollection to make sure that as many registered types
        // can be verified.
        internal event Action Verifying = () => { };

        /// <summary>
        /// Registers that a new instance of <typeparamref name="TConcrete"/> will be returned every time it 
        /// is requested (transient).
        /// </summary>
        /// <remarks>
        /// This method uses the container's 
        /// <see cref="ContainerOptions.LifestyleSelectionBehavior">LifestyleSelectionBehavior</see> to select
        /// the exact lifestyle for the specified type. By default this will be 
        /// <see cref="Lifestyle.Transient">Transient</see>.
        /// </remarks>
        /// <typeparam name="TConcrete">The concrete type that will be registered.</typeparam>
        /// <exception cref="InvalidOperationException">
        /// Thrown when this container instance is locked and can not be altered, or when an 
        /// the <typeparamref name="TConcrete"/> has already been registered.
        /// </exception>
        /// <exception cref="ArgumentException">Thrown when the <typeparamref name="TConcrete"/> is a type
        /// that can not be created by the container.</exception>
        public void Register<TConcrete>() where TConcrete : class
        {
            this.Register<TConcrete, TConcrete>(this.SelectionBasedLifestyle, "TConcrete", "TConcrete");
        }

        /// <summary>
        /// Registers that an  instance of <typeparamref name="TConcrete"/> will be returned when it 
        /// is requested. The instance is cached according to the supplied <paramref name="lifestyle"/>.
        /// </summary>
        /// <typeparam name="TConcrete">The concrete type that will be registered.</typeparam>
        /// <param name="lifestyle">The lifestyle that specifies how the returned instance will be cached.</param>
        /// <exception cref="InvalidOperationException">
        /// Thrown when this container instance is locked and can not be altered, or when an 
        /// the <typeparamref name="TConcrete"/> has already been registered.
        /// </exception>
        /// <exception cref="ArgumentException">Thrown when the <typeparamref name="TConcrete"/> is a type
        /// that can not be created by the container.</exception>
        public void Register<TConcrete>(Lifestyle lifestyle) where TConcrete : class
        {
            this.Register<TConcrete, TConcrete>(lifestyle, "TConcrete", "TConcrete");
        }

        /// <summary>
        /// Registers that a new instance of <typeparamref name="TImplementation"/> will be returned every time a
        /// <typeparamref name="TService"/> is requested (transient).
        /// </summary>
        /// <remarks>
        /// This method uses the container's 
        /// <see cref="ContainerOptions.LifestyleSelectionBehavior">LifestyleSelectionBehavior</see> to select
        /// the exact lifestyle for the specified type. By default this will be 
        /// <see cref="Lifestyle.Transient">Transient</see>.
        /// </remarks>
        /// <typeparam name="TService">The interface or base type that can be used to retrieve the instances.</typeparam>
        /// <typeparam name="TImplementation">The concrete type that will be registered.</typeparam>
        /// <exception cref="InvalidOperationException">
        /// Thrown when this container instance is locked and can not be altered, or when an 
        /// the <typeparamref name="TService"/> has already been registered.</exception>
        /// <exception cref="ArgumentException">Thrown when the given <typeparamref name="TImplementation"/> 
        /// type is not a type that can be created by the container.
        /// </exception>
        public void Register<TService, TImplementation>()
            where TImplementation : class, TService
            where TService : class
        {
            this.Register<TService, TImplementation>(this.SelectionBasedLifestyle, "TService", "TImplementation");
        }

        /// <summary>
        /// Registers that an instance of <typeparamref name="TImplementation"/> will be returned when an
        /// instance of type <typeparamref name="TService"/> is requested. The instance is cached according to 
        /// the supplied <paramref name="lifestyle"/>.
        /// </summary>
        /// <typeparam name="TService">The interface or base type that can be used to retrieve the instances.</typeparam>
        /// <typeparam name="TImplementation">The concrete type that will be registered.</typeparam>
        /// <param name="lifestyle">The lifestyle that specifies how the returned instance will be cached.</param>
        /// <exception cref="InvalidOperationException">
        /// Thrown when this container instance is locked and can not be altered, or when an 
        /// the <typeparamref name="TService"/> has already been registered.</exception>
        /// <exception cref="ArgumentException">Thrown when the given <typeparamref name="TImplementation"/> 
        /// type is not a type that can be created by the container.
        /// </exception>
        public void Register<TService, TImplementation>(Lifestyle lifestyle)
            where TImplementation : class, TService
            where TService : class
        {
            this.Register<TService, TImplementation>(lifestyle, "TService", "TImplementation");
        }

        /// <summary>
        /// Registers the specified delegate that allows returning transient instances of 
        /// <typeparamref name="TService"/>. The delegate is expected to always return a new instance on
        /// each call.
        /// </summary>
        /// <remarks>
        /// This method uses the container's 
        /// <see cref="ContainerOptions.LifestyleSelectionBehavior">LifestyleSelectionBehavior</see> to select
        /// the exact lifestyle for the specified type. By default this will be 
        /// <see cref="Lifestyle.Transient">Transient</see>.
        /// </remarks>
        /// <typeparam name="TService">The interface or base type that can be used to retrieve instances.</typeparam>
        /// <param name="instanceCreator">The delegate that allows building or creating new instances.</param>
        /// <exception cref="InvalidOperationException">
        /// Thrown when this container instance is locked and can not be altered, or when the 
        /// <typeparamref name="TService"/> has already been registered.</exception>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="instanceCreator"/> is a null reference.</exception>
        public void Register<TService>(Func<TService> instanceCreator) where TService : class
        {
            this.Register<TService>(instanceCreator, this.SelectionBasedLifestyle);
        }

        /// <summary>
        /// Registers the specified delegate <paramref name="instanceCreator"/> that will produce instances of
        /// type <typeparamref name="TService"/> and will be returned when an instance of type 
        /// <typeparamref name="TService"/> is requested. The delegate is expected to produce new instances on
        /// each call. The instances are cached according to the supplied <paramref name="lifestyle"/>.
        /// </summary>
        /// <typeparam name="TService">The interface or base type that can be used to retrieve instances.</typeparam>
        /// <param name="instanceCreator">The delegate that allows building or creating new instances.</param>
        /// <param name="lifestyle">The lifestyle that specifies how the returned instance will be cached.</param>
        /// <exception cref="InvalidOperationException">
        /// Thrown when this container instance is locked and can not be altered, or when the 
        /// <typeparamref name="TService"/> has already been registered.</exception>
        /// <exception cref="ArgumentNullException">
        /// Thrown when one of the supplied arguments is a null reference (Nothing in VB).</exception>
        public void Register<TService>(Func<TService> instanceCreator, Lifestyle lifestyle)
            where TService : class
        {
            Requires.IsNotNull(instanceCreator, nameof(instanceCreator));
            Requires.IsNotNull(lifestyle, nameof(lifestyle));

            Requires.IsNotAnAmbiguousType(typeof(TService), nameof(TService));

            var registration = lifestyle.CreateRegistration<TService>(instanceCreator, this);

            this.AddRegistration(typeof(TService), registration);
        }

        /// <summary>
        /// Registers that a new instance of <paramref name="concreteType"/> will be returned every time it 
        /// is requested (transient).
        /// </summary>
        /// <remarks>
        /// This method uses the container's 
        /// <see cref="ContainerOptions.LifestyleSelectionBehavior">LifestyleSelectionBehavior</see> to select
        /// the exact lifestyle for the specified type. By default this will be 
        /// <see cref="Lifestyle.Transient">Transient</see>.
        /// </remarks>
        /// <param name="concreteType">The concrete type that will be registered.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="concreteType"/> is a null 
        /// references (Nothing in VB).</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="concreteType"/> represents an 
        /// open generic type or is a type that can not be created by the container.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when this container instance is locked and can not be altered, or when an 
        /// the <paramref name="concreteType"/> has already been registered.
        /// </exception>
        public void Register(Type concreteType)
        {
            this.Register(concreteType, concreteType, this.SelectionBasedLifestyle, nameof(concreteType), nameof(concreteType));
        }

        /// <summary>
        /// Registers that a new instance of <paramref name="implementationType"/> will be returned every time a
        /// <paramref name="serviceType"/> is requested. If <paramref name="serviceType"/> and 
        /// <paramref name="implementationType"/> represent the same type, the type is registered by itself.
        /// Open and closed generic types are supported.
        /// </summary>
        /// <remarks>
        /// This method uses the container's 
        /// <see cref="ContainerOptions.LifestyleSelectionBehavior">LifestyleSelectionBehavior</see> to select
        /// the exact lifestyle for the specified type. By default this will be 
        /// <see cref="Lifestyle.Transient">Transient</see>.
        /// </remarks>
        /// <param name="serviceType">The base type or interface to register. This can be an open-generic type.</param>
        /// <param name="implementationType">The actual type that will be returned when requested. 
        /// This can be an open-generic type.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="serviceType"/> or 
        /// <paramref name="implementationType"/> are null references (Nothing in VB).</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="implementationType"/> is
        /// no sub type from <paramref name="serviceType"/> (or the same type).
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when this container instance is locked and can not be altered, or when an 
        /// the <paramref name="serviceType"/> has already been registered.
        /// </exception>
        public void Register(Type serviceType, Type implementationType)
        {
            this.Register(serviceType, implementationType, this.SelectionBasedLifestyle,
                nameof(serviceType), nameof(implementationType));
        }

        /// <summary>
        /// Registers that an instance of type <paramref name="implementationType"/> will be returned when an
        /// instance of type <paramref name="serviceType"/> is requested. The instance is cached according to 
        /// the supplied <paramref name="lifestyle"/>. Open and closed generic types are supported.
        /// </summary>
        /// <param name="serviceType">The interface or base type that can be used to retrieve the instances.
        /// This can be an open-generic type.</param>
        /// <param name="implementationType">The concrete type that will be registered.
        /// This can be an open-generic type.</param>
        /// <param name="lifestyle">The lifestyle that specifies how the returned instance will be cached.</param>
        /// <exception cref="ArgumentNullException">Thrown when one of the supplied arguments is a null
        /// reference (Nothing in VB).</exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when this container instance is locked and can not be altered, or when the 
        /// <paramref name="serviceType"/> has already been registered.</exception>
        /// <exception cref="ArgumentException">Thrown when the given <paramref name="implementationType"/>
        /// type is not a type that can be created by the container, when either <paramref name="serviceType"/>
        /// or <paramref name="implementationType"/> are open generic types, or when 
        /// <paramref name="serviceType"/> is not assignable from the <paramref name="implementationType"/>.
        /// </exception>
        /// <exception cref="ArgumentNullException">Thrown when one of the supplied arguments is a null
        /// reference (Nothing in VB).</exception>
        public void Register(Type serviceType, Type implementationType, Lifestyle lifestyle)
        {
            this.Register(serviceType, implementationType, lifestyle, nameof(serviceType), nameof(implementationType));
        }

        /// <summary>
        /// Registers the specified delegate that allows returning instances of <paramref name="serviceType"/>.
        /// </summary>
        /// <remarks>
        /// This method uses the container's 
        /// <see cref="ContainerOptions.LifestyleSelectionBehavior">LifestyleSelectionBehavior</see> to select
        /// the exact lifestyle for the specified type. By default this will be 
        /// <see cref="Lifestyle.Transient">Transient</see>.
        /// </remarks>
        /// <param name="serviceType">The base type or interface to register.</param>
        /// <param name="instanceCreator">The delegate that will be used for creating new instances.</param>
        /// <exception cref="ArgumentNullException">Thrown when either <paramref name="serviceType"/> or 
        /// <paramref name="instanceCreator"/> are null references (Nothing in VB).</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="serviceType"/> represents an
        /// open generic type.</exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when this container instance is locked and can not be altered, or when an 
        /// the <paramref name="serviceType"/> has already been registered.
        /// </exception>
        public void Register(Type serviceType, Func<object> instanceCreator)
        {
            this.Register(serviceType, instanceCreator, this.SelectionBasedLifestyle);
        }

        /// <summary>
        /// Registers the specified delegate <paramref name="instanceCreator"/> that will produce instances of
        /// type <paramref name="serviceType"/> and will be returned when an instance of type 
        /// <paramref name="serviceType"/> is requested. The delegate is expected to produce new instances on 
        /// each call. The instances are cached according to the supplied <paramref name="lifestyle"/>.
        /// </summary>
        /// <param name="serviceType">The interface or base type that can be used to retrieve instances.</param>
        /// <param name="instanceCreator">The delegate that allows building or creating new instances.</param>
        /// <param name="lifestyle">The lifestyle that specifies how the returned instance will be cached.</param>
        /// <exception cref="InvalidOperationException">
        /// Thrown when this container instance is locked and can not be altered, or when the 
        /// <paramref name="serviceType"/> has already been registered.</exception>
        /// <exception cref="ArgumentNullException">
        /// Thrown when one of the supplied arguments is a null reference (Nothing in VB).</exception>
        public void Register(Type serviceType, Func<object> instanceCreator, Lifestyle lifestyle)
        {
            Requires.IsNotNull(serviceType, nameof(serviceType));
            Requires.IsNotNull(instanceCreator, nameof(instanceCreator));
            Requires.IsNotNull(lifestyle, nameof(lifestyle));

            Requires.IsReferenceType(serviceType, nameof(serviceType));
            Requires.IsNotOpenGenericType(serviceType, nameof(serviceType));

            Requires.IsNotAnAmbiguousType(serviceType, nameof(serviceType));

            var registration = lifestyle.CreateRegistration(serviceType, instanceCreator, this);

            this.AddRegistration(serviceType, registration);
        }

        /// <summary>
        /// Registers a single instance that will be returned when an instance of type 
        /// <typeparamref name="TService"/> is requested. This <paramref name="instance"/> must be thread-safe
        /// when working in a multi-threaded environment.
        /// <b>NOTE:</b> Do note that instances supplied by this method <b>NEVER</b> get disposed by the
        /// container, since the instance is assumed to outlive this container instance. If disposing is
        /// required, use the overload that accepts a <see cref="Func{T}"/> delegate.
        /// </summary>
        /// <typeparam name="TService">The interface or base type that can be used to retrieve the instance.</typeparam>
        /// <param name="instance">The instance to register.</param>
        /// <exception cref="InvalidOperationException">
        /// Thrown when this container instance is locked and can not be altered, or when the 
        /// <typeparamref name="TService"/> has already been registered.</exception>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="instance"/> is a null reference.
        /// </exception>
        public void RegisterSingleton<TService>(TService instance) where TService : class
        {
            Requires.IsNotNull(instance, nameof(instance));
            Requires.IsNotAnAmbiguousType(typeof(TService), nameof(TService));

            var registration = SingletonLifestyle.CreateSingleInstanceRegistration(typeof(TService), instance, this,
                instance.GetType());

            this.AddRegistration(typeof(TService), registration);
        }

        /// <summary>
        /// Registers a single instance that will be returned when an instance of type 
        /// <paramref name="serviceType"/> is requested. This <paramref name="instance"/> must be thread-safe
        /// when working in a multi-threaded environment.
        /// </summary>
        /// <param name="serviceType">The base type or interface to register.</param>
        /// <param name="instance">The instance to register.</param>
        /// <exception cref="ArgumentNullException">Thrown when either <paramref name="serviceType"/> or 
        /// <paramref name="instance"/> are null references (Nothing in VB).</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="instance"/> is
        /// no sub type from <paramref name="serviceType"/>.</exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when this container instance is locked and can not be altered, or when an 
        /// the <paramref name="serviceType"/> has already been registered.
        /// </exception>
        public void RegisterSingleton(Type serviceType, object instance)
        {
            Requires.IsNotNull(serviceType, nameof(serviceType));
            Requires.IsNotNull(instance, nameof(instance));
            Requires.ServiceIsAssignableFromImplementation(serviceType, instance.GetType(), nameof(serviceType));

            Requires.IsNotAnAmbiguousType(serviceType, nameof(serviceType));

            var registration = SingletonLifestyle.CreateSingleInstanceRegistration(serviceType, instance, this);

            this.AddRegistration(serviceType, registration);
        }
        
        /// <summary>
        /// Registers a single concrete instance that will be constructed using constructor injection and will
        /// be returned when this instance is requested by type <typeparamref name="TConcrete"/>. 
        /// This <typeparamref name="TConcrete"/> must be thread-safe when working in a multi-threaded 
        /// environment.
        /// If <typeparamref name="TConcrete"/> implements <see cref="IDisposable"/>, a created instance will
        /// get disposed when <see cref="Container.Dispose()">Container.Dispose</see> gets called.
        /// </summary>
        /// <typeparam name="TConcrete">The concrete type that will be registered.</typeparam>
        /// <exception cref="InvalidOperationException">
        /// Thrown when this container instance is locked and can not be altered, or when 
        /// <typeparamref name="TConcrete"/> has already been registered.
        /// </exception>
        /// <exception cref="ArgumentException">Thrown when the <typeparamref name="TConcrete"/> is a type
        /// that can not be created by the container.</exception>
        public void RegisterSingleton<TConcrete>() where TConcrete : class
        {
            this.Register<TConcrete, TConcrete>(Lifestyle.Singleton, nameof(TConcrete), nameof(TConcrete));
        }

        /// <summary>
        /// Registers that the same a single instance of type <typeparamref name="TImplementation"/> will be 
        /// returned every time an <typeparamref name="TService"/> type is requested. If 
        /// <typeparamref name="TService"/> and <typeparamref name="TImplementation"/>  represent the same 
        /// type, the type is registered by itself. <typeparamref name="TImplementation"/> must be thread-safe 
        /// when working in a multi-threaded environment.
        /// If <typeparamref name="TImplementation"/> implements <see cref="IDisposable"/>, a created instance will
        /// get disposed when <see cref="Container.Dispose()">Container.Dispose</see> gets called.
        /// </summary>
        /// <typeparam name="TService">
        /// The interface or base type that can be used to retrieve the instances.
        /// </typeparam>
        /// <typeparam name="TImplementation">The concrete type that will be registered.</typeparam>
        /// <exception cref="InvalidOperationException">
        /// Thrown when this container instance is locked and can not be altered, or when the 
        /// <typeparamref name="TService"/> has already been registered.</exception>
        /// <exception cref="ArgumentException">Thrown when the given <typeparamref name="TImplementation"/> 
        /// type is not a type that can be created by the container.
        /// </exception>
        public void RegisterSingleton<TService, TImplementation>()
            where TImplementation : class, TService
            where TService : class
        {
            this.Register<TService, TImplementation>(Lifestyle.Singleton, nameof(TService), nameof(TImplementation));
        }

        /// <summary>
        /// Registers the specified delegate that allows constructing a single instance of 
        /// <typeparamref name="TService"/>. This delegate will be called at most once during the lifetime of 
        /// the application. The returned instance must be thread-safe when working in a multi-threaded 
        /// environment.
        /// If the instance returned from <paramref name="instanceCreator"/> implements <see cref="IDisposable"/>, 
        /// the created instance will get disposed when <see cref="Container.Dispose()">Container.Dispose</see> 
        /// gets called.
        /// </summary>
        /// <typeparam name="TService">The interface or base type that can be used to retrieve instances.</typeparam>
        /// <param name="instanceCreator">The delegate that allows building or creating this single
        /// instance.</param>
        /// <exception cref="InvalidOperationException">
        /// Thrown when this container instance is locked and can not be altered, or when a 
        /// <paramref name="instanceCreator"/> for <typeparamref name="TService"/> has already been registered.
        /// </exception>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="instanceCreator"/> is a 
        /// null reference.</exception>
        public void RegisterSingleton<TService>(Func<TService> instanceCreator) where TService : class
        {
            Requires.IsNotNull(instanceCreator, nameof(instanceCreator));
            Requires.IsNotAnAmbiguousType(typeof(TService), nameof(TService));

            this.Register<TService>(instanceCreator, Lifestyle.Singleton);
        }

        /// <summary>
        /// Registers that the same instance of type <paramref name="implementationType"/> will be returned every 
        /// time an instance of type <paramref name="serviceType"/> type is requested. If 
        /// <paramref name="serviceType"/> and <paramref name="implementationType"/> represent the same type, the 
        /// type is registered by itself. <paramref name="implementationType"/> must be thread-safe when working 
        /// in a multi-threaded environment. Open and closed generic types are supported.
        /// </summary>
        /// <param name="serviceType">The base type or interface to register.
        /// This can be an open-generic type.</param>
        /// <param name="implementationType">The actual type that will be returned when requested.
        /// This can be an open-generic type.</param>
        /// <exception cref="ArgumentNullException">Thrown when either <paramref name="serviceType"/> or 
        /// <paramref name="implementationType"/> are null references (Nothing in VB).</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="implementationType"/> is
        /// no sub type from <paramref name="serviceType"/>, or when one of them represents an open generic
        /// type.</exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when this container instance is locked and can not be altered, or when an 
        /// the <paramref name="serviceType"/> has already been registered.
        /// </exception>
        public void RegisterSingleton(Type serviceType, Type implementationType)
        {
            this.Register(serviceType, implementationType, Lifestyle.Singleton, nameof(serviceType), 
                nameof(implementationType));
        }

        /// <summary>
        /// Registers the specified delegate that allows constructing a single <paramref name="serviceType"/> 
        /// instance. The container will call this delegate at most once during the lifetime of the application.
        /// </summary>
        /// <param name="serviceType">The base type or interface to register.</param>
        /// <param name="instanceCreator">The delegate that will be used for creating that single instance.</param>
        /// <exception cref="ArgumentException">Thrown when <paramref name="serviceType"/> represents an open
        /// generic type.</exception>
        /// <exception cref="ArgumentNullException">Thrown when either <paramref name="serviceType"/> or 
        /// <paramref name="instanceCreator"/> are null references (Nothing in
        /// VB).</exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when this container instance is locked and can not be altered, or when an 
        /// the <paramref name="serviceType"/> has already been registered.
        /// </exception>
        public void RegisterSingleton(Type serviceType, Func<object> instanceCreator)
        {
            this.Register(serviceType, instanceCreator, Lifestyle.Singleton);
        }

        /// <summary>
        /// Registers an <see cref="Action{T}"/> delegate that runs after the creation of instances that
        /// implement or derive from the given <typeparamref name="TService"/>. Please note that only instances
        /// that are created by the container (using constructor injection) can be initialized this way.
        /// </summary>
        /// <typeparam name="TService">The type for which the initializer will be registered.</typeparam>
        /// <param name="instanceInitializer">The delegate that will be called after the instance has been
        /// constructed and before it is returned.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when the <paramref name="instanceInitializer"/> is a null reference.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when this container instance is locked and can not be altered.</exception>
        /// <remarks>
        /// <para>
        /// Multiple <paramref name="instanceInitializer"/> delegates can be registered per 
        /// <typeparamref name="TService"/> and multiple initializers can be applied on a created instance,
        /// before it is returned. For instance, when registering a <paramref name="instanceInitializer"/>
        /// for type <see cref="System.Object"/>, the delegate will be called for every instance created by
        /// the container, which can be nice for debugging purposes.
        /// </para>
        /// <para>
        /// Note: Initializers are guaranteed to be executed in the order they are registered.
        /// </para>
        /// <para>
        /// The following example shows the usage of the 
        /// <see cref="RegisterInitializer{TService}(Action{TService})">RegisterInitializer</see> method:
        /// </para>
        /// <code lang="cs"><![CDATA[
        /// public interface ITimeProvider { DateTime Now { get; } }
        /// public interface ICommand { bool SendAsync { get; set; } }
        /// 
        /// public abstract class CommandBase : ICommand
        /// {
        ///     ITimeProvider Clock { get; set; }
        ///     
        ///     public bool SendAsync { get; set; }
        /// }
        /// 
        /// public class ConcreteCommand : CommandBase { }
        /// 
        /// [TestMethod]
        /// public static void TestRegisterInitializer()
        /// {
        ///     // Arrange
        ///     var container = new Container();
        /// 
        ///     container.Register<ICommand, ConcreteCommand>();
        /// 
        ///     // Configuring property injection for types that implement ICommand:
        ///     container.RegisterInitializer<ICommand>(command =>
        ///     {
        ///         command.SendAsync = true;
        ///     });
        /// 
        ///     // Configuring property injection for types that implement CommandBase:
        ///     container.RegisterInitializer<CommandBase>(command =>
        ///     {
        ///         command.Clock = container.GetInstance<ITimeProvider>();
        ///     });
        ///     
        ///     // Act
        ///     var command = (ConcreteCommand)container.GetInstance<ICommand>();
        /// 
        ///     // Assert
        ///     // Because ConcreteCommand implements both ICommand and CommandBase, 
        ///     // both the initializers will have been executed.
        ///     Assert.IsTrue(command.SendAsync);
        ///     Assert.IsNotNull(command.Clock);
        /// }
        /// ]]></code>
        /// <para>
        /// The container does not use the type information of the requested service type, but it uses the 
        /// type information of the actual implementation to find all initialized that apply for that 
        /// type. This makes it possible to have multiple initializers to be applied on a single returned
        /// instance while keeping performance high.
        /// </para>
        /// <para>
        /// Registered initializers will only be applied to instances that are created by the container self
        /// (using constructor injection). Types that are newed up manually by supplying a 
        /// <see cref="Func{T}"/> delegate to the container (using the 
        /// <see cref="Register{TService}(Func{TService})"/> method) or registered as single instance
        /// (using <see cref="RegisterSingleton{TService}(TService)"/>) will not trigger initialization.
        /// When initialization of these instances is needed, this must be done manually, as can be seen in 
        /// the following example:
        /// <code lang="cs"><![CDATA[
        /// [TestMethod]
        /// public static void TestRegisterInitializer()
        /// {
        ///     // Arrange
        ///     int initializerCallCount = 0;
        ///     
        ///     var container = new Container();
        ///     
        ///     // Define a initializer for ICommand
        ///     Action<ICommand> commandInitializer = command =>
        ///     {
        ///         initializerCallCount++;
        ///     });
        ///     
        ///     // Configuring that initializer.
        ///     container.RegisterInitializer<ICommand>(commandInitializer);
        ///     
        ///     container.Register<ICommand>(() =>
        ///     {
        ///         // Create a ConcreteCommand manually: will not be initialized.
        ///         var command = new ConcreteCommand("Data Source=.;Initial Catalog=db;");
        ///     
        ///         // Run the initializer manually.
        ///         commandInitializer(command);
        ///     
        ///         return command;
        ///     });
        ///     
        ///     // Act
        ///     var command = container.GetInstance<ICommand>();
        /// 
        ///     // Assert
        ///     // The initializer will only be called once.
        ///     Assert.AreEqual(1, initializerCallCount);
        /// }
        /// ]]></code>
        /// The previous example shows how a manually created instance can still be initialized. Try to
        /// prevent creating types manually, by changing the design of those classes. If possible, create a
        /// single public constructor that only contains dependencies that can be resolved.
        /// </para>
        /// </remarks>
        public void RegisterInitializer<TService>(Action<TService> instanceInitializer) where TService : class
        {
            Requires.IsNotNull(instanceInitializer, nameof(instanceInitializer));

            this.ThrowWhenContainerIsLockedOrDisposed();

            this.instanceInitializers.Add(TypedInstanceInitializer.Create(instanceInitializer));
        }

        /// <summary>
        /// Registers an <see cref="Action{InstanceInitializationData}"/> delegate that runs after the 
        /// creation of instances for which the supplied <paramref name="predicate"/> returns true. Please 
        /// note that only instances that are created by the container can be initialized this way.
        /// </summary>
        /// <param name="instanceInitializer">The delegate that will be called after the instance has been
        /// constructed and before it is returned.</param>
        /// <param name="predicate">The predicate that will be used to check whether the given delegate must
        /// be applied to a registration or not. The given predicate will be called once for each registration
        /// in the container.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when either the <paramref name="instanceInitializer"/> or <paramref name="predicate"/> are 
        /// null references.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when this container instance is locked and can not be altered.</exception>
        /// <remarks>
        /// <para>
        /// Note: Initializers are guaranteed to be executed in the order they are registered.
        /// </para>
        /// <para>
        /// Note: The <paramref name="predicate"/> is <b>not</b> guaranteed to be called once per registration;
        /// when a registration's instance is requested for the first time simultaneously over multiple thread,
        /// the predicate might be called multiple times. The caller of this method is responsible of supplying
        /// a predicate that is thread-safe.
        /// </para>
        /// </remarks>
        public void RegisterInitializer(Action<InstanceInitializationData> instanceInitializer,
            Predicate<InitializerContext> predicate)
        {
            Requires.IsNotNull(instanceInitializer, nameof(instanceInitializer));
            Requires.IsNotNull(predicate, nameof(predicate));

            this.ThrowWhenContainerIsLockedOrDisposed();

            this.instanceInitializers.Add(ContextualInstanceInitializer.Create(instanceInitializer, predicate));
        }
        
        /// <summary>
        /// Adds the <paramref name="registration"/> for the supplied <paramtyperef name="TService"/>. This
        /// method can be used to apply the same <see cref="Registration"/> to multiple different service
        /// types.
        /// </summary>
        /// <typeparam name="TService">The base type or interface to register.</typeparam>
        /// <param name="registration">The registration that should be stored for the given 
        /// <paramtyperef name="TService"/>.</param>
        /// <exception cref="ArgumentNullException">Thrown when one of the supplied arguments is a null
        /// reference (Nothing in VB).</exception>
        /// <exception cref="ArgumentException">Thrown when <paramtyperef name="TService"/> is ambiguous, 
        /// when it is not assignable from the 
        /// <paramref name="registration"/>'s <see cref="Registration.ImplementationType">ImplementationType</see>
        /// or when the supplied <paramref name="registration"/> is created for a different 
        /// <see cref="Container"/> instance.</exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when this container instance is locked and can not be altered, or when an 
        /// the <paramtyperef name="TService"/> has already been registered.
        /// </exception>
        public void AddRegistration<TService>(Registration registration) where TService : class
        {
            Requires.IsNotNull(registration, nameof(registration));

            this.AddRegistrationInternal(typeof(TService), registration);
        }

        /// <summary>
        /// Adds the <paramref name="registration"/> for the supplied <paramref name="serviceType"/>. This
        /// method can be used to apply the same <see cref="Registration"/> to multiple different service
        /// types.
        /// </summary>
        /// <param name="serviceType">The base type or interface to register.</param>
        /// <param name="registration">The registration that should be stored for the given 
        /// <paramref name="serviceType"/>.</param>
        /// <example>
        /// <code lang="cs"><![CDATA[
        /// public interface IFoo { }
        /// public interface IBar { }
        /// public class FooBar : IFoo, IBar { }
        /// 
        /// public void AddRegistration_SuppliedWithSameSingletonRegistrationTwice_ReturnsSameInstance()
        /// {
        ///     // Arrange
        ///     Registration registration =
        ///         Lifestyle.Singleton.CreateRegistration<FooBar, FooBar>(container);
        /// 
        ///     container.AddRegistration(typeof(IFoo), registration);
        ///     container.AddRegistration(typeof(IBar), registration);
        /// 
        ///     // Act
        ///     IFoo foo = container.GetInstance<IFoo>();
        ///     IBar bar  = container.GetInstance<IBar>();
        /// 
        ///     // Assert
        ///     bool fooAndBareAreTheSameInstance = object.ReferenceEquals(foo, bar);
        ///     Assert.IsTrue(fooAndBareAreTheSameInstance);
        /// }
        /// ]]></code>
        /// <para>
        /// In the example above a singleton registration is created for type <c>FooBar</c> and this 
        /// registration is added to the container for each interface (<c>IFoo</c> and <c>IBar</c>) that it
        /// implements. Since both services use the same singleton registration, requesting those services 
        /// will result in the return of the same (singleton) instance.
        /// </para>
        /// <para>
        /// <see cref="ExpressionBuilding"/> events are applied to the <see cref="Expression"/> of the
        /// <see cref="Registration"/> instance and are therefore applied once. <see cref="ExpressionBuilt"/> 
        /// events on the other hand get applied to the <b>Expression</b> of the <see cref="InstanceProducer"/>.
        /// Since each <b>AddRegistration</b> gets its own instance producer (that wraps the 
        /// <b>Registration</b> instance), this means that the <b>ExpressionBuilt</b> events will be 
        /// applied for each registered service type.
        /// </para>
        /// <para>
        /// The most practical example of this is the use of decorators using one of the 
        /// <see cref="RegisterDecorator(System.Type, System.Type)">RegisterDecorator</see> overloads (decorator registration 
        /// use the <b>ExpressionBuilt</b> event under the covers). Take a look at the following example:
        /// </para>
        /// <code lang="cs"><![CDATA[
        /// public interface IFoo { }
        /// public interface IBar { }
        /// public class FooBar : IFoo, IBar { }
        /// 
        /// public class BarDecorator : IBar
        /// {
        ///     public BarDecorator(IBar decoratedBar)
        ///     {
        ///         this.DecoratedBar = decoratedBar;
        ///     }
        ///     
        ///     public IBar DecoratedBar { get; private set; }
        /// }
        /// 
        /// public void AddRegistration_SameSingletonRegistrationTwiceAndOneDecoratorApplied_ReturnsSameInstance()
        /// {
        ///     // Arrange
        ///     Registration registration =
        ///         Lifestyle.Singleton.CreateRegistration<FooBar, FooBar>(container);
        /// 
        ///     container.AddRegistration(typeof(IFoo), registration);
        ///     container.AddRegistration(typeof(IBar), registration);
        ///     
        ///     // Registere a decorator for IBar, but not for IFoo
        ///     container.RegisterDecorator(typeof(IBar), typeof(BarDecorator));
        /// 
        ///     // Act
        ///     var foo = container.GetInstance<IFoo>();
        ///     var decorator = container.GetInstance<IBar>() as BarDecorator;
        ///     var bar = decorator.DecoratedBar;
        /// 
        ///     // Assert
        ///     bool fooAndBareAreTheSameInstance = object.ReferenceEquals(foo, bar);
        ///     Assert.IsTrue(fooAndBareAreTheSameInstance);
        /// }
        /// ]]></code>
        /// The example shows that the decorator gets applied to <c>IBar</c> but not to <c>IFoo</c>, but that
        /// the decorated <c>IBar</c> is still the same instance as the resolved <c>IFoo</c> instance.
        /// </example>
        /// <exception cref="ArgumentNullException">Thrown when one of the supplied arguments is a null
        /// reference (Nothing in VB).</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="serviceType"/> is not a reference
        /// type, is open generic, is ambiguous, when it is not assignable from the 
        /// <paramref name="registration"/>'s <see cref="Registration.ImplementationType">ImplementationType</see>
        /// or when the supplied <paramref name="registration"/> is created for a different 
        /// <see cref="Container"/> instance.</exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when this container instance is locked and can not be altered, or when an 
        /// the <paramref name="serviceType"/> has already been registered.
        /// </exception>
        public void AddRegistration(Type serviceType, Registration registration)
        {
            Requires.IsNotNull(serviceType, nameof(serviceType));
            Requires.IsNotNull(registration, nameof(registration));
            Requires.IsNotOpenGenericType(serviceType, nameof(serviceType));

            this.AddRegistrationInternal(serviceType, registration);
        }

        internal void RegisterExternalProducer(InstanceProducer producer)
        {
            lock (this.externalProducers)
            {
                this.externalProducers.Add(producer);
            }
        }

        internal void RemoveExternalProducer(InstanceProducer producer)
        {
            lock (this.externalProducers)
            {
                this.externalProducers.Remove(producer);
            }
        }

        internal void RegisterResolveInterceptor(ResolveInterceptor interceptor,
            Predicate<InitializationContext> predicate)
        {
            this.resolveInterceptors.Add(new ContextualResolveInterceptor(interceptor, predicate));
        }

        private void Register<TService, TImplementation>(Lifestyle lifestyle, string serviceTypeParamName,
            string implementationTypeParamName)
            where TImplementation : class, TService
            where TService : class
        {
            Requires.IsNotNull(lifestyle, nameof(lifestyle));

            Requires.IsNotAnAmbiguousType(typeof(TService), serviceTypeParamName);

            this.ThrowArgumentExceptionWhenTypeIsNotConstructable(typeof(TImplementation),
                implementationTypeParamName);

            var registration = lifestyle.CreateRegistration<TImplementation>(this);

            this.AddRegistrationInternal(typeof(TService), registration);
        }

        private void Register(Type serviceType, Type implementationType, Lifestyle lifestyle,
            string serviceTypeParamName, string implementationTypeParamName)
        {
            Requires.IsNotNull(serviceType, serviceTypeParamName);
            Requires.IsNotNull(implementationType, implementationTypeParamName);
            Requires.IsNotNull(lifestyle, nameof(lifestyle));

            Requires.IsReferenceType(serviceType, serviceTypeParamName);
            Requires.IsReferenceType(implementationType, implementationTypeParamName);
            
            Requires.IsNotAnAmbiguousType(serviceType, serviceTypeParamName);

            this.ThrowArgumentExceptionWhenTypeIsNotConstructable(implementationType,
                implementationTypeParamName);

            if (serviceType.ContainsGenericParameters())
            {
                this.RegisterOpenGeneric(serviceType, implementationType, lifestyle);
            }
            else
            {
                Requires.ServiceIsAssignableFromImplementation(serviceType, implementationType,
                    implementationTypeParamName);

                var registration = lifestyle.CreateRegistration(implementationType, this);

                this.AddRegistrationInternal(serviceType, registration);
            }
        }

        private void AddRegistrationInternal(Type serviceType, Registration registration)
        {
            Requires.IsRegistrationForThisContainer(this, registration, nameof(registration));

            this.ThrowWhenContainerIsLockedOrDisposed();

            this.AddInstanceProducer(new InstanceProducer(serviceType, registration));
        }

        private void ThrowArgumentExceptionWhenTypeIsNotConstructable(
            Type implementationType, string parameterName)
        {
            string message;

            bool constructable = this.Options.IsConstructableType(implementationType, out message);

            if (!constructable)
            {
                // After some doubt (and even after reading https://stackoverflow.com/questions/1412598/)
                // I decided to throw an ArgumentException when the given generic type argument was 
                // invalid. Mainly because a generic type argument is just an argument, and 
                // ArgumentException even allows us to supply the name of the argument. No developer 
                // will be surprised to see an ArgumentException in this case.
                throw new ArgumentException(message, parameterName);
            }
        }
        
        private sealed class ContainerVerificationScope : Scope
        {
            public ContainerVerificationScope(Container container) : base(container)
            {
            }

            public override void WhenScopeEnds(Action action)
            {
                // No-op.
            }
        }
    }
}