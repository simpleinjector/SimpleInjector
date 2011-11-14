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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

using SimpleInjector.InstanceProducers;

namespace SimpleInjector
{
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
        /// are finite (and will mostly happen just once), a container can call the delegate multiple times
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
        /// public static void TestResolveUnregisteredType()
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
        /// Registers that a new instance of <typeparamref name="TConcrete"/> will be returned every time it 
        /// is requested (transient). Note that calling this method is redundant in most scenario's, because
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
            Helpers.ThrowArgumentExceptionWhenTypeIsNotConstructable(typeof(TConcrete), "TConcrete");

            this.ThrowWhenContainerIsLocked();
            this.ThrowWhenTypeAlreadyRegistered(typeof(TConcrete));

            this.AddRegistration(new ConcreteTransientInstanceProducer<TConcrete>());
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
            Helpers.ThrowArgumentExceptionWhenTypeIsNotConstructable(typeof(TImplementation), "TImplementation");

            this.ThrowWhenContainerIsLocked();
            this.ThrowWhenTypeAlreadyRegistered(typeof(TService));

            this.AddRegistration(new TransientInstanceProducer<TService, TImplementation>());
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
            if (instanceCreator == null)
            {
                throw new ArgumentNullException("instanceCreator");
            }

            this.ThrowWhenContainerIsLocked();
            this.ThrowWhenTypeAlreadyRegistered(typeof(TService));

            this.AddRegistration(new FuncInstanceProducer<TService>(instanceCreator));
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
            Helpers.ThrowArgumentExceptionWhenTypeIsNotConstructable(typeof(TConcrete), "TConcrete");

            this.ThrowWhenContainerIsLocked();
            this.ThrowWhenTypeAlreadyRegistered(typeof(TConcrete));

            var instanceProducer = new ConcreteTransientInstanceProducer<TConcrete> { Container = this };

            Func<TConcrete> instanceCreator = () => (TConcrete)instanceProducer.GetInstance();

            this.AddRegistration(new FuncSingletonInstanceProducer<TConcrete>(instanceCreator));
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
            Helpers.ThrowArgumentExceptionWhenTypeIsNotConstructable(typeof(TImplementation), "TImplementation");

            this.ThrowWhenContainerIsLocked();
            this.ThrowWhenTypeAlreadyRegistered(typeof(TService));

            var producer = new TransientInstanceProducer<TService, TImplementation> { Container = this };

            Func<TService> instanceCreator = () => (TService)producer.GetInstance();

            this.AddRegistration(new FuncSingletonInstanceProducer<TService>(instanceCreator));
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
            if (instance == null)
            {
                throw new ArgumentNullException("instance");
            }

            this.ThrowWhenContainerIsLocked();
            this.ThrowWhenTypeAlreadyRegistered(typeof(TService));

            this.AddRegistration(new SingletonInstanceProducer<TService>(instance));
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
            if (instanceCreator == null)
            {
                throw new ArgumentNullException("instanceCreator");
            }

            this.ThrowWhenContainerIsLocked();
            this.ThrowWhenTypeAlreadyRegistered(typeof(TService));

            this.AddRegistration(new FuncSingletonInstanceProducer<TService>(instanceCreator));
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
            if (instanceInitializer == null)
            {
                throw new ArgumentNullException("instanceInitializer");
            }

            this.ThrowWhenContainerIsLocked();

            this.instanceInitializers.Add(new InstanceInitializer
            {
                ServiceType = typeof(TService),
                Action = instanceInitializer,
            });
        }

        /// <summary>
        /// Registers a collection of elements of <typeparamref name="TService"/>.
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
            if (collection == null)
            {
                throw new ArgumentNullException("collection");
            }

            this.ThrowWhenContainerIsLocked();
            this.ThrowWhenRegisteredCollectionsAlreadyContainsKeyFor<TService>();

            var immutableCollection = collection.MakeImmutable();

            this.collectionsToValidate[typeof(TService)] = immutableCollection;

            this.AddRegistration(new SingletonInstanceProducer<IEnumerable<TService>>(immutableCollection));
        }

        /// <summary>
        /// Registers a collection of elements of <typeparamref name="TService"/>.
        /// </summary>
        /// <typeparam name="TService">The interface or base type that can be used to retrieve instances.</typeparam>
        /// <param name="collection">The collection to register.</param>
        /// <exception cref="InvalidOperationException">
        /// Thrown when this container instance is locked and can not be altered, or when a <paramref name="collection"/>
        /// for <typeparamref name="TService"/> has already been registered.
        /// </exception>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="collection"/> is a null
        /// reference.</exception>
        public void RegisterAll<TService>(params TService[] collection) where TService : class
        {
            this.RegisterAll<TService>((IEnumerable<TService>)collection);
        }

        /// <summary>
        /// Verifies the <b>Container</b>. This method will call all registered delegates, 
        /// iterate registered collections and throws an exception if there was an error.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when the registration of instances was
        /// invalid.</exception>
        public void Verify()
        {
            this.ValidateRegistrations();
            this.ValidateRegisteredCollections();
        }

        internal void AddRegistration(InstanceProducer registration)
        {
            registration.Container = this;

            this.registrations[registration.ServiceType] = registration;
        }

        internal Action<T> GetInitializerFor<T>()
        {
            var initializersForType = this.GetInstanceInitializersFor<T>();

            if (initializersForType.Length <= 1)
            {
                return initializersForType.FirstOrDefault();
            }
            else
            {
                return obj =>
                {
                    for (int index = 0; index < initializersForType.Length; index++)
                    {
                        initializersForType[index](obj);
                    }
                };
            }
        }

        internal void ThrowWhenTypeAlreadyRegistered(Type type)
        {
            if (this.registrations.ContainsKey(type))
            {
                throw new InvalidOperationException(
                    StringResources.TypeAlreadyRegistered(type));
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
                    throw new InvalidOperationException(
                        StringResources.ContainerCanNotBeChangedAfterUse(this.GetType()));
                }
            }
        }

        private void ValidateRegistrations()
        {
            foreach (var pair in this.registrations)
            {
                Type serviceType = pair.Key;
                InstanceProducer instanceProducer = pair.Value;

                instanceProducer.Verify(serviceType);
            }
        }

        private void ValidateRegisteredCollections()
        {
            foreach (var pair in this.collectionsToValidate)
            {
                Type serviceType = pair.Key;
                IEnumerable collection = pair.Value;

                Helpers.ValidateIfCollectionCanBeIterated(collection, serviceType);
                Helpers.ValidateIfCollectionForNullElements(collection, serviceType);
            }
        }

        private void ThrowWhenRegisteredCollectionsAlreadyContainsKeyFor<T>()
        {
            if (this.registrations.ContainsKey(typeof(IEnumerable<T>)))
            {
                throw new InvalidOperationException(
                    StringResources.CollectionTypeAlreadyRegistered(typeof(T)));
            }
        }

        private Action<T>[] GetInstanceInitializersFor<T>()
        {
            var typeHierarchy = Helpers.GetTypeHierarchyFor<T>();

            return (
                from instanceInitializer in this.instanceInitializers
                where typeHierarchy.Contains(instanceInitializer.ServiceType)
                select Helpers.CreateAction<T>(instanceInitializer.Action))
                .ToArray();
        }
    }
}