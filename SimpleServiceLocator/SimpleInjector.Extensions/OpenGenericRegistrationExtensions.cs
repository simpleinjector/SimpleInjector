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
    using System.Linq.Expressions;

    using SimpleInjector;

    /// <summary>
    /// Provides a set of static (Shared in Visual Basic) methods for registration of open generic service
    /// types in the <see cref="Container"/>.
    /// </summary>
    public static class OpenGenericRegistrationExtensions
    {
        /// <summary>
        /// Registers that a new instance of <paramref name="openGenericImplementation"/> will be returned 
        /// every time a <paramref name="openGenericServiceType"/> is requested.
        /// </summary>
        /// <example>
        /// The following example shows the definition of a generic <b>IValidator&lt;T&gt;</b> interface
        /// and, a <b>NullValidator&lt;T&gt;</b> implementation and a specific validator for Orders.
        /// The registration ensures a <b>OrderValidator</b> is returned when a 
        /// <b>IValidator&lt;Order&gt;</b> is requested. For all requests for a 
        /// <b>IValidator&lt;T&gt;</b> other than a <b>IValidator&lt;Order&gt;</b>, an 
        /// implementation of <b>NullValidator&lt;T&gt;</b> will be returned.
        /// <code lang="cs"><![CDATA[
        /// public interface IValidator<T>
        /// { 
        ///     void Validate(T instance);
        /// }
        /// 
        /// public class NullValidator<T> : IValidator<T>
        /// {
        ///     public void Validate(T instance)
        ///     {
        ///     }
        /// }
        /// 
        /// public class OrderValidator : IValidator<Order>
        /// {
        ///     public void Validate(Order instance)
        ///     {
        ///         if (instance.Total < 0)
        ///         {
        ///             throw new ValidationException("Total can not be negative.");
        ///         }
        ///     }
        /// }
        /// 
        /// [TestMethod]
        /// public static void TestRegisterOpenGeneric()
        /// {
        ///     // Arrange
        ///     var container = new Container();
        ///     
        ///     container.Register<IValidator<Order>, OrderValidator>();
        ///     container.RegisterOpenGeneric(typeof(IValidator<>), typeof(NullValidator<>));
        ///     
        ///     // Act
        ///     var orderValidator = container.GetInstance<IValidator<Order>>();
        ///     var customerValidator = container.GetInstance<IValidator<Customer>>();
        ///     var productValidator = container.GetInstance<IValidator<Product>>();
        /// 
        ///     // Assert
        ///     Assert.IsInstanceOfType(orderValidator, typeof(OrderValidator));
        ///     Assert.IsInstanceOfType(customerValidator, typeof(NullValidator<Customer>));
        ///     Assert.IsInstanceOfType(productValidator, typeof(NullValidator<Product>));
        /// }
        /// ]]></code>
        /// </example>
        /// <param name="container">The container to make the registrations in.</param>
        /// <param name="openGenericServiceType">The definition of the open generic service type that can be 
        /// used to retrieve instances.</param>
        /// <param name="openGenericImplementation">The definition of the open generic implementation type
        /// that will be returned when a <paramref name="openGenericServiceType"/> is requested.</param>
        public static void RegisterOpenGeneric(this Container container,
            Type openGenericServiceType, Type openGenericImplementation)
        {
            Requires.TypeIsOpenGeneric(openGenericServiceType, "openGenericServiceType");
            Requires.TypeIsOpenGeneric(openGenericImplementation, "openGenericImplementation");
            Requires.ServiceIsAssignableFromImplementation(openGenericServiceType, openGenericImplementation,
                "openGenericServiceType");
            Requires.ImplementationHasSelectableConstructor(container, openGenericServiceType,
                openGenericImplementation, "openGenericImplementation");

            var transientResolver = new TransientOpenGenericResolver
            {
                OpenGenericServiceType = openGenericServiceType,
                OpenGenericImplementation = openGenericImplementation,
                Container = container
            };

            container.ResolveUnregisteredType += transientResolver.ResolveOpenGeneric;
        }

        /// <summary>
        /// Registers that the same instance of <paramref name="openGenericImplementation"/> will be returned 
        /// every time a <paramref name="openGenericServiceType"/> is requested.
        /// </summary>
        /// <example>
        /// The following example shows the definition of a generic <b>IValidator&lt;T&gt;</b> interface
        /// and, a <b>NullValidator&lt;T&gt;</b> implementation and a specific validator for Orders.
        /// The registration ensures a <b>OrderValidator</b> is returned when a 
        /// <b>IValidator&lt;Order&gt;</b> is requested. For all requests for a 
        /// <b>IValidator&lt;T&gt;</b> other than a <b>IValidator&lt;Order&gt;</b>, an 
        /// implementation of <b>NullValidator&lt;T&gt;</b> will be returned.
        /// <code lang="cs"><![CDATA[
        /// public interface IValidator<T>
        /// { 
        ///     void Validate(T instance);
        /// }
        /// 
        /// public class NullValidator<T> : IValidator<T>
        /// {
        ///     public void Validate(T instance)
        ///     {
        ///     }
        /// }
        /// 
        /// public class OrderValidator : IValidator<Order>
        /// {
        ///     public void Validate(Order instance)
        ///     {
        ///         if (instance.Total < 0)
        ///         {
        ///             throw new ValidationException("Total can not be negative.");
        ///         }
        ///     }
        /// }
        /// 
        /// [TestMethod]
        /// public static void TestRegisterOpenGeneric()
        /// {
        ///     // Arrange
        ///     var container = new Container();
        ///     
        ///     container.RegisterSingle<IValidator<Order>, OrderValidator>();
        ///     container.RegisterSingleOpenGeneric(typeof(IValidator<>), typeof(NullValidator<>));
        ///     
        ///     // Act
        ///     var orderValidator = container.GetInstance<IValidator<Order>>();
        ///     var customerValidator = container.GetInstance<IValidator<Customer>>();
        ///     var productValidator = container.GetInstance<IValidator<Product>>();
        /// 
        ///     // Assert
        ///     Assert.IsInstanceOfType(orderValidator, typeof(OrderValidator));
        ///     Assert.IsInstanceOfType(customerValidator, typeof(NullValidator<Customer>));
        ///     Assert.IsInstanceOfType(productValidator, typeof(NullValidator<Product>));
        /// }
        /// ]]></code>
        /// </example>
        /// <param name="container">The container to make the registrations in.</param>
        /// <param name="openGenericServiceType">The definition of the open generic service type that can be 
        /// used to retrieve instances..</param>
        /// <param name="openGenericImplementation">The definition of the open generic implementation type
        /// that will be returned when a <paramref name="openGenericServiceType"/> is requested.</param>
        public static void RegisterSingleOpenGeneric(this Container container,
            Type openGenericServiceType, Type openGenericImplementation)
        {
            Requires.TypeIsOpenGeneric(openGenericServiceType, "openGenericServiceType");
            Requires.TypeIsOpenGeneric(openGenericImplementation, "openGenericImplementation");
            Requires.ServiceIsAssignableFromImplementation(openGenericServiceType, openGenericImplementation,
                "openGenericServiceType");
            Requires.ImplementationHasSelectableConstructor(container, openGenericServiceType,
                openGenericImplementation, "openGenericImplementation");

            var singletonResolver = new SingleOpenGenericResolver
            {
                OpenGenericServiceType = openGenericServiceType,
                OpenGenericImplementation = openGenericImplementation,
                Container = container
            };

            container.ResolveUnregisteredType += singletonResolver.ResolveOpenGeneric;
        }

        /// <summary>Resolves a given open generic type as transient.</summary>
        private sealed class TransientOpenGenericResolver : OpenGenericResolver
        {
            internal override void Register(Type closedGenericImplementation, UnregisteredTypeEventArgs e)
            {
                try
                {
                    IInstanceProducer producer =
                        this.Container.GetRegistration(closedGenericImplementation, throwOnFailure: true);

                    e.Register(producer.BuildExpression());
                }
                catch (Exception ex)
                {
                    try
                    {
                        this.Container.GetInstance(closedGenericImplementation);
                    }
                    catch (Exception ex2)
                    {
                        // The exception thrown from GetInstance (if any) will be much more descriptive,
                        // than the one thrown from GetRegistration (this is by design).
                        ex = ex2;
                    }

                    throw new ActivationException(StringResources.ErrorInRegisterOpenGenericRegistration(
                        this.OpenGenericServiceType, closedGenericImplementation, ex.Message), ex);
                }
            }
        }

        /// <summary>Resolves a given open generic type as singleton.</summary>
        private sealed class SingleOpenGenericResolver : OpenGenericResolver
        {
            private readonly Dictionary<Type, object> singletons = new Dictionary<Type, object>();

            public static Func<T> BuildFunc<T>(object instance)
            {
                var constant = Expression.Constant(instance, typeof(T));

                return Expression.Lambda<Func<T>>(constant, new ParameterExpression[0]).Compile();
            }

            internal override void Register(Type closedGenericImplementation, UnregisteredTypeEventArgs e)
            {
                object singleton = this.GetSingleton(closedGenericImplementation);

                Expression expression = Expression.Constant(singleton, e.UnregisteredServiceType);

                e.Register(expression);
            }

            private object GetSingleton(Type closedGenericImplementation)
            {
                object singleton;

                lock (this)
                {
                    if (!this.singletons.TryGetValue(closedGenericImplementation, out singleton))
                    {
                        try
                        {
                            singleton = this.Container.GetInstance(closedGenericImplementation);
                        }
                        catch (Exception ex)
                        {
                            throw new ActivationException(StringResources.ErrorInRegisterOpenGenericRegistration(
                                this.OpenGenericServiceType, closedGenericImplementation, ex.Message), ex);
                        }

                        this.singletons[closedGenericImplementation] = singleton;
                    }
                }

                return singleton;
            }

            private static Delegate BuildDelegate(object instance, Type serviceType)
            {
                return typeof(SingleOpenGenericResolver).GetMethod("BuildFunc")
                    .MakeGenericMethod(serviceType)
                    .Invoke(null, new[] { instance }) as Delegate;
            }
        }

        /// <summary>Resolves a given open generic type.</summary>
        private abstract class OpenGenericResolver
        {
            internal Type OpenGenericServiceType { get; set; }

            internal Type OpenGenericImplementation { get; set; }

            internal Container Container { get; set; }

            internal void ResolveOpenGeneric(object sender, UnregisteredTypeEventArgs e)
            {
                if (!this.OpenGenericServiceType.IsGenericTypeDefinitionOf(e.UnregisteredServiceType))
                {
                    return;
                }

                var builder = new GenericTypeBuilder(e.UnregisteredServiceType, this.OpenGenericImplementation);

                var results = builder.BuildClosedGenericImplementation();

                if (results.ClosedServiceTypeSatisfiesAllTypeConstraints)
                {
                    this.Register(results.ClosedGenericImplementation, e);
                }
            }

            internal abstract void Register(Type closedGenericImplementation, UnregisteredTypeEventArgs e);
        }
    }
}