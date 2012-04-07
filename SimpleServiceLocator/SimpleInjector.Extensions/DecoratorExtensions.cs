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
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;

    /// <summary>
    /// Extension methods for applying decorators.
    /// </summary>
    public static class DecoratorExtensions
    {
        /// <summary>
        /// Ensures that the supplied <paramref name="decoratorType"/> decorator is returned, wrapping the 
        /// original registered <paramref name="serviceType"/>, by injecting that service type into the 
        /// constructor of the supplied <paramref name="decoratorType"/>. Multiple decorators may be applied 
        /// to the same <paramref name="serviceType"/>. Decorators can be applied to both open, closed, and 
        /// non-generic service types.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The <b>RegisterDecorator</b> method works by hooking onto the container's
        /// <see cref="Container.ExpressionBuilt">ExpressionBuilt</see> event. This event fires after the
        /// <see cref="Container.ResolveUnregisteredType">ResolveUnregisteredType</see> event, which allows
        /// decoration of types that are resolved using unregistered type resolution. The
        /// <see cref="OpenGenericRegistrationExtensions.RegisterOpenGeneric">RegisterOpenGeneric</see>
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
        /// The registered <paramref name="decoratorType"/> must have a single public constructor. Constructor
        /// injection will be used on that type, and although it may have many constructor arguments, it must
        /// have exactly one argument of the type of <paramref name="serviceType"/>. An exception will be
        /// thrown when this is not the case.
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
        /// <exception cref="ArgumentNullException">Thrown when either <paramref name="container"/>,
        /// <paramref name="serviceType"/>, or <paramref name="decoratorType"/> are null
        /// references.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="serviceType"/>  is not
        /// an open generic type, when <paramref name="decoratorType"/> does not inherit from or implement 
        /// <paramref name="serviceType"/>, when <paramref name="decoratorType"/> does not
        /// have a single public constructor, or when <paramref name="decoratorType"/> does not
        /// contain a constructor that has exactly one argument of type 
        /// <paramref name="serviceType"/>.</exception>
        public static void RegisterDecorator(this Container container, Type serviceType, Type decoratorType)
        {
            container.RegisterDecoratorCore(serviceType, decoratorType, null);
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
        /// <param name="predicate">The predicate that determines whether the 
        /// <paramref name="decoratorType"/> must be applied to a service type.</param>
        /// <exception cref="ArgumentNullException">Thrown when either <paramref name="container"/>,
        /// <paramref name="serviceType"/>, <paramref name="decoratorType"/>, or
        /// <paramref name="predicate"/> are null references.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="serviceType"/> is not
        /// an open generic type, when <paramref name="decoratorType"/> does not inherit from or 
        /// implement <paramref name="serviceType"/>, when <paramref name="decoratorType"/>
        /// does not have a single public constructor, or when <paramref name="decoratorType"/> does 
        /// not contain a constructor that has exactly one argument of type 
        /// <paramref name="serviceType"/>.</exception>
        public static void RegisterDecorator(this Container container, Type serviceType, Type decoratorType,
            Predicate<DecoratorPredicateContext> predicate)
        {
            Requires.IsNotNull(predicate, "predicate");

            container.RegisterDecoratorCore(serviceType, decoratorType, predicate);
        }

        private static void RegisterDecoratorCore(this Container container, Type serviceType, Type decoratorType,
            Predicate<DecoratorPredicateContext> predicate)
        {
            VerifyMethodArguments(container, serviceType, decoratorType);

            var interceptor = new DecoratorExpressionInterceptor
            {
                Container = container,
                ServiceType = serviceType,
                Decorator = decoratorType,
                Predicate = predicate
            };

            container.ExpressionBuilt += interceptor.Decorate;
        }

        private static void VerifyMethodArguments(Container container, Type serviceType, Type decoratorType)
        {
            Requires.IsNotNull(container, "container");
            Requires.IsNotNull(serviceType, "serviceType");
            Requires.IsNotNull(decoratorType, "decoratorType");
            Requires.ServiceIsAssignableFromImplementation(serviceType, decoratorType, "serviceType");
            Requires.ContainsOneSinglePublicConstructor(decoratorType, "decoratorType");
            Requires.DecoratorHasConstructorThatContainsServiceTypeAsArgument(decoratorType, serviceType,
                "decoratorType");
            Requires.DecoratorIsNotAnOpenGenericTypeDefinitionWhenTheServiceTypeIsNot(serviceType,
                decoratorType, "decoratorType");
        }

        private sealed class DecoratorExpressionInterceptor
        {
            // Store a ServiceTypeDecoratorInfo object per closed service type. We have a dictionary per
            // thread for thread-safety. We need a dictionary per thread, since the ExpressionBuilt event can
            // get raised by multiple threads at the same time (especially for types resolved using
            // unregistered type resolution) and using the same dictionary could lead to duplicate entries
            // in the ServiceTypeDecoratorInfo.AppliedDecorators list. Because the ExpressionBuilt event gets 
            // raised and all delegates registered to that event will get called on the same thread and before
            // an InstanceProducer stores the Expression, we can safely store this information in a 
            // thread-static field.
            [ThreadStatic]
            private static Dictionary<Container, Dictionary<Type, ServiceTypeDecoratorInfo>>
                threadStaticServiceTypePredicateCache;

            internal Container Container { get; set; }

            internal Type ServiceType { get; set; }

            internal Type Decorator { get; set; }

            internal Predicate<DecoratorPredicateContext> Predicate { get; set; }

            public void Decorate(object sender, ExpressionBuiltEventArgs e)
            {
                Type decoratorType;

                if (this.MustDecorate(e, out decoratorType))
                {
                    var parameters = this.BuildParameters(decoratorType, e);

                    var ctor = decoratorType.GetConstructors().Single();

                    var serviceInfo = this.GetServiceTypeInfo(e);

                    // Add the decorator to the list of applied decorator. This way users can use this
                    // information in the predicate of the next decorator they add.
                    serviceInfo.AppliedDecorators.Add(decoratorType);

                    e.Expression = this.BuildDecoratorExpression(ctor, parameters);
                }
            }

            // This method must be public because of Silverlight Sandbox restrictions.
            [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode",
                Justification = "The method is called by the framwork using reflection.")]
            public Func<TImplementation, TImplementation> BuildFuncInitializer<TImplementation>()
            {
                var instanceInitializer = this.Container.GetInitializer<TImplementation>();

                if (instanceInitializer != null)
                {
                    return instance =>
                    {
                        instanceInitializer(instance);

                        return instance;
                    };
                }

                return null;
            }

            private bool MustDecorate(ExpressionBuiltEventArgs e, out Type decoratorType)
            {
                decoratorType = null;

                if (this.ServiceType == e.RegisteredServiceType)
                {
                    decoratorType = this.Decorator;
                }
                else if (!this.ServiceType.IsGenericTypeDefinitionOf(e.RegisteredServiceType))
                {
                    return false;
                }
                else
                {
                    var results = this.BuildClosedGenericImplementation(e.RegisteredServiceType);

                    if (!results.ClosedServiceTypeSatisfiesAllTypeConstraints)
                    {
                        return false;
                    }

                    decoratorType = results.ClosedGenericImplementation;
                }

                if (!this.SatisfiesPredicate(e))
                {
                    decoratorType = null;
                    return false;
                }

                return true;
            }

            private GenericTypeBuilder.BuildResult BuildClosedGenericImplementation(Type serviceType)
            {
                var builder = new GenericTypeBuilder(serviceType, this.Decorator);

                return builder.BuildClosedGenericImplementation();
            }

            private bool SatisfiesPredicate(ExpressionBuiltEventArgs e)
            {
                return this.Predicate == null || this.Predicate(this.CreatePredicateContext(e));
            }

            private DecoratorPredicateContext CreatePredicateContext(ExpressionBuiltEventArgs e)
            {
                var info = this.GetServiceTypeInfo(e);

                return new DecoratorPredicateContext
                {
                    ServiceType = e.RegisteredServiceType,
                    ImplementationType = info.ImplementationType,
                    AppliedDecorators = info.AppliedDecorators.ToList().AsReadOnly(),
                    Expression = e.Expression,
                };
            }

            private ServiceTypeDecoratorInfo GetServiceTypeInfo(ExpressionBuiltEventArgs e)
            {
                var predicateCache = this.GetServiceTypePredicateCache();

                if (!predicateCache.ContainsKey(e.RegisteredServiceType))
                {
                    Type implementationType = DetermineImplementationType(e);

                    predicateCache[e.RegisteredServiceType] = new ServiceTypeDecoratorInfo(implementationType);
                }

                return predicateCache[e.RegisteredServiceType];
            }

            private Dictionary<Type, ServiceTypeDecoratorInfo> GetServiceTypePredicateCache()
            {
                var predicateCache = threadStaticServiceTypePredicateCache;

                if (predicateCache == null)
                {
                    predicateCache = new Dictionary<Container, Dictionary<Type, ServiceTypeDecoratorInfo>>();

                    threadStaticServiceTypePredicateCache = predicateCache;
                }

                if (!predicateCache.ContainsKey(this.Container))
                {
                    predicateCache[this.Container] = new Dictionary<Type, ServiceTypeDecoratorInfo>();
                }

                return predicateCache[this.Container];
            }

            private static Type DetermineImplementationType(ExpressionBuiltEventArgs e)
            {
                if (e.Expression is ConstantExpression)
                {
                    // Singleton
                    return ((ConstantExpression)e.Expression).Value.GetType();
                }

                if (e.Expression is NewExpression)
                {
                    // Transient without initializers.
                    return ((NewExpression)e.Expression).Constructor.DeclaringType;
                }

                var invocation = e.Expression as InvocationExpression;

                if (invocation != null && invocation.Expression is ConstantExpression &&
                    invocation.Arguments.Count == 1 && invocation.Arguments[0] is NewExpression)
                {
                    // Transient with initializers.
                    return ((NewExpression)invocation.Arguments[0]).Constructor.DeclaringType;
                }

                // Implementation type can not be determined.
                return e.RegisteredServiceType;
            }

            private Expression[] BuildParameters(Type closedGenericDecorator, ExpressionBuiltEventArgs e)
            {
                var parameters =
                    from parameter in closedGenericDecorator.GetConstructors().Single().GetParameters()
                    select this.BuildExpressionForParameter(parameter, e);

                try
                {
                    return parameters.ToArray();
                }
                catch (ActivationException ex)
                {
                    // Build a more expressive exception message.
                    string message =
                        StringResources.ErrorWhileTryingToGetInstanceOfType(closedGenericDecorator, ex.Message);

                    throw new ActivationException(message, ex);
                }
            }

            private Expression BuildExpressionForParameter(ParameterInfo parameter,
                ExpressionBuiltEventArgs e)
            {
                if (parameter.ParameterType == e.RegisteredServiceType)
                {
                    return e.Expression;
                }

                return this.Container.GetRegistration(parameter.ParameterType, true).BuildExpression();
            }

            private Expression BuildDecoratorExpression(ConstructorInfo ctor, Expression[] parameters)
            {
                var instanceInitializer =
                    this.GetType().GetMethod("BuildFuncInitializer").MakeGenericMethod(ctor.DeclaringType)
                    .Invoke(this, null);

                var newInstanceExpression = Expression.New(ctor, parameters);

                if (instanceInitializer != null)
                {
                    // It's not possible to return a Expression that is as heavily optimized as the
                    // Expression.New simply is, because the instance initializer must be called as well.
                    return Expression.Invoke(Expression.Constant(instanceInitializer), newInstanceExpression);
                }

                return newInstanceExpression;
            }

            private sealed class ServiceTypeDecoratorInfo
            {
                public ServiceTypeDecoratorInfo(Type implementationType)
                {
                    this.ImplementationType = implementationType;
                    this.AppliedDecorators = new List<Type>();
                }

                public Type ImplementationType { get; private set; }

                public List<Type> AppliedDecorators { get; private set; }
            }
        }
    }
}