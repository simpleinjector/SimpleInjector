#region Copyright (c) 2013 S. van Deursen
/* The Simple Injector is an easy-to-use Inversion of Control library for .NET
 * 
 * Copyright (C) 2013 S. van Deursen
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

namespace SimpleInjector.Extensions.Decorators
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    internal sealed class ServiceDecoratorExpressionInterceptor : DecoratorExpressionInterceptor
    {
        private static readonly object ContainerItemsKeyAndLock = new object();

        private readonly Dictionary<InstanceProducer, Registration> registrations;
        private readonly ExpressionBuiltEventArgs e;
        private readonly Type registeredServiceType;
        private readonly ConstructorInfo decoratorConstructor;
        private readonly Type decoratorType;

        public ServiceDecoratorExpressionInterceptor(DecoratorExpressionInterceptorData data,
            Dictionary<InstanceProducer, Registration> registrations, ExpressionBuiltEventArgs e, 
            Type decoratorType)
            : base(data)
        {
            this.registrations = registrations;
            this.e = e;
            this.registeredServiceType = e.RegisteredServiceType;

            this.decoratorConstructor = data.Container.Options.ConstructorResolutionBehavior
                .GetConstructor(e.RegisteredServiceType, decoratorType);

            // The actual decorator could be different. TODO: must... write... test... for... this.
            this.decoratorType = this.decoratorConstructor.DeclaringType;
        }

        protected override Dictionary<InstanceProducer, ServiceTypeDecoratorInfo> ThreadStaticServiceTypePredicateCache
        {
            get { return this.GetThreadStaticServiceTypePredicateCacheByKey(ContainerItemsKeyAndLock); }
        }

        internal bool SatisfiesPredicate()
        {
            var context = this.CreatePredicateContext(this.e);

            return this.SatisfiesPredicate(context);
        }

        internal void ApplyDecorator()
        {
            // By creating the decorator using a Lifestyle Registration the decorator can be completely
            // incorperated into the pipeline. This means that the ExpressionBuilding can be applied and it
            // can be wrapped with an initializer.
            var registration = this.CreateRegistration();

            this.ReplaceExpression(registration);

            this.AddKnownDecoratorRelationships(registration);

            this.AddAppliedDecoratorToDecoratorPredicateContext();
        }

        private void ReplaceExpression(Registration registration)
        {
            this.e.Expression = registration.BuildExpression();

            this.e.Lifestyle = this.Lifestyle;
        }

        private Registration CreateRegistration()
        {
            Registration registration;

            // Ensure that the registration for the decorator is created only once to prevent the possibility
            // of multiple instances being created when dealing lifestyles that cache an instance within the
            // Registration instance itself (such as the Singleton lifestyle does).
            lock (this.registrations)
            {
                if (!this.registrations.TryGetValue(this.e.InstanceProducer, out registration))
                {
                    registration = this.CreateRegistration(this.registeredServiceType, 
                        this.decoratorConstructor, this.e.Expression);

                    this.registrations[this.e.InstanceProducer] = registration;
                }
            }

            return registration;
        }
        
        private void AddKnownDecoratorRelationships(Registration decoratorRegistration)
        {
            var info = this.GetServiceTypeInfo(this.e);

            InstanceProducer decoratee = info.GetCurrentInstanceProducer();

            // Must be called before the current decorator is added to the list of applied decorators
            var relationships = this.GetKnownDecoratorRelationships(decoratorRegistration,
                this.decoratorConstructor, this.registeredServiceType, decoratee);

            this.e.KnownRelationships.AddRange(relationships);
        }

        private void AddAppliedDecoratorToDecoratorPredicateContext()
        {
            var info = this.GetServiceTypeInfo(this.e);

            // Add the decorator to the list of applied decorators. This way users can use this information in 
            // the predicate of the next decorator they add.
            info.AddAppliedDecorator(this.decoratorType, info.ImplementationType, this.Container, 
                this.Lifestyle, this.e.Expression);
        }
    }
}