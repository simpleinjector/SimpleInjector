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
    using System.Diagnostics;

    [DebuggerDisplay(nameof(DefaultDependencyInjectionBehavior))]
    internal sealed class DefaultDependencyInjectionBehavior : IDependencyInjectionBehavior
    {
        private readonly Container container;

        internal DefaultDependencyInjectionBehavior(Container container)
        {
            this.container = container;
        }

        public void Verify(InjectionConsumerInfo consumer)
        {
            Requires.IsNotNull(consumer, nameof(consumer));

            InjectionTargetInfo target = consumer.Target;

            if (target.TargetType.IsValueType() || target.TargetType == typeof(string))
            {
                throw new ActivationException(StringResources.TypeMustNotContainInvalidInjectionTarget(target));
            }
        }

        public InstanceProducer GetInstanceProducer(InjectionConsumerInfo consumer, bool throwOnFailure)
        {
            Requires.IsNotNull(consumer, nameof(consumer));

            InjectionTargetInfo target = consumer.Target;

            InstanceProducer producer = this.container.GetRegistrationEvenIfInvalid(target.TargetType, consumer);
            
            if (producer == null && throwOnFailure)
            {
                // By redirecting to Verify() we let the verify throw an expressive exception. If it doesn't
                // we throw the exception ourselves.
                this.container.Options.DependencyInjectionBehavior.Verify(consumer);

                this.container.ThrowParameterTypeMustBeRegistered(target);
            }

            return producer;
        }
    }
}