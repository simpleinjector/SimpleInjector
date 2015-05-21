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

namespace SimpleInjector.Decorators
{
    using System;

    /// <summary>
    /// Extension methods for applying decorators.
    /// </summary>
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    [Obsolete(
        "The methods in this class have been replaced with an instances methods on the Container class. " + 
        " Please use one of the Container.RegisterDecorator overloads instead.",
        error: true)]
    public static class DecoratorExtensions
    {
        /// <summary>This method has been removed.</summary>
        /// <param name="container">The container to make the registrations in.</param>
        /// <param name="serviceType">The definition of the open generic service type that will
        /// be wrapped by the given <paramref name="decoratorType"/>.</param>
        /// <param name="decoratorType">The definition of the open generic decorator type that will
        /// be used to wrap the original service type.</param>
        /// <param name="lifestyle">The lifestyle that specifies how the returned decorator will be cached.</param>
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        [Obsolete(
            "This extension method has been replaced with an instance method on the Container. Please use " +
            "Container.RegisterDecorator(Type, Type, Lifestyle) instead.",
            error: true)]
        public static void RegisterDecorator(this Container container, Type serviceType, Type decoratorType,
            Lifestyle lifestyle)
        {
            container.RegisterDecorator(serviceType, decoratorType, lifestyle);
        }

        /// <summary>This method has been removed.</summary>
        /// <param name="container">The container to make the registrations in.</param>
        /// <param name="serviceType">The definition of the open generic service type that will
        /// be wrapped by the given <paramref name="decoratorType"/>.</param>
        /// <param name="decoratorType">The definition of the open generic decorator type that will
        /// be used to wrap the original service type.</param>
        /// <param name="lifestyle">The lifestyle that specifies how the returned decorator will be cached.</param>
        /// <param name="predicate">The predicate that determines whether the 
        /// <paramref name="decoratorType"/> must be applied to a service type.</param>
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        [Obsolete(
            "This extension method has been replaced with an instance method on the Container. Please use " +
            "Container.RegisterDecorator(Type, Type, Lifestyle, Predicate<DecoratorPredicateContext>) instead.",
            error: true)]
        public static void RegisterDecorator(this Container container, Type serviceType, Type decoratorType,
            Lifestyle lifestyle, Predicate<DecoratorPredicateContext> predicate)
        {
            container.RegisterDecorator(serviceType, decoratorType, lifestyle, predicate);
        }

        /// <summary>This method has been removed.</summary>
        /// <param name="container">The container to make the registrations in.</param>
        /// <param name="serviceType">The definition of the open generic service type that will
        /// be wrapped by the decorator type returned by the supplied <paramref name="decoratorTypeFactory"/>.</param>
        /// <param name="decoratorTypeFactory">A factory that allows building Type objects that define the
        /// decorators to inject, based on the given contextual information. The delegate is allowed to return
        /// open-generic types.</param>
        /// <param name="lifestyle">The lifestyle that specifies how the returned decorator will be cached.</param>
        /// <param name="predicate">The predicate that determines whether the decorator must be applied to a 
        /// service type.</param>
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        [Obsolete(
            "This extension method has been replaced with an instance method on the Container. Please use " +
            "Container.RegisterDecorator(Type, Func<DecoratorPredicateContext, Type>, Lifestyle, Predicate<DecoratorPredicateContext>) instead.",
            error: true)]
        public static void RegisterDecorator(this Container container, Type serviceType,
            Func<DecoratorPredicateContext, Type> decoratorTypeFactory, Lifestyle lifestyle,
            Predicate<DecoratorPredicateContext> predicate)
        {
            container.RegisterDecorator(serviceType, decoratorTypeFactory, lifestyle, predicate);
        }

        /// <summary>This method has been removed.</summary>
        /// <param name="container">The container to make the registrations in.</param>
        /// <param name="serviceType">The definition of the open generic service type that will
        /// be wrapped by the given <paramref name="decoratorType"/>.</param>
        /// <param name="decoratorType">The definition of the open generic decorator type that will
        /// be used to wrap the original service type.</param>
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        [Obsolete(
            "This extension method has been replaced with an instance method on the Container. Please use " +
            "Container.RegisterDecorator(Type, Type) instead.",
            error: true)]
        public static void RegisterDecorator(this Container container, Type serviceType, Type decoratorType)
        {
            container.RegisterDecorator(serviceType, decoratorType);
        }

        /// <summary>This method has been removed.</summary>
        /// <param name="container">The container to make the registrations in.</param>
        /// <param name="serviceType">The definition of the open generic service type that will
        /// be wrapped by the given <paramref name="decoratorType"/>.</param>
        /// <param name="decoratorType">The definition of the open generic decorator type that will
        /// be used to wrap the original service type.</param>
        /// <param name="predicate">The predicate that determines whether the 
        /// <paramref name="decoratorType"/> must be applied to a service type.</param>
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        [Obsolete(
            "This extension method has been replaced with an instance method on the Container. Please use " +
            "Container.RegisterDecorator(Type, Type, Predicate<DecoratorPredicateContext>) instead.",
            error: true)]
        public static void RegisterDecorator(this Container container, Type serviceType, Type decoratorType,
            Predicate<DecoratorPredicateContext> predicate)
        {
            container.RegisterDecorator(serviceType, decoratorType, predicate);
        }

        /// <summary>This method has been removed.</summary>
        /// <param name="container">The container to make the registrations in.</param>
        /// <param name="serviceType">The definition of the open generic service type that will
        /// be wrapped by the given <paramref name="decoratorType"/>.</param>
        /// <param name="decoratorType">The definition of the open generic decorator type that will
        /// be used to wrap the original service type.</param>
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        [Obsolete(
            "This method is not supported anymore. Please use " +
            "Container.RegisterDecorator(Type, Type, Lifestyle) instead.",
            error: true)]
        public static void RegisterSingleDecorator(this Container container, Type serviceType,
            Type decoratorType)
        {
            container.RegisterDecorator(serviceType, decoratorType, Lifestyle.Singleton);
        }

        /// <summary>This method has been removed.</summary>
        /// <param name="container">The container to make the registrations in.</param>
        /// <param name="serviceType">The definition of the open generic service type that will
        /// be wrapped by the given <paramref name="decoratorType"/>.</param>
        /// <param name="decoratorType">The definition of the open generic decorator type that will
        /// be used to wrap the original service type.</param>
        /// <param name="predicate">The predicate that determines whether the 
        /// <paramref name="decoratorType"/> must be applied to a service type.</param>
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        [Obsolete(
            "This method is not supported anymore. Please use " +
            "Container.RegisterDecorator(Type, Type, Lifestyle, Predicate<DecoratorPredicateContext>) instead.",
            error: true)]
        public static void RegisterSingleDecorator(this Container container, Type serviceType,
            Type decoratorType, Predicate<DecoratorPredicateContext> predicate)
        {
            container.RegisterDecorator(serviceType, decoratorType, Lifestyle.Singleton, predicate);
        }
    }
}