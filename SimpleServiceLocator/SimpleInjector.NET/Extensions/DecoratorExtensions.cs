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

namespace SimpleInjector.Extensions
{
    using System;

    using SimpleInjector.Extensions.Decorators;

    /// <summary>
    /// Extension methods for applying decorators.
    /// </summary>
    public static class DecoratorExtensions
    {
        /// <summary>
        /// Ensures that the supplied <paramref name="decoratorType"/> decorator is returned and cached with
        /// the given <paramref name="lifestyle"/>, wrapping the original registered 
        /// <paramref name="serviceType"/>, by injecting that service type into the constructor of the 
        /// supplied <paramref name="decoratorType"/>. Multiple decorators may be applied to the same 
        /// <paramref name="serviceType"/>. Decorators can be applied to both open, closed, and non-generic 
        /// service types.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The <b>RegisterOpenGenericDecorator</b> method works by hooking onto the container's
        /// <see cref="Container.ExpressionBuilt">ExpressionBuilt</see> event. This event fires after the
        /// <see cref="Container.ResolveUnregisteredType">ResolveUnregisteredType</see> event, which allows
        /// decoration of types that are resolved using unregistered type resolution. The
        /// <see cref="OpenGenericRegistrationExtensions.RegisterOpenGeneric(Container,Type,Type,Lifestyle)">RegisterOpenGeneric</see>
        /// extension method, for instance, hooks onto the <b>ResolveUnregisteredType</b>. This allows you to
        /// use <b>RegisterOpenGenericDecorator</b> on the same service type as <b>RegisterOpenGeneric</b>.
        /// </para>
        /// <para>
        /// Multiple decorators can be applied to the same service type. The order in which they are registered
        /// is the order they get applied in. This means that the decorator that gets registered first, gets
        /// applied first, which means that the next registered decorator, will wrap the first decorator, which
        /// wraps the original service type.
        /// </para>
        /// <para>
        /// Constructor injection will be used on that type, and although it may have many constructor 
        /// arguments, it must have exactly one argument of the type of <paramref name="serviceType"/>, or an 
        /// argument of type <see cref="Func{TResult}"/> where <b>TResult</b> is <paramref name="serviceType"/>.
        /// An exception will be thrown when this is not the case.
        /// </para>
        /// <para>
        /// The registered <paramref name="decoratorType"/> may have a constructor with an argument of type
        /// <see cref="Func{T}"/> where <b>T</b> is <paramref name="serviceType"/>. In this case, the
        /// will not inject the decorated <paramref name="serviceType"/> itself into the 
        /// <paramref name="decoratorType"/> instance, but it will inject a <see cref="Func{T}"/> that allows
        /// creating instances of the decorated type, according to the lifestyle of that type. This enables
        /// more advanced scenarios, such as executing the decorated types on a different thread, or executing
        /// decorated instance within a certain scope (such as a lifetime scope).
        /// </para>
        /// </remarks>
        /// <example>
        /// Please see the <see cref="RegisterDecorator(Container, Type, Type)">RegisterDecorator</see> method
        /// for more information.
        /// </example>
        /// <param name="container">The container to make the registrations in.</param>
        /// <param name="serviceType">The definition of the open generic service type that will
        /// be wrapped by the given <paramref name="decoratorType"/>.</param>
        /// <param name="decoratorType">The definition of the open generic decorator type that will
        /// be used to wrap the original service type.</param>
        /// <param name="lifestyle">The lifestyle that specifies how the returned decorator will be cached.</param>
        /// <exception cref="ArgumentNullException">Thrown when one of the arguments is a null reference.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="serviceType"/> is not
        /// an open generic type, when <paramref name="decoratorType"/> does not inherit from or 
        /// implement <paramref name="serviceType"/>, when <paramref name="decoratorType"/>
        /// does not have a single public constructor, or when <paramref name="decoratorType"/> does 
        /// not contain a constructor that has exactly one argument of type 
        /// <paramref name="serviceType"/> or <see cref="Func{T}"/> where <b>T</b> is
        /// <paramref name="serviceType"/>.</exception>
        public static void RegisterDecorator(this Container container, Type serviceType, Type decoratorType,
            Lifestyle lifestyle)
        {
            container.RegisterDecoratorCore(serviceType, decoratorType, null, lifestyle);
        }

        /// <summary>
        /// Ensures that the supplied <paramref name="decoratorType"/> decorator is returned when the supplied
        /// <paramref name="predicate"/> returns <b>true</b> and cached with the given 
        /// <paramref name="lifestyle"/>, wrapping the original registered <paramref name="serviceType"/>, by 
        /// injecting that service type into the constructor of the supplied <paramref name="decoratorType"/>. 
        /// Multiple decorators may be applied to the same <paramref name="serviceType"/>. Decorators can be 
        /// applied to both open, closed, and non-generic service types.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The <b>RegisterOpenGenericDecorator</b> method works by hooking onto the container's
        /// <see cref="Container.ExpressionBuilt">ExpressionBuilt</see> event. This event fires after the
        /// <see cref="Container.ResolveUnregisteredType">ResolveUnregisteredType</see> event, which allows
        /// decoration of types that are resolved using unregistered type resolution. The
        /// <see cref="OpenGenericRegistrationExtensions.RegisterOpenGeneric(Container,Type,Type,Lifestyle)">RegisterOpenGeneric</see>
        /// extension method, for instance, hooks onto the <b>ResolveUnregisteredType</b>. This allows you to
        /// use <b>RegisterOpenGenericDecorator</b> on the same service type as <b>RegisterOpenGeneric</b>.
        /// </para>
        /// <para>
        /// Multiple decorators can be applied to the same service type. The order in which they are registered
        /// is the order they get applied in. This means that the decorator that gets registered first, gets
        /// applied first, which means that the next registered decorator, will wrap the first decorator, which
        /// wraps the original service type.
        /// </para>
        /// <para>
        /// Constructor injection will be used on that type, and although it may have many constructor 
        /// arguments, it must have exactly one argument of the type of <paramref name="serviceType"/>, or an 
        /// argument of type <see cref="Func{TResult}"/> where <b>TResult</b> is <paramref name="serviceType"/>.
        /// An exception will be thrown when this is not the case.
        /// </para>
        /// <para>
        /// The registered <paramref name="decoratorType"/> may have a constructor with an argument of type
        /// <see cref="Func{T}"/> where <b>T</b> is <paramref name="serviceType"/>. In this case, the
        /// will not inject the decorated <paramref name="serviceType"/> itself into the 
        /// <paramref name="decoratorType"/> instance, but it will inject a <see cref="Func{T}"/> that allows
        /// creating instances of the decorated type, according to the lifestyle of that type. This enables
        /// more advanced scenarios, such as executing the decorated types on a different thread, or executing
        /// decorated instance within a certain scope (such as a lifetime scope).
        /// </para>
        /// </remarks>
        /// <example>
        /// Please see the <see cref="RegisterDecorator(Container, Type, Type)">RegisterDecorator</see> method
        /// for more information.
        /// </example>
        /// <param name="container">The container to make the registrations in.</param>
        /// <param name="serviceType">The definition of the open generic service type that will
        /// be wrapped by the given <paramref name="decoratorType"/>.</param>
        /// <param name="decoratorType">The definition of the open generic decorator type that will
        /// be used to wrap the original service type.</param>
        /// <param name="lifestyle">The lifestyle that specifies how the returned decorator will be cached.</param>
        /// <param name="predicate">The predicate that determines whether the 
        /// <paramref name="decoratorType"/> must be applied to a service type.</param>
        /// <exception cref="ArgumentNullException">Thrown when one of the arguments is a null reference.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="serviceType"/> is not
        /// an open generic type, when <paramref name="decoratorType"/> does not inherit from or 
        /// implement <paramref name="serviceType"/>, when <paramref name="decoratorType"/>
        /// does not have a single public constructor, or when <paramref name="decoratorType"/> does 
        /// not contain a constructor that has exactly one argument of type 
        /// <paramref name="serviceType"/> or <see cref="Func{T}"/> where <b>T</b> is
        /// <paramref name="serviceType"/>.</exception>
        public static void RegisterDecorator(this Container container, Type serviceType, Type decoratorType,
            Lifestyle lifestyle, Predicate<DecoratorPredicateContext> predicate)
        {
            Requires.IsNotNull(predicate, "predicate");

            container.RegisterDecoratorCore(serviceType, decoratorType, predicate, lifestyle);
        }

        /// <summary>
        /// Ensures that the supplied <paramref name="decoratorType"/> decorator is returned, wrapping the 
        /// original registered <paramref name="serviceType"/>, by injecting that service type into the 
        /// constructor of the supplied <paramref name="decoratorType"/>. Multiple decorators may be applied 
        /// to the same <paramref name="serviceType"/>. Decorators can be applied to both open, closed, and 
        /// non-generic service types. A new <paramref name="decoratorType"/> will always be returned (the
        /// <see cref="Lifestyle.Transient">Transient</see> lifestyle), independently of the lifestyle of the 
        /// wrapped service.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The <b>RegisterDecorator</b> method works by hooking onto the container's
        /// <see cref="Container.ExpressionBuilt">ExpressionBuilt</see> event. This event fires after the
        /// <see cref="Container.ResolveUnregisteredType">ResolveUnregisteredType</see> event, which allows
        /// decoration of types that are resolved using unregistered type resolution. The
        /// <see cref="OpenGenericRegistrationExtensions.RegisterOpenGeneric(Container,Type,Type,Lifestyle)">RegisterOpenGeneric</see>
        /// extension method, for instance, hooks onto the <b>ResolveUnregisteredType</b>. This allows you to
        /// use <b>RegisterDecorator</b> on the same generic service type as <b>RegisterOpenGeneric</b>.
        /// </para>
        /// <para>
        /// Multiple decorators can be applied to the same service type. The order in which they are registered
        /// is the order they get applied in. This means that the decorator that gets registered first, gets
        /// applied first, which means that the next registered decorator, will wrap the first decorator, which
        /// wraps the original service type.
        /// </para>
        /// <para>
        /// Constructor injection will be used on that type, and although it may have many constructor 
        /// arguments, it must have exactly one argument of the type of <paramref name="serviceType"/>, or an 
        /// argument of type <see cref="Func{TResult}"/> where <b>TResult</b> is <paramref name="serviceType"/>.
        /// An exception will be thrown when this is not the case.
        /// </para>
        /// <para>
        /// The registered <paramref name="decoratorType"/> may have a constructor with an argument of type
        /// <see cref="Func{T}"/> where <b>T</b> is <paramref name="serviceType"/>. In this case, an decorated
        /// instance will not injected into the <paramref name="decoratorType"/>, but it will inject a 
        /// <see cref="Func{TResult}"/> that allows creating instances of the decorated type, according to the
        /// lifestyle of that type. This enables more advanced scenarios, such as executing the decorated 
        /// types on a different thread, or executing decorated instance within a certain scope (such as a 
        /// lifetime scope).
        /// </para>
        /// </remarks>
        /// <example>
        /// The following example shows the definition of a generic <b>ICommandHandler&lt;T&gt;</b> interface,
        /// a <b>CustomerMovedCommandHandler</b> implementing that interface, and a 
        /// <b>ValidatorCommandHandlerDecorator&lt;T&gt;</b> that acts as a decorator for that interface.
        /// <code lang="cs"><![CDATA[
        /// using System.ComponentModel.DataAnnotations;
        /// using System.Diagnostics;
        /// using System.Linq;
        /// 
        /// using Microsoft.VisualStudio.TestTools.UnitTesting;
        /// 
        /// using SimpleInjector;
        /// using SimpleInjector.Extensions;
        /// 
        /// public interface ICommandHandler<TCommand>
        /// {
        ///     void Handle(TCommand command);
        /// }
        ///
        /// public class CustomerMovedCommand
        /// {
        ///     [Required]
        ///     public int CustomerId { get; set; }
        ///
        ///     [Required]
        ///     public Address Address { get; set; }
        /// }
        ///
        /// public class CustomerMovedCommandHandler
        ///     : ICommandHandler<CustomerMovedCommand>
        /// {
        ///     public void Handle(CustomerMovedCommand command)
        ///     {
        ///         // some logic
        ///     }
        /// }
        ///
        /// // Decorator that validates commands before they get executed.
        /// public class ValidatorCommandHandlerDecorator<TCommand>
        ///     : ICommandHandler<TCommand>
        /// {
        ///     private readonly ICommandHandler<TCommand> decoratedHandler;
        ///     private readonly Container container;
        ///
        ///     public ValidatorCommandHandlerDecorator(
        ///         ICommandHandler<TCommand> decoratedHandler,
        ///         Container container)
        ///     {
        ///         this.decoratedHandler = decoratedHandler;
        ///         this.container = container;
        ///     }
        ///
        ///     public void Handle(TCommand command)
        ///     {
        ///         this.Validate(command);
        ///
        ///         this.decoratedHandler.Handle(command);
        ///     }
        ///
        ///     private void Validate(TCommand command)
        ///     {
        ///         var validationContext =
        ///             new ValidationContext(command, this.container, null);
        ///
        ///         Validator.ValidateObject(command, validationContext);
        ///     }
        /// }
        /// 
        /// // Decorator that measures the time it takes to execute a command.
        /// public class MonitoringCommandHandlerDecorator<TCommand>
        ///     : ICommandHandler<TCommand>
        /// {
        ///     private readonly ICommandHandler<TCommand> decoratedHandler;
        ///     private readonly ILogger logger;
        ///
        ///     public MonitoringCommandHandlerDecorator(
        ///         ICommandHandler<TCommand> decoratedHandler,
        ///         ILogger logger)
        ///     {
        ///         this.decoratedHandler = decoratedHandler;
        ///         this.logger = logger;
        ///     }
        ///
        ///     public void Handle(TCommand command)
        ///     {
        ///         var watch = Stopwatch.StartNew();
        ///
        ///         this.decoratedHandler.Handle(command);
        ///
        ///         this.logger.Log(string.Format("{0} executed in {1} ms.",
        ///             command.GetType().Name, watch.ElapsedMilliseconds));
        ///     }
        /// }
        /// 
        /// [TestMethod]
        /// public static void TestRegisterOpenGenericDecorator()
        /// {
        ///     // Arrange
        ///     var container = new Container();
        ///
        ///     container.RegisterSingle<ILogger, DebugLogger>();
        ///
        ///     // Search the given assembly and register all concrete types that 
        ///     // implement ICommandHandler<TCommand>.
        ///     container.RegisterManyForOpenGeneric(typeof(ICommandHandler<>),
        ///         typeof(ICommandHandler<>).Assembly);
        ///
        ///     // Wrap all ICommandHandler<TCommand> service types with a decorator
        ///     // that measures and logs the duration of that handler.
        ///     container.RegisterDecorator(typeof(ICommandHandler<>),
        ///         typeof(MonitoringCommandHandlerDecorator<>));
        ///
        ///     // Wrap all ICommandHandler<TCommand> types (in this case it will
        ///     // wrap the monitoring decorator), but only if the TCommand contains
        ///     // any properties.
        ///     container.RegisterDecorator(typeof(ICommandHandler<>),
        ///         typeof(ValidatorCommandHandlerDecorator<>), context =>
        ///         {
        ///             var commandType = context.ServiceType.GetGenericArguments()[0];
        ///             bool mustDecorate = commandType.GetProperties().Any();
        ///             return mustDecorate;
        ///         });
        ///
        ///     // Act
        ///     var handler = 
        ///         container.GetInstance<ICommandHandler<CustomerMovedCommand>>();
        ///
        ///     // Assert
        ///     Assert.IsInstanceOfType(handler, 
        ///         typeof(ValidatorCommandHandlerDecorator<CustomerMovedCommand>));
        /// }
        /// ]]></code>
        /// </example>
        /// <param name="container">The container to make the registrations in.</param>
        /// <param name="serviceType">The definition of the open generic service type that will
        /// be wrapped by the given <paramref name="decoratorType"/>.</param>
        /// <param name="decoratorType">The definition of the open generic decorator type that will
        /// be used to wrap the original service type.</param>
        /// <exception cref="ArgumentNullException">Thrown when one of the arguments is a null reference.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="serviceType"/>  is not
        /// an open generic type, when <paramref name="decoratorType"/> does not inherit from or implement 
        /// <paramref name="serviceType"/>, when <paramref name="decoratorType"/> does not
        /// have a single public constructor, or when <paramref name="decoratorType"/> does not
        /// contain a constructor that has exactly one argument of type 
        /// <paramref name="serviceType"/> or <see cref="Func{T}"/> where <b>T</b> is
        /// <paramref name="serviceType"/>.</exception>
        public static void RegisterDecorator(this Container container, Type serviceType, Type decoratorType)
        {
            container.RegisterDecoratorCore(serviceType, decoratorType, null, Lifestyle.Transient);
        }

        /// <summary>
        /// Ensures that the supplied <paramref name="decoratorType"/> decorator is returned when the supplied
        /// <paramref name="predicate"/> returns <b>true</b>, wrapping the original registered 
        /// <paramref name="serviceType"/>, by injecting that service type into the constructor of the 
        /// supplied <paramref name="decoratorType"/>. Multiple decorators may be applied to the same 
        /// <paramref name="serviceType"/>. Decorators can be applied to both open, closed, and non-generic 
        /// service types. A new <paramref name="decoratorType"/> will always be returned (the
        /// <see cref="Lifestyle.Transient">Transient</see> lifestyle), independently of the lifestyle of the 
        /// wrapped service.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The <b>RegisterOpenGenericDecorator</b> method works by hooking onto the container's
        /// <see cref="Container.ExpressionBuilt">ExpressionBuilt</see> event. This event fires after the
        /// <see cref="Container.ResolveUnregisteredType">ResolveUnregisteredType</see> event, which allows
        /// decoration of types that are resolved using unregistered type resolution. The
        /// <see cref="OpenGenericRegistrationExtensions.RegisterOpenGeneric(Container,Type,Type,Lifestyle)">RegisterOpenGeneric</see>
        /// extension method, for instance, hooks onto the <b>ResolveUnregisteredType</b>. This allows you to
        /// use <b>RegisterOpenGenericDecorator</b> on the same service type as <b>RegisterOpenGeneric</b>.
        /// </para>
        /// <para>
        /// Multiple decorators can be applied to the same service type. The order in which they are registered
        /// is the order they get applied in. This means that the decorator that gets registered first, gets
        /// applied first, which means that the next registered decorator, will wrap the first decorator, which
        /// wraps the original service type.
        /// </para>
        /// <para>
        /// Constructor injection will be used on that type, and although it may have many constructor 
        /// arguments, it must have exactly one argument of the type of <paramref name="serviceType"/>, or an 
        /// argument of type <see cref="Func{TResult}"/> where <b>TResult</b> is <paramref name="serviceType"/>.
        /// An exception will be thrown when this is not the case.
        /// </para>
        /// <para>
        /// The registered <paramref name="decoratorType"/> may have a constructor with an argument of type
        /// <see cref="Func{T}"/> where <b>T</b> is <paramref name="serviceType"/>. In this case, the
        /// will not inject the decorated <paramref name="serviceType"/> itself into the 
        /// <paramref name="decoratorType"/> instance, but it will inject a <see cref="Func{T}"/> that allows
        /// creating instances of the decorated type, according to the lifestyle of that type. This enables
        /// more advanced scenarios, such as executing the decorated types on a different thread, or executing
        /// decorated instance within a certain scope (such as a lifetime scope).
        /// </para>
        /// </remarks>
        /// <example>
        /// Please see the <see cref="RegisterDecorator(Container, Type, Type)">RegisterDecorator</see> method
        /// for more information.
        /// </example>
        /// <param name="container">The container to make the registrations in.</param>
        /// <param name="serviceType">The definition of the open generic service type that will
        /// be wrapped by the given <paramref name="decoratorType"/>.</param>
        /// <param name="decoratorType">The definition of the open generic decorator type that will
        /// be used to wrap the original service type.</param>
        /// <param name="predicate">The predicate that determines whether the 
        /// <paramref name="decoratorType"/> must be applied to a service type.</param>
        /// <exception cref="ArgumentNullException">Thrown when one of the arguments is a null reference.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="serviceType"/> is not
        /// an open generic type, when <paramref name="decoratorType"/> does not inherit from or 
        /// implement <paramref name="serviceType"/>, when <paramref name="decoratorType"/>
        /// does not have a single public constructor, or when <paramref name="decoratorType"/> does 
        /// not contain a constructor that has exactly one argument of type 
        /// <paramref name="serviceType"/> or <see cref="Func{T}"/> where <b>T</b> is
        /// <paramref name="serviceType"/>.</exception>
        public static void RegisterDecorator(this Container container, Type serviceType, Type decoratorType,
            Predicate<DecoratorPredicateContext> predicate)
        {
            Requires.IsNotNull(predicate, "predicate");

            container.RegisterDecoratorCore(serviceType, decoratorType, predicate, Lifestyle.Transient);
        }

        /// <summary>
        /// Ensures that a single instance of the supplied <paramref name="decoratorType"/> decorator is 
        /// returned, wrapping the original registered <paramref name="serviceType"/>, by injecting that 
        /// service type into the constructor of the supplied <paramref name="decoratorType"/>. Multiple 
        /// decorators may be applied to the same <paramref name="serviceType"/>. Decorators can be applied 
        /// to both open, closed, and non-generic service types.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This method ensures that a single instance of the supplied <paramref name="decoratorType"/> is
        /// returned, no matter what the lifestyle of the wrapped service type is. Use with care, because the
        /// wrapped service type will also become a singleton. This method is especially useful when use for
        /// injecting <see cref="Func{T}"/> factory methods, which will allow the wrapped service type to get
        /// it's own lifestyle back.
        /// </para>
        /// </remarks>
        /// <example>
        /// Please see the <see cref="RegisterDecorator(Container, Type, Type)">RegisterDecorator</see> method
        /// for more information.
        /// </example>
        /// <param name="container">The container to make the registrations in.</param>
        /// <param name="serviceType">The definition of the open generic service type that will
        /// be wrapped by the given <paramref name="decoratorType"/>.</param>
        /// <param name="decoratorType">The definition of the open generic decorator type that will
        /// be used to wrap the original service type.</param>
        /// <exception cref="ArgumentNullException">Thrown when one of the arguments is a null reference.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="serviceType"/>  is not
        /// an open generic type, when <paramref name="decoratorType"/> does not inherit from or implement 
        /// <paramref name="serviceType"/>, when <paramref name="decoratorType"/> does not
        /// have a single public constructor, or when <paramref name="decoratorType"/> does not
        /// contain a constructor that has exactly one argument of type 
        /// <paramref name="serviceType"/> or <see cref="Func{T}"/> where <b>T</b> is
        /// <paramref name="serviceType"/>.</exception>
        public static void RegisterSingleDecorator(this Container container, Type serviceType,
            Type decoratorType)
        {
            container.RegisterDecoratorCore(serviceType, decoratorType, null, Lifestyle.Singleton);
        }

        /// <summary>
        /// Ensures that the supplied <paramref name="decoratorType"/> decorator is returned when the supplied
        /// <paramref name="predicate"/> returns <b>true</b>, wrapping the original registered 
        /// <paramref name="serviceType"/>, by injecting that service type into the constructor of the 
        /// supplied <paramref name="decoratorType"/>. Multiple decorators may be applied to the same 
        /// <paramref name="serviceType"/>. Decorators can be applied to both open, closed, and non-generic 
        /// service types.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This method ensures that a single instance of the supplied <paramref name="decoratorType"/> is
        /// returned, no matter what the lifestyle of the wrapped service type is. Use with care, because the
        /// wrapped service type will also become a singleton. This method is especially useful when use for
        /// injecting <see cref="Func{T}"/> factory methods, which will allow the wrapped service type to get
        /// it's own lifestyle back.
        /// </para>
        /// </remarks>
        /// <example>
        /// Please see the <see cref="RegisterDecorator(Container, Type, Type)">RegisterDecorator</see> method
        /// for more information.
        /// </example>
        /// <param name="container">The container to make the registrations in.</param>
        /// <param name="serviceType">The definition of the open generic service type that will
        /// be wrapped by the given <paramref name="decoratorType"/>.</param>
        /// <param name="decoratorType">The definition of the open generic decorator type that will
        /// be used to wrap the original service type.</param>
        /// <param name="predicate">The predicate that determines whether the 
        /// <paramref name="decoratorType"/> must be applied to a service type.</param>
        /// <exception cref="ArgumentNullException">Thrown when one of the arguments is a null reference.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="serviceType"/> is not
        /// an open generic type, when <paramref name="decoratorType"/> does not inherit from or 
        /// implement <paramref name="serviceType"/>, when <paramref name="decoratorType"/>
        /// does not have a single public constructor, or when <paramref name="decoratorType"/> does 
        /// not contain a constructor that has exactly one argument of type 
        /// <paramref name="serviceType"/> or <see cref="Func{T}"/> where <b>T</b> is
        /// <paramref name="serviceType"/>.</exception>
        public static void RegisterSingleDecorator(this Container container, Type serviceType,
            Type decoratorType, Predicate<DecoratorPredicateContext> predicate)
        {
            Requires.IsNotNull(predicate, "predicate");

            container.RegisterDecoratorCore(serviceType, decoratorType, predicate, Lifestyle.Singleton);
        }

        private static void RegisterDecoratorCore(this Container container, Type serviceType,
            Type decoratorType, Predicate<DecoratorPredicateContext> predicate, Lifestyle lifestyle)
        {
            VerifyMethodArguments(container, serviceType, decoratorType, lifestyle);

            var data = new DecoratorExpressionInterceptorData(container, serviceType, decoratorType,
                predicate, lifestyle);

            container.ExpressionBuilt += new DecoratorInterceptor(data).ExpressionBuilt;
        }

        private static void VerifyMethodArguments(Container container, Type serviceType, Type decoratorType,
            Lifestyle lifestyle)
        {
            Requires.IsNotNull(container, "container");
            Requires.IsNotNull(serviceType, "serviceType");
            Requires.IsNotNull(decoratorType, "decoratorType");
            Requires.IsNotNull(lifestyle, "lifestyle");

            Requires.ServiceTypeIsNotClosedWhenImplementationIsOpen(serviceType, decoratorType);
            Requires.ServiceOrItsGenericTypeDefinitionIsAssignableFromImplementation(serviceType, 
                decoratorType, "serviceType");

            Requires.ImplementationHasSelectableConstructor(container, serviceType, decoratorType, 
                "decoratorType");
            Requires.IsDecorator(container, serviceType, decoratorType, "decoratorType");
            Requires.DecoratorIsNotAnOpenGenericTypeDefinitionWhenTheServiceTypeIsNot(serviceType,
                decoratorType, "decoratorType");
            Requires.OpenGenericTypeDoesNotContainUnresolvableTypeArguments(serviceType, decoratorType, 
                "decoratorType");
        }
    }
}