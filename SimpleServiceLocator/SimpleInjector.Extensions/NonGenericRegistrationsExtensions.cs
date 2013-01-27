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
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;

    using SimpleInjector.Extensions.Decorators;

    /// <summary>
    /// Extension methods with non-generic method overloads.
    /// </summary>
    public static class NonGenericRegistrationsExtensions
    {
        /// <summary>
        /// Registers that the same instance of type <paramref name="implementation"/> will be returned every 
        /// time a <paramref name="serviceType"/> type is requested. If <paramref name="serviceType"/> and
        /// <paramref name="implementation"/> represent the same type, the type is registered by itself.
        /// </summary>
        /// <param name="container">The container to make the registrations in.</param>
        /// <param name="serviceType">The base type or interface to register.</param>
        /// <param name="implementation">The actual type that will be returned when requested.</param>
        /// <exception cref="ArgumentNullException">Thrown when either <paramref name="container"/>,
        /// <paramref name="serviceType"/> or <paramref name="implementation"/> are null references (Nothing in
        /// VB).</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="implementation"/> is
        /// no sub type from <paramref name="serviceType"/>, or when one of them represents an open generic
        /// type.</exception>
        public static void RegisterSingle(this Container container, Type serviceType, Type implementation)
        {
            Requires.IsNotNull(container, "container");

            container.Register(serviceType, implementation, Lifestyle.Singleton);
        }

        /// <summary>
        /// Registers the specified delegate that allows constructing a single <paramref name="serviceType"/> 
        /// instance. The container will call this delegate at most once during the lifetime of the application.
        /// </summary>
        /// <param name="container">The container to make the registrations in.</param>
        /// <param name="serviceType">The base type or interface to register.</param>
        /// <param name="instanceCreator">The delegate that will be used for creating that single instance.</param>
        /// <exception cref="ArgumentException">Thrown when <paramref name="serviceType"/> represents an open
        /// generic type.</exception>
        /// <exception cref="ArgumentNullException">Thrown when either <paramref name="container"/>,
        /// <paramref name="serviceType"/> or <paramref name="instanceCreator"/> are null references (Nothing in
        /// VB).</exception>
        public static void RegisterSingle(this Container container, Type serviceType,
            Func<object> instanceCreator)
        {
            Requires.IsNotNull(container, "container");

            container.Register(serviceType, instanceCreator, Lifestyle.Singleton);
        }

        /// <summary>
        /// Registers a single instance. This <paramref name="instance"/> must be thread-safe.
        /// </summary>
        /// <param name="container">The container to make the registrations in.</param>
        /// <param name="serviceType">The base type or interface to register.</param>
        /// <param name="instance">The instance to register.</param>
        /// <exception cref="ArgumentNullException">Thrown when either <paramref name="container"/>,
        /// <paramref name="serviceType"/> or <paramref name="instance"/> are null references (Nothing in
        /// VB).</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="instance"/> is
        /// no sub type from <paramref name="serviceType"/>.</exception>
        public static void RegisterSingle(this Container container, Type serviceType, object instance)
        {
            Requires.IsNotNull(container, "container");
            Requires.IsNotNull(serviceType, "serviceType");
            Requires.IsNotNull(instance, "instance");
            Requires.ServiceIsAssignableFromImplementation(serviceType, instance.GetType(), "serviceType");

            container.Register(serviceType, () => instance, Lifestyle.Singleton);
        }

        /// <summary>
        /// Registers that a new instance of <paramref name="concreteType"/> will be returned every time it 
        /// is requested (transient).
        /// </summary>
        /// <param name="container">The container to make the registrations in.</param>
        /// <param name="concreteType">The concrete type that will be registered.</param>
        /// <exception cref="ArgumentNullException">Thrown when either <paramref name="container"/> or 
        /// <paramref name="concreteType"/> are null references (Nothing in VB).</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="concreteType"/> represents an 
        /// open generic type or is a type that can not be created by the container.
        /// </exception>
        public static void Register(this Container container, Type concreteType)
        {
            Requires.IsNotNull(container, "container");
            Requires.IsNotNull(concreteType, "concreteType");

            container.Register(concreteType, concreteType, Lifestyle.Transient);
        }

        /// <summary>
        /// Registers that a new instance of <paramref name="implementation"/> will be returned every time a
        /// <paramref name="serviceType"/> is requested. If <paramref name="serviceType"/> and 
        /// <paramref name="implementation"/> represent the same type, the type is registered by itself.
        /// </summary>
        /// <param name="container">The container to make the registrations in.</param>
        /// <param name="serviceType">The base type or interface to register.</param>
        /// <param name="implementation">The actual type that will be returned when requested.</param>
        /// <exception cref="ArgumentNullException">Thrown when either <paramref name="container"/>,
        /// <paramref name="serviceType"/> or <paramref name="implementation"/> are null references (Nothing in
        /// VB).</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="implementation"/> is
        /// no sub type from <paramref name="serviceType"/>, or one of them represents an open generic type.
        /// </exception>
        public static void Register(this Container container, Type serviceType, Type implementation)
        {
            Requires.IsNotNull(container, "container");

            container.Register(serviceType, implementation, Lifestyle.Transient);
        }

        /// <summary>
        /// Registers the specified delegate that allows returning instances of <paramref name="serviceType"/>.
        /// </summary>
        /// <param name="container">The container to make the registrations in.</param>
        /// <param name="serviceType">The base type or interface to register.</param>
        /// <param name="instanceCreator">The delegate that will be used for creating new instances.</param>
        /// <exception cref="ArgumentNullException">Thrown when either <paramref name="container"/>,
        /// <paramref name="serviceType"/> or <paramref name="instanceCreator"/> are null references (Nothing in
        /// VB).</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="serviceType"/> represents an
        /// open generic type.</exception>
        public static void Register(this Container container, Type serviceType, Func<object> instanceCreator)
        {
            Requires.IsNotNull(container, "container");

            container.Register(serviceType, instanceCreator, Lifestyle.Transient);
        }       
    }
}