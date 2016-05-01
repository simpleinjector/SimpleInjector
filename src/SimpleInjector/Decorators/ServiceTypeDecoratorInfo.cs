#region Copyright Simple Injector Contributors
/* The Simple Injector is an easy-to-use Inversion of Control library for .NET
 * 
 * Copyright (c) 2013 Simple Injector Contributors
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
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using SimpleInjector.Advanced;
    using SimpleInjector.Lifestyles;

    // A list of all decorators applied to a given service type.
    internal sealed class ServiceTypeDecoratorInfo
    {
        private readonly List<DecoratorInfo> appliedDecorators = new List<DecoratorInfo>();

        internal ServiceTypeDecoratorInfo(Type implementationType, InstanceProducer originalProducer)
        {
            this.ImplementationType = implementationType;
            this.OriginalProducer = originalProducer;
        }

        internal Type ImplementationType { get; }

        internal InstanceProducer OriginalProducer { get; }

        internal IEnumerable<DecoratorInfo> AppliedDecorators => this.appliedDecorators;

        internal InstanceProducer GetCurrentInstanceProducer() => 
            this.AppliedDecorators.Any() ? this.AppliedDecorators.Last().DecoratorProducer : this.OriginalProducer;

        internal void AddAppliedDecorator(Type serviceType, Type decoratorType, Container container, 
            Lifestyle lifestyle, Expression decoratedExpression, 
            IEnumerable<KnownRelationship> decoratorRelationships = null)
        {
            var registration = new ExpressionRegistration(decoratedExpression, decoratorType,
                lifestyle, container);

            registration.ReplaceRelationships(decoratorRelationships ?? Enumerable.Empty<KnownRelationship>());

            var producer = new InstanceProducer(serviceType, registration);

            producer.IsDecorated = true;

            this.appliedDecorators.Add(new DecoratorInfo(decoratorType, producer));
        }
    }
}