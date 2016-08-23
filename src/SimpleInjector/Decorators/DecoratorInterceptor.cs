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
    using System.Collections;
    using System.Collections.Generic;
    using System.Reflection;
    using SimpleInjector.Internals;

    internal class DecoratorInterceptor
    {
        // Cache for decorators when the decorator is registered as singleton. Since all decoration requests
        // for the registration of that decorator will go through the same instance, we can (or must)
        // define this dictionary as instance field (not as static or thread-static). When a decorator is
        // registered.
        // So the Type is an closed generic version of the open generic service that is wrapped, the
        // registration is the registration for the closed generic decorator.
        private readonly Dictionary<InstanceProducer, Registration> registrationsCache;
        private readonly Dictionary<InstanceProducer, IEnumerable> singletonDecoratedCollectionsCache;

        private readonly DecoratorExpressionInterceptorData data;

        public DecoratorInterceptor(DecoratorExpressionInterceptorData data)
        {
            this.registrationsCache = new Dictionary<InstanceProducer, Registration>();
            this.singletonDecoratedCollectionsCache = new Dictionary<InstanceProducer, IEnumerable>();

            this.data = data;
        }

        // The service type definition (possibly open generic).
        protected Type ServiceTypeDefinition => this.data.ServiceType;

        // The decorator type definition (possibly open generic).
        protected Type DecoratorTypeDefinition => this.data.DecoratorType;

        internal void ExpressionBuilt(object sender, ExpressionBuiltEventArgs e)
        {
            this.TryToApplyDecorator(e);

            this.TryToApplyDecoratorOnContainerUncontrolledCollections(e);
        }

        private void TryToApplyDecorator(ExpressionBuiltEventArgs e)
        {
            Type closedDecoratorType;

            if (this.MustDecorate(e.RegisteredServiceType, out closedDecoratorType))
            {
                var decoratorInterceptor =
                    new ServiceDecoratorExpressionInterceptor(this.data, this.registrationsCache, e);

                if (decoratorInterceptor.SatisfiesPredicate())
                {
                    if (this.data.DecoratorTypeFactory != null)
                    {
                        closedDecoratorType = this.GetDecoratorTypeFromDecoratorFactory(
                            e.RegisteredServiceType, decoratorInterceptor.Context);
                    }
                    
                    if (closedDecoratorType != null)
                    {
                        decoratorInterceptor.ApplyDecorator(closedDecoratorType);
                    }
                }
            }
        }

        private void TryToApplyDecoratorOnContainerUncontrolledCollections(ExpressionBuiltEventArgs e)
        {
            if (!IsCollectionType(e.RegisteredServiceType) ||
                DecoratorHelpers.IsContainerControlledCollectionExpression(e.Expression))
            {
                // NOTE: Decorators on controlled collections will be applied by the normal mechanism.
                return;
            }

            var serviceType = e.RegisteredServiceType.GetGenericArguments()[0];

            Type decoratorType;

            if (this.MustDecorate(serviceType, out decoratorType))
            {
                this.ApplyDecoratorOnContainerUncontrolledCollection(e, decoratorType);
            }
        }

        private void ApplyDecoratorOnContainerUncontrolledCollection(ExpressionBuiltEventArgs e,
            Type decoratorType)
        {
            var serviceType = e.RegisteredServiceType.GetGenericArguments()[0];

            var uncontrolledInterceptor = new ContainerUncontrolledServicesDecoratorInterceptor(this.data,
                this.singletonDecoratedCollectionsCache, e, serviceType);

            if (uncontrolledInterceptor.SatisfiesPredicate())
            {
                if (this.data.DecoratorTypeFactory != null)
                {
                    decoratorType = this.GetDecoratorTypeFromDecoratorFactory(
                        serviceType, uncontrolledInterceptor.Context);
                }

                if (decoratorType != null)
                {
                    uncontrolledInterceptor.SetDecorator(decoratorType);
                    uncontrolledInterceptor.ApplyDecorator();
                }
            }
        }

        private static bool IsCollectionType(Type serviceType) => 
            typeof(IEnumerable<>).IsGenericTypeDefinitionOf(serviceType);

        private bool MustDecorate(Type serviceType, out Type decoratorType)
        {
            decoratorType = null;

            if (this.ServiceTypeDefinition == serviceType)
            {
                decoratorType = this.DecoratorTypeDefinition;

                return true;
            }

            if (!this.ServiceTypeDefinition.IsGenericTypeDefinitionOf(serviceType))
            {
                return false;
            }

            if (this.data.DecoratorTypeFactory != null)
            {
                // Since a decorator type factory delegate has been registered, we must assume at this point
                // that the decorator must be applied, because we can't call the factory at this point. The
                // predicate must always be called before calling the factory.
                return true;
            }

            var results = this.BuildClosedGenericImplementation(serviceType);

            if (!results.ClosedServiceTypeSatisfiesAllTypeConstraints)
            {
                return false;
            }

            decoratorType = results.ClosedGenericImplementation;

            return true;
        }

        private Type GetDecoratorTypeFromDecoratorFactory(Type requestedServiceType, 
            DecoratorPredicateContext context)
        {
            Type decoratorType = this.data.DecoratorTypeFactory(context);

            if (decoratorType.ContainsGenericParameters())
            {
                if (!requestedServiceType.IsGenericType())
                {
                    throw new ActivationException(
                        StringResources.TheDecoratorReturnedFromTheFactoryShouldNotBeOpenGeneric(
                            requestedServiceType, decoratorType));
                }

                Requires.TypeFactoryReturnedTypeThatDoesNotContainUnresolvableTypeArguments(
                    requestedServiceType, decoratorType);

                var builder = new GenericTypeBuilder(requestedServiceType, decoratorType);

                decoratorType = builder.BuildClosedGenericImplementation().ClosedGenericImplementation;

                // decoratorType == null when type constraints don't match.
                if (decoratorType != null)
                {
                    Requires.HasFactoryCreatedDecorator(this.data.Container, requestedServiceType, decoratorType);
                }
            }
            else
            {
                Requires.FactoryReturnsATypeThatIsAssignableFromServiceType(requestedServiceType, decoratorType);
            }

            return decoratorType;
        }

        private GenericTypeBuilder.BuildResult BuildClosedGenericImplementation(Type serviceType)
        {
            var builder = new GenericTypeBuilder(serviceType, this.DecoratorTypeDefinition);

            return builder.BuildClosedGenericImplementation();
        }
    }
}