#region Copyright Simple Injector Contributors
/* The Simple Injector is an easy-to-use Inversion of Control library for .NET
 * 
 * Copyright (c) 2015-2016 Simple Injector Contributors
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

namespace SimpleInjector.Advanced
{
    using System;

    /// <summary>
    /// Defines the container's behavior for building an expression tree for an dependency to inject, based on
    /// the information of the consuming type the dependency is injected into.
    /// Set the <see cref="ContainerOptions.DependencyInjectionBehavior">ConstructorInjectionBehavior</see> 
    /// property of the container's <see cref="Container.Options"/> property to change the default behavior 
    /// of the container.
    /// </summary>
    public interface IDependencyInjectionBehavior
    {
        /// <summary>Verifies the specified <paramref name="consumer"/>.</summary>
        /// <param name="consumer">Contextual information about the consumer where the built dependency is
        /// injected into.</param>
        /// <exception cref="ActivationException">
        /// Thrown when the type of the <see cref="InjectionConsumerInfo.Target">target</see> supplied with 
        /// the supplied <paramref name="consumer"/> cannot be used for auto wiring.</exception>
        /// <exception cref="ArgumentNullException">Thrown when the supplied argument is a null reference.</exception>
        void Verify(InjectionConsumerInfo consumer);

        /// <summary>
        /// Gets the <see cref="InstanceProducer"/> for the 
        /// <see cref="InjectionConsumerInfo.Target">Target</see> of the supplied <paramref name="consumer"/>.
        /// </summary>
        /// <param name="consumer">Contextual information about the consumer where the built dependency is
        /// injected into.</param>
        /// <param name="throwOnFailure">The indication whether the method should return null or throw
        /// an exception when the type is not registered.</param>
        /// <returns>An <see cref="InstanceProducer"/> that describes the intend of creating that 
        /// <see cref="InjectionConsumerInfo.Target">Target</see>. This method never returns null.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the argument is a null reference.</exception>
        InstanceProducer GetInstanceProducer(InjectionConsumerInfo consumer, bool throwOnFailure);
    }
}