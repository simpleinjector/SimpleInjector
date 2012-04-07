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
    using System.Collections.ObjectModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq.Expressions;

    /// <summary>
    /// Extension methods for applying generic decorators.
    /// </summary>
    public static class GenericDecoratorExtensions
    {
        /// <summary>
        /// Ensures that a closed generic version of the supplied <paramref name="openGenericDecorator"/> 
        /// decorator is returned, wrapping the original closed generic version of the registered
        /// <paramref name="openGenericServiceType"/>, by injecting that service type into the constructor
        /// of the supplied <paramref name="openGenericDecorator"/>. Multiple decorators may be applied to the
        /// same <paramref name="openGenericServiceType"/>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The <b>RegisterOpenGenericDecorator</b> method works by hooking onto the container's
        /// <see cref="Container.ExpressionBuilt">ExpressionBuilt</see> event. This event fires after the
        /// <see cref="Container.ResolveUnregisteredType">ResolveUnregisteredType</see> event, which allows
        /// decoration of types that are resolved using unregistered type resolution. The
        /// <see cref="OpenGenericRegistrationExtensions.RegisterOpenGeneric">RegisterOpenGeneric</see>
        /// extension method, for instance, hooks onto the <b>ResolveUnregisteredType</b>. This allows you to
        /// use <b>RegisterOpenGenericDecorator</b> on the same service type as <b>RegisterOpenGeneric</b>.
        /// </para>
        /// <para>
        /// Multiple decorators can be applied to the same service type. The order in which they are registered
        /// is the order they get applied in. This means that the decorator that gets registered first, gets
        /// applied first, which means that the next registered decorator, will wrap the first decorator, which
        /// wraps the original service type.
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
        ///     container.RegisterOpenGenericDecorator(typeof(ICommandHandler<>),
        ///         typeof(MonitoringCommandHandlerDecorator<>));
        ///
        ///     // Wrap all ICommandHandler<TCommand> types (in this case it will
        ///     // wrap the monitoring decorator), but only if the TCommand contains
        ///     // any properties.
        ///     container.RegisterOpenGenericDecorator(typeof(ICommandHandler<>),
        ///         typeof(ValidatorCommandHandlerDecorator<>), c =>
        ///         {
        ///             var commandType = c.ServiceType.GetGenericArguments()[0];
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
        /// <param name="openGenericServiceType">The definition of the open generic service type that will
        /// be wrapped by the given <paramref name="openGenericDecorator"/>.</param>
        /// <param name="openGenericDecorator">The definition of the open generic decorator type that will
        /// be used to wrap the original service type.</param>
        /// <exception cref="ArgumentNullException">Thrown when either <paramref name="container"/>,
        /// <paramref name="openGenericServiceType"/>, or <paramref name="openGenericDecorator"/> are null
        /// references.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="openGenericServiceType"/>  is not
        /// an open generic type, when <paramref name="openGenericDecorator"/> does not inherit from or implement 
        /// <paramref name="openGenericServiceType"/>, when <paramref name="openGenericDecorator"/> does not
        /// have a single public constructor, or when <paramref name="openGenericDecorator"/> does not
        /// contain a constructor that has exactly one argument of type 
        /// <paramref name="openGenericServiceType"/>.</exception>
#if !DEBUG
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        [Obsolete("This method is obsolete. Please use the " +
            "SimpleInjector.Extensions.DecoratorExtensions.RegisterDecorator extension method instead.")]
#endif
        public static void RegisterOpenGenericDecorator(this Container container,
            Type openGenericServiceType, Type openGenericDecorator)
        {
            Requires.TypeIsOpenGeneric(openGenericServiceType, "openGenericServiceType");

            container.RegisterDecorator(openGenericServiceType, openGenericDecorator);
        }

        /// <summary>
        /// Ensures that a closed generic version of the supplied <paramref name="openGenericDecorator"/> 
        /// decorator is returned, wrapping the original closed generic version of the registered
        /// <paramref name="openGenericServiceType"/>, by injecting that service type into the constructor
        /// of the supplied <paramref name="openGenericDecorator"/>. Multiple decorators may be applied to the
        /// same <paramref name="openGenericServiceType"/>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The <b>RegisterOpenGenericDecorator</b> method works by hooking onto the container's
        /// <see cref="Container.ExpressionBuilt">ExpressionBuilt</see> event. This event fires after the
        /// <see cref="Container.ResolveUnregisteredType">ResolveUnregisteredType</see> event, which allows
        /// decoration of types that are resolved using unregistered type resolution. The
        /// <see cref="OpenGenericRegistrationExtensions.RegisterOpenGeneric">RegisterOpenGeneric</see>
        /// extension method, for instance, hooks onto the <b>ResolveUnregisteredType</b>. This allows you to
        /// use <b>RegisterOpenGenericDecorator</b> on the same service type as <b>RegisterOpenGeneric</b>.
        /// </para>
        /// <para>
        /// Multiple decorators can be applied to the same service type. The order in which they are registered
        /// is the order they get applied in. This means that the decorator that gets registered first, gets
        /// applied first, which means that the next registered decorator, will wrap the first decorator, which
        /// wraps the original service type.
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
        ///     container.RegisterOpenGenericDecorator(typeof(ICommandHandler<>),
        ///         typeof(MonitoringCommandHandlerDecorator<>));
        ///
        ///     // Wrap all ICommandHandler<TCommand> types (in this case it will
        ///     // wrap the monitoring decorator), but only if the TCommand contains
        ///     // any properties.
        ///     container.RegisterOpenGenericDecorator(typeof(ICommandHandler<>),
        ///         typeof(ValidatorCommandHandlerDecorator<>), c =>
        ///         {
        ///             var commandType = c.ServiceType.GetGenericArguments()[0];
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
        /// <param name="openGenericServiceType">The definition of the open generic service type that will
        /// be wrapped by the given <paramref name="openGenericDecorator"/>.</param>
        /// <param name="openGenericDecorator">The definition of the open generic decorator type that will
        /// be used to wrap the original service type.</param>
        /// <param name="predicate">The predicate that determines whether the 
        /// <paramref name="openGenericDecorator"/> must be applied to a service type.</param>
        /// <exception cref="ArgumentNullException">Thrown when either <paramref name="container"/>,
        /// <paramref name="openGenericServiceType"/>, <paramref name="openGenericDecorator"/>, or
        /// <paramref name="predicate"/> are null references.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="openGenericServiceType"/> is not
        /// an open generic type, when <paramref name="openGenericDecorator"/> does not inherit from or 
        /// implement <paramref name="openGenericServiceType"/>, when <paramref name="openGenericDecorator"/>
        /// does not have a single public constructor, or when <paramref name="openGenericDecorator"/> does 
        /// not contain a constructor that has exactly one argument of type 
        /// <paramref name="openGenericServiceType"/>.</exception>
#if !DEBUG
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        [Obsolete("This method is obsolete. Please use the " +
            "SimpleInjector.Extensions.DecoratorExtensions.RegisterDecorator extension method instead.")]
#endif
        public static void RegisterOpenGenericDecorator(this Container container,
            Type openGenericServiceType, Type openGenericDecorator,
            Predicate<PredicateContext> predicate)
        {
            Requires.TypeIsOpenGeneric(openGenericServiceType, "openGenericServiceType");

            container.RegisterDecorator(openGenericServiceType, openGenericDecorator,
                context => predicate(ConvertToPredicateContext(context)));
        }

        private static PredicateContext ConvertToPredicateContext(DecoratorPredicateContext context)
        {
            return new PredicateContext
            {
                ServiceType = context.ServiceType,
                ImplementationType = context.ImplementationType,
                AppliedDecorators = context.AppliedDecorators,
                Expression = context.Expression,
            };
        }

        /// <summary>
        /// An instance of this type will be supplied to the <see cref="Predicate{T}"/>
        /// delegate that is that is supplied to the 
        /// <see cref="GenericDecoratorExtensions.RegisterOpenGenericDecorator(Container, Type, Type, Predicate{PredicateContext})">RegisterOpenGenericDecorator</see>
        /// overload that takes this delegate. This type contains information about the decoration that is about
        /// to be applied and it allows users to examine the given instance to see whether the decorator should
        /// be applied or not.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible", Justification =
            "Users will never have to spell out GenericDecoratorExtensions.PredicateContext, since this " +
            "type is supplied as input argument of a Predicate<T>.")]
        public sealed class PredicateContext
        {
            internal PredicateContext()
            {
            }

            /// <summary>
            /// Gets the closed generic service type for which the decorator is about to be applied. The original
            /// service type will be returned, even if other decorators have already been applied to this type.
            /// </summary>
            /// <value>The closed generic service type.</value>
            public Type ServiceType { get; internal set; }

            /// <summary>
            /// Gets the type of the implementation that is created by the container and for which the decorator
            /// is about to be applied. The original implementation type will be returned, even if other decorators
            /// have already been applied to this type. Please not that the implementation type can not always be
            /// determined. In that case the closed generic service type will be returned.
            /// </summary>
            /// <value>The implementation type.</value>
            public Type ImplementationType { get; internal set; }

            /// <summary>
            /// Gets the list of the types of decorators that have already been applied to this instance.
            /// </summary>
            /// <value>The applied decorators.</value>
            public ReadOnlyCollection<Type> AppliedDecorators { get; internal set; }

            /// <summary>
            /// Gets the current <see cref="Expression"/> object that describes the intention to create a new
            /// instance with its currently applied decorators.
            /// </summary>
            /// <value>The current expression that is about to be decorated.</value>
            public Expression Expression { get; internal set; }
        }
    }
}