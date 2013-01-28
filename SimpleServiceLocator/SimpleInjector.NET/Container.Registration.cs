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

namespace SimpleInjector
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Linq.Expressions;

    using SimpleInjector.Advanced;
    using SimpleInjector.Extensions.Decorators;
    using SimpleInjector.Lifestyles;

#if DEBUG
    /// <summary>
    /// Methods for registration.
    /// </summary>
#endif
    public partial class Container
    {
        /// <summary>
        /// Occurs when an instance of a type is requested that has not been registered, allowing resolution
        /// of unregistered types.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The <see cref="ResolveUnregisteredType"/> event is called by the container every time an 
        /// unregistered type is requested, allowing a developer to do unregistered type resolution. By calling the 
        /// <see cref="UnregisteredTypeEventArgs.Register(Func{object})">Register</see> method on the
        /// <see cref="UnregisteredTypeEventArgs"/>, a delegate can be hooked to the container allowing the
        /// container to retrieve instances of the requested type, and preventing the 
        /// <see cref="ResolveUnregisteredType"/> event from being called again for the same type.
        /// </para>
        /// <para>
        /// This event is called before resolving concrete unregistered types, allowing a developer to
        /// intercept the creation of concrete types.
        /// </para>
        /// <para>
        /// <b>Thread-safety:</b> Please note that the container will not ensure that the hooked delegates
        /// are executed only once. While the calls to <see cref="ResolveUnregisteredType" /> for a given type
        /// are finite (and will in most cases happen just once), a container can call the delegate multiple times
        /// and make parallel calls to the delegate. You must make sure that the code can be called multiple 
        /// times and is thread-safe.
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
        /// The example above registers a delegate that is fired every time an unregistered type is requested
        /// from the container. The delegate checks whether the requested type is a closed generic
        /// implementation of the <b>IValidator&lt;T&gt;</b> interface (such as 
        /// <b>IValidator&lt;Order&gt;</b> or <b>IValidator&lt;Customer&gt;</b>). In that case it
        /// will request the container for a concrete <b>EmptyValidator&lt;T&gt;</b> implementation that
        /// implements the given 
        /// <see cref="UnregisteredTypeEventArgs.UnregisteredServiceType">UnregisteredServiceType</see>, and
        /// registers a delegate that will return this created instance. The <b>e.Register</b> call
        /// registers the method in the container, preventing the <see cref="ResolveUnregisteredType"/> from
        /// being called again for the exact same service type, preventing any performance penalties.
        /// </example>
        public event EventHandler<UnregisteredTypeEventArgs> ResolveUnregisteredType
        {
            add
            {
                this.ThrowWhenContainerIsLocked();

                this.resolveUnregisteredType += value;
            }

            remove
            {
                this.ThrowWhenContainerIsLocked();

                this.resolveUnregisteredType -= value;
            }
        }

        /// <summary>
        /// Occurs after the creation of the <see cref="System.Linq.Expressions.Expression">Expression</see> 
        /// of a registered type, allowing the created 
        /// <see cref="System.Linq.Expressions.Expression">Expression</see>  to be replaced. Multiple delegates 
        /// may handle the same service type.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The <see cref="ExpressionBuilt"/> event is called by the container every time an registered type is 
        /// getting compiled, allowing a developer to change the way the type is created. The delegate that
        /// hooks to the <see cref="ExpressionBuilt"/> event, can change the 
        /// <see cref="ExpressionBuiltEventArgs.Expression" /> property on the 
        /// <see cref="ExpressionBuiltEventArgs"/>, which allows changing the way the type is constructed.
        /// </para>
        /// <para>
        /// This event is called after unregistered types are resolved.
        /// </para>
        /// <para>
        /// <b>Thread-safety:</b> Please note that the container will not ensure that the hooked delegates
        /// are executed only once per service type. While the calls to <see cref="ExpressionBuilt" /> for a given 
        /// type are finite (and will in most cases happen just once), a container can call the delegate 
        /// multiple times and make parallel calls to the delegate. You must make sure that the code can be 
        /// called multiple times and is thread-safe.
        /// </para>
        /// </remarks>
        /// <example>
        /// The following example shows the usage of the <see cref="ExpressionBuilt" /> event:
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
        ///     container.RegisterSingle<ILogger, ConsoleLogger>();
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
        /// The example above registers a delegate that is fired every time the container compiles the
        /// expression for an registered type. The delegate checks whether the requested type is a closed generic
        /// implementation of the <b>IValidator&lt;T&gt;</b> interface (such as 
        /// <b>IValidator&lt;Order&gt;</b> or <b>IValidator&lt;Customer&gt;</b>). In that case it
        /// will changes the current <see cref="ExpressionBuiltEventArgs.Expression"/> with a new one that creates
        /// a new <b>MonitoringValidator&lt;T&gt;</b> that takes the current validator (and an <b>ILogger</b>)
        /// as an dependency.
        /// </example>
        public event EventHandler<ExpressionBuiltEventArgs> ExpressionBuilt
        {
            add
            {
                this.ThrowWhenContainerIsLocked();

                this.expressionBuilt += value;
            }

            remove
            {
                this.ThrowWhenContainerIsLocked();

                this.expressionBuilt -= value;
            }
        }

        public event EventHandler<ExpressionBuildingEventArgs> ExpressionBuilding
        {
            add
            {
                this.ThrowWhenContainerIsLocked();

                this.expressionBuilding += value;
            }

            remove
            {
                this.ThrowWhenContainerIsLocked();

                this.expressionBuilding -= value;
            }
        }

        /// <summary>
        /// Registers that a new instance of <typeparamref name="TConcrete"/> will be returned every time it 
        /// is requested (transient). Note that calling this method is redundant in most scenarios, because
        /// the container will return a new instance for unregistered concrete types. Registration is needed
        /// when the security restrictions of the application's sandbox don't allow the container to create
        /// such type.
        /// </summary>
        /// <typeparam name="TConcrete">The concrete type that will be registered.</typeparam>
        /// <exception cref="InvalidOperationException">
        /// Thrown when this container instance is locked and can not be altered, or when an 
        /// the <typeparamref name="TConcrete"/> has already been registered.
        /// </exception>
        /// <exception cref="ArgumentException">Thrown when the <typeparamref name="TConcrete"/> is a type
        /// that can not be created by the container.</exception>
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter",
            Justification = "A design without a generic T would be unpractical, because the other " +
            "overloads also take a generic T.")]
        public void Register<TConcrete>() where TConcrete : class
        {
            Requires.IsNotAnAmbiguousType(typeof(TConcrete), "TConcrete");
            this.ThrowArgumentExceptionWhenTypeIsNotConstructable(typeof(TConcrete), "TConcrete");

            this.Register<TConcrete, TConcrete>(Lifestyle.Transient);
        }

        /// <summary>
        /// Registers that a new instance of <typeparamref name="TImplementation"/> will be returned every time a
        /// <typeparamref name="TService"/> is requested.
        /// </summary>
        /// <typeparam name="TService">The interface or base type that can be used to retrieve the instances.</typeparam>
        /// <typeparam name="TImplementation">The concrete type that will be registered.</typeparam>
        /// <exception cref="InvalidOperationException">
        /// Thrown when this container instance is locked and can not be altered, or when an 
        /// the <typeparamref name="TService"/> has already been registered.</exception>
        /// <exception cref="ArgumentException">Thrown when the given <typeparamref name="TImplementation"/> 
        /// type is not a type that can be created by the container.
        /// </exception>
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter",
            Justification = "Any other design would be inappropriate.")]
        public void Register<TService, TImplementation>()
            where TImplementation : class, TService
            where TService : class
        {
            Requires.IsNotAnAmbiguousType(typeof(TService), "TService");
            this.ThrowArgumentExceptionWhenTypeIsNotConstructable(typeof(TService), typeof(TImplementation),
                "TImplementation");

            this.Register<TService, TImplementation>(Lifestyle.Transient);
        }

        /// <summary>
        /// Registers the specified delegate that allows returning instances of <typeparamref name="TService"/>.
        /// </summary>
        /// <typeparam name="TService">The interface or base type that can be used to retrieve instances.</typeparam>
        /// <param name="instanceCreator">The delegate that allows building or creating new instances.</param>
        /// <exception cref="InvalidOperationException">
        /// Thrown when this container instance is locked and can not be altered, or when the 
        /// <typeparamref name="TService"/> has already been registered.</exception>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="instanceCreator"/> is a null reference.</exception>
        public void Register<TService>(Func<TService> instanceCreator) where TService : class
        {
            Requires.IsNotNull(instanceCreator, "instanceCreator");
            Requires.IsNotAnAmbiguousType(typeof(TService), "TService");

            this.Register<TService>(instanceCreator, Lifestyle.Transient);
        }

        /// <summary>
        /// Registers a single concrete instance that will be constructed using constructor injection. 
        /// This <typeparamref name="TConcrete"/> must be thread-safe.
        /// </summary>
        /// <typeparam name="TConcrete">The concrete type that will be registered.</typeparam>
        /// <exception cref="InvalidOperationException">
        /// Thrown when this container instance is locked and can not be altered, or when 
        /// <typeparamref name="TConcrete"/> has already been registered.
        /// </exception>
        /// <exception cref="ArgumentException">Thrown when the <typeparamref name="TConcrete"/> is a type
        /// that can not be created by the container.</exception>
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter",
            Justification = "A design without a generic T would be unpractical, because the other " +
            "overloads also take a generic T.")]
        public void RegisterSingle<TConcrete>() where TConcrete : class
        {
            Requires.IsNotAnAmbiguousType(typeof(TConcrete), "TConcrete");
            this.ThrowArgumentExceptionWhenTypeIsNotConstructable(typeof(TConcrete), "TConcrete");

            this.Register<TConcrete, TConcrete>(Lifestyle.Singleton);
        }

        /// <summary>
        /// Registers that the same instance of <typeparamref name="TImplementation"/> will be returned every 
        /// time a <typeparamref name="TService"/> is requested.
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
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter",
            Justification = "Any other design would be inappropriate.")]
        public void RegisterSingle<TService, TImplementation>()
            where TImplementation : class, TService
            where TService : class
        {
            Requires.IsNotAnAmbiguousType(typeof(TService), "TService");
            this.ThrowArgumentExceptionWhenTypeIsNotConstructable(typeof(TService), typeof(TImplementation),
                "TImplementation");

            this.Register<TService, TImplementation>(Lifestyle.Singleton);
        }

        /// <summary>Registers a single instance. This <paramref name="instance"/> must be thread-safe.</summary>
        /// <typeparam name="TService">The interface or base type that can be used to retrieve the instance.</typeparam>
        /// <param name="instance">The instance to register.</param>
        /// <exception cref="InvalidOperationException">
        /// Thrown when this container instance is locked and can not be altered, or when the 
        /// <typeparamref name="TService"/> has already been registered.</exception>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="instance"/> is a null reference.
        /// </exception>
        public void RegisterSingle<TService>(TService instance) where TService : class
        {
            Requires.IsNotNull(instance, "instance");
            Requires.IsNotAnAmbiguousType(typeof(TService), "TService");

            var registration = SingletonLifestyle.CreateRegistrationForSingleInstance(typeof(TService), instance, this);

            this.AddRegistration(typeof(TService), registration);
        }

        /// <summary>
        /// Registers the specified delegate that allows constructing a single instance of 
        /// <typeparamref name="TService"/>. This delegate will be called at most once during the lifetime of 
        /// the application.
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
        public void RegisterSingle<TService>(Func<TService> instanceCreator) where TService : class
        {
            Requires.IsNotNull(instanceCreator, "instanceCreator");
            Requires.IsNotAnAmbiguousType(typeof(TService), "TService");

            this.Register<TService>(instanceCreator, Lifestyle.Singleton);
        }

        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter",
            Justification = "Any other design would be inappropriate.")]
        public void Register<TService, TImplementation>(Lifestyle lifestyle)
            where TImplementation : class, TService
            where TService : class
        {
            Requires.IsNotNull(lifestyle, "lifestyle");

            var registration = lifestyle.CreateRegistration<TService, TImplementation>(this);

            this.AddRegistration(typeof(TService), registration);
        }

        public void Register<TService>(Func<TService> instanceCreator, Lifestyle lifestyle)
            where TService : class
        {
            Requires.IsNotNull(instanceCreator, "instanceCreator");
            Requires.IsNotNull(lifestyle, "lifestyle");

            var registration = lifestyle.CreateRegistration<TService>(instanceCreator, this);

            this.AddRegistration(typeof(TService), registration);
        }

        public void Register(Type serviceType, Type implementationType, Lifestyle lifestyle)
        {
            Requires.IsNotNull(serviceType, "serviceType");
            Requires.IsNotNull(implementationType, "implementationType");
            Requires.IsNotNull(lifestyle, "lifestyle");

            Requires.IsReferenceType(serviceType, "serviceType");
            Requires.IsReferenceType(implementationType, "implementationType");
            Requires.IsNotOpenGenericType(serviceType, "serviceType");
            Requires.IsNotOpenGenericType(implementationType, "implementationType");
            Requires.ServiceIsAssignableFromImplementation(serviceType, implementationType, 
                "implementationType");

            Requires.IsNotAnAmbiguousType(serviceType, "serviceType");

            var registration = lifestyle.CreateRegistration(serviceType, implementationType, this);

            this.AddRegistration(serviceType, registration);
        }

        public void Register(Type serviceType, Func<object> instanceCreator, Lifestyle lifestyle)
        {
            Requires.IsNotNull(serviceType, "serviceType");
            Requires.IsNotNull(instanceCreator, "instanceCreator");
            Requires.IsNotNull(lifestyle, "lifestyle");

            Requires.IsReferenceType(serviceType, "serviceType");
            Requires.IsNotOpenGenericType(serviceType, "serviceType");

            Requires.IsNotAnAmbiguousType(serviceType, "serviceType");

            var registration = lifestyle.CreateRegistration(serviceType, instanceCreator, this);

            this.AddRegistration(serviceType, registration);
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
        /// <see cref="Register{TService}(Func{TService})"/> and 
        /// <see cref="RegisterSingle{TService}(Func{TService})"/> methods) or registered as single instance
        /// (using <see cref="RegisterSingle{TService}(TService)"/>) will not trigger initialization.
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
            Requires.IsNotNull(instanceInitializer, "instanceInitializer");

            this.ThrowWhenContainerIsLocked();

            this.instanceInitializers.Add(new InstanceInitializer
            {
                ServiceType = typeof(TService),
                Action = instanceInitializer,
            });
        }

        /// <summary>
        /// Registers a dynamic collection of elements of type <typeparamref name="TService"/>. A call to
        /// <see cref="GetAllInstances{T}"/> will return the <paramref name="collection"/> itself, and updates 
        /// to the collection will be reflected in the result. If updates are allowed, make sure the 
        /// collection can be iterated safely if you're running a multi-threaded application.
        /// </summary>
        /// <typeparam name="TService">The interface or base type that can be used to retrieve instances.</typeparam>
        /// <param name="collection">The collection to register.</param>
        /// <exception cref="InvalidOperationException">
        /// Thrown when this container instance is locked and can not be altered, or when a <paramref name="collection"/>
        /// for <typeparamref name="TService"/> has already been registered.
        /// </exception>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="collection"/> is a null
        /// reference.</exception>
        public void RegisterAll<TService>(IEnumerable<TService> collection) where TService : class
        {
            Requires.IsNotNull(collection, "collection");
            Requires.IsNotAnAmbiguousType(typeof(TService), "TService");

            this.ThrowWhenCollectionTypeAlreadyRegistered(typeof(TService));

            var readOnlyCollection = collection.MakeReadOnly();

            var registration = SingletonLifestyle.CreateRegistrationForSingleInstance(
                typeof(IEnumerable<TService>), readOnlyCollection, this);

            this.AddRegistration(typeof(IEnumerable<TService>), registration);

            this.collectionsToValidate[typeof(TService)] = readOnlyCollection;
        }

        /// <summary>
        /// Registers a collection of singleton elements of type <typeparamref name="TService"/>.
        /// </summary>
        /// <typeparam name="TService">The interface or base type that can be used to retrieve instances.</typeparam>
        /// <param name="singletons">The collection to register.</param>
        /// <exception cref="InvalidOperationException">
        /// Thrown when this container instance is locked and can not be altered, or when a <paramref name="singletons"/>
        /// for <typeparamref name="TService"/> has already been registered.
        /// </exception>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="singletons"/> is a null
        /// reference.</exception>
        /// <exception cref="ArgumentException">Thrown when one of the elements of <paramref name="singletons"/>
        /// is a null reference.</exception>
        public void RegisterAll<TService>(params TService[] singletons) where TService : class
        {
            Requires.IsNotNull(singletons, "singletons");

            Requires.DoesNotContainNullValues(singletons, "singletons");

            this.RegisterAll<TService>(new DecoratableSingletonCollection<TService>(this, singletons));
        }

        /// <summary>
        /// Registers an collection of <paramref name="serviceTypes"/>, which instances will be resolved when
        /// enumerating the set returned when a collection of <typeparamref name="TService"/> objects is 
        /// requested. On enumeration the container is called for each type in the list.
        /// </summary>
        /// <typeparam name="TService">The base type or interface for elements in the collection.</typeparam>
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
        public void RegisterAll<TService>(params Type[] serviceTypes)
        {
            this.RegisterAll(typeof(TService), serviceTypes);
        }

        /// <summary>
        /// Registers a collection of instances of <paramref name="serviceTypes"/> to be returned when
        /// a collection of <typeparamref name="TService"/> objects is requested.
        /// </summary>
        /// <typeparam name="TService">The base type or interface for elements in the collection.</typeparam>
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
        public void RegisterAll<TService>(IEnumerable<Type> serviceTypes)
        {
            this.RegisterAll(typeof(TService), serviceTypes);
        }

        /// <summary>
        /// Registers an collection of <paramref name="serviceTypes"/>, which instances will be resolved when
        /// enumerating the set returned when a collection of <paramref name="serviceType"/> objects is 
        /// requested. On enumeration the container is called for each type in the list.
        /// </summary>
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
        public void RegisterAll(Type serviceType, IEnumerable<Type> serviceTypes)
        {
            Requires.IsNotNull(serviceType, "serviceType");
            Requires.IsNotNull(serviceTypes, "serviceTypes");
            Requires.TypeIsNotOpenGeneric(serviceType, "serviceType");

            // Make a copy for correctness and performance.
            Type[] types = serviceTypes.ToArray();

            Requires.DoesNotContainNullValues(types, "serviceTypes");
            Requires.DoesNotContainOpenGenericTypes(types, "serviceTypes");
            Requires.ServiceIsAssignableFromImplementations(serviceType, types, "serviceTypes",
                typeCanBeServiceType: true);

            IDecoratableEnumerable enumerable =
                DecoratorHelpers.CreateDecoratableEnumerable(serviceType, this, types);

            this.RegisterAllInternal(serviceType, enumerable);
        }

        /// <summary>
        /// Registers a <paramref name="collection"/> of elements of type <paramref name="serviceType"/>.
        /// </summary>
        /// <param name="serviceType">The base type or interface for elements in the collection.</param>
        /// <param name="collection">The collection of items to register.</param>
        /// <exception cref="ArgumentNullException">Thrown when either <paramref name="container"/>,
        /// <paramref name="serviceType"/> or <paramref name="collection"/> are null references (Nothing in
        /// VB).</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="serviceType"/> represents an
        /// open generic type.</exception>
        public void RegisterAll(Type serviceType, IEnumerable collection)
        {
            Requires.IsNotNull(serviceType, "serviceType");
            Requires.IsNotNull(collection, "collection");
            Requires.TypeIsNotOpenGeneric(serviceType, "serviceType");

            try
            {
                this.RegisterAllInternal(serviceType, collection.Cast<object>().MakeReadOnly());
            }
            catch (MemberAccessException ex)
            {
                // This happens when the user tries to resolve an internal type inside a (Silverlight) sandbox.
                throw new ArgumentException(
                    StringResources.UnableToResolveTypeDueToSecurityConfiguration(serviceType, ex),
#if !SILVERLIGHT
                    "serviceType",
#endif
                    ex);
            }
        }

        /// <summary>
        /// Verifies the <b>Container</b>. This method will call all registered delegates, 
        /// iterate registered collections and throws an exception if there was an error.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when the registration of instances was
        /// invalid.</exception>
        public void Verify()
        {
            this.IsVerifying = true;

            try
            {
                this.ValidateRegistrations();
                this.ValidateRegisteredCollections();
                this.succesfullyVerified = true;
            }
            finally
            {
                this.IsVerifying = false;
            }
        }

        internal void ThrowWhenContainerIsLocked()
        {
            // By using a lock, we have the certainty that all threads will see the new value for 'locked'
            // immediately.
            lock (this.locker)
            {
                if (this.locked)
                {
                    throw new InvalidOperationException(StringResources.ContainerCanNotBeChangedAfterUse());
                }
            }
        }

        internal bool IsConstructableType(Type serviceType, Type implementationType, out string errorMessage)
        {
            errorMessage = null;

            try
            {
                var constructor = this.Options.ConstructorResolutionBehavior
                    .GetConstructor(serviceType, implementationType);

                this.Options.ConstructorVerificationBehavior.Verify(constructor);
            }
            catch (ActivationException ex)
            {
                errorMessage = ex.Message;
            }

            return errorMessage == null;
        }

        private void RegisterAllInternal(Type serviceType, IEnumerable readOnlyCollection)
        {
            IEnumerable castedCollection = Helpers.CastCollection(readOnlyCollection, serviceType);

            this.ThrowWhenCollectionTypeAlreadyRegistered(serviceType);

            Type enumerableServiceType = typeof(IEnumerable<>).MakeGenericType(serviceType);

            var registration = SingletonLifestyle.CreateRegistrationForSingleInstance(enumerableServiceType,
                castedCollection, this);

            this.AddRegistration(enumerableServiceType, registration);

            this.collectionsToValidate[serviceType] = readOnlyCollection;
        }

        private void AddRegistration(Type key, Registration registration)
        {
            this.ThrowWhenContainerIsLocked();
            this.ThrowWhenTypeAlreadyRegistered(key);

            this.registrations[key] = new InstanceProducer(key, registration);
        }

        private void ThrowWhenTypeAlreadyRegistered(Type type)
        {
            if (this.registrations.ContainsKey(type))
            {
                if (!this.Options.AllowOverridingRegistrations)
                {
                    throw new InvalidOperationException(StringResources.TypeAlreadyRegistered(type));
                }
            }
        }

        private void ThrowWhenCollectionTypeAlreadyRegistered(Type itemType)
        {
            if (!this.Options.AllowOverridingRegistrations &&
                this.registrations.ContainsKey(typeof(IEnumerable<>).MakeGenericType(itemType)))
            {
                throw new InvalidOperationException(
                    StringResources.CollectionTypeAlreadyRegistered(itemType));
            }
        }

        private void ValidateRegistrations()
        {
            foreach (var pair in this.registrations)
            {
                InstanceProducer producer = pair.Value;

                // The producer can be null.
                if (producer != null)
                {
                    producer.Verify();
                }
            }
        }

        private void ValidateRegisteredCollections()
        {
            foreach (var pair in this.collectionsToValidate)
            {
                Type serviceType = pair.Key;
                IEnumerable collection = pair.Value;

                Helpers.ThrowWhenCollectionCanNotBeIterated(collection, serviceType);
                Helpers.ThrowWhenCollectionContainsNullArguments(collection, serviceType);
            }
        }

        private Action<T>[] GetInstanceInitializersFor<T>(Type type)
        {
            var typeHierarchy = Helpers.GetTypeHierarchyFor(type);

            return (
                from instanceInitializer in this.instanceInitializers
                where typeHierarchy.Contains(instanceInitializer.ServiceType)
                select Helpers.CreateAction<T>(instanceInitializer.Action))
                .ToArray();
        }

        private void ThrowArgumentExceptionWhenTypeIsNotConstructable(Type concreteType, string parameterName)
        {
            this.ThrowArgumentExceptionWhenTypeIsNotConstructable(concreteType, concreteType, parameterName);
        }

        private void ThrowArgumentExceptionWhenTypeIsNotConstructable(Type serviceType,
            Type implementationType, string parameterName)
        {
            string message;

            bool constructable = this.IsConstructableType(serviceType, implementationType, out message);

            if (!constructable)
            {
                // After some doubt (and even after reading http://bit.ly/1CPDv9) I decided to throw an
                // ArgumentException when the given generic type argument was invalid. Mainly because a
                // generic type argument is just an argument, and ArgumentException even allows us to supply 
                // the name of the argument. No developer will be surprise to see an ArgEx in this case.
                throw new ArgumentException(message, parameterName);
            }
        }

        // This class is a trick to allow the SimpleInjector.Extensions library to correctly wrap these
        // instances with decorators (it uses the IEnumerable<Expression>).
        private sealed class DecoratableSingletonCollection<TService> 
            : DecoratableSingletonCollectionBase<TService>, IEnumerable<Expression>
        {
            internal DecoratableSingletonCollection(Container container, TService[] services)
                : base(container, services)
            {
            }

            IEnumerator<Expression> IEnumerable<Expression>.GetEnumerator()
            {
                foreach (var item in this.Items)
                {
                    yield return Expression.Constant(item.Instance);
                }
            }
        }

        private abstract class DecoratableSingletonCollectionBase<TService> : IEnumerable<TService>
        {
            protected readonly DecoratableSingleton[] Items;

            protected DecoratableSingletonCollectionBase(Container container, TService[] instances)
            {
                this.Items = (
                    from instance in instances
                    select new DecoratableSingleton(instance, container))
                    .ToArray();
            }

            public IEnumerator<TService> GetEnumerator()
            {
                for (int i = 0; i < this.Items.Length; i++)
                {
                    yield return this.Items[i].Instance;
                }
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return this.GetEnumerator();
            }

            protected sealed class DecoratableSingleton
            {
                private readonly TService instance;
                private readonly Container container;
                private bool initialized;

                public DecoratableSingleton(TService instance, Container container)
                {
                    this.instance = instance;
                    this.container = container;
                    this.initialized = false;
                }

                public TService Instance
                {
                    get
                    {
                        if (!this.initialized)
                        {
                            lock (this)
                            {
                                if (!this.initialized)
                                {
                                    this.Initialize(this.instance);

                                    this.initialized = true;
                                }
                            }
                        }

                        return this.instance;
                    }
                }

                private void Initialize(TService instance)
                {
                    var initializer = this.container.GetInitializer<TService>();

                    if (initializer != null)
                    {
                        initializer(instance);
                    }
                }
            }
        }
    }
}