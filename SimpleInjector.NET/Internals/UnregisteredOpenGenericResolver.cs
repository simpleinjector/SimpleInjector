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

namespace SimpleInjector.Internals
{
    using System;
    using System.Collections.Generic;
    using SimpleInjector.Extensions;

    /// <summary>Resolves a given open generic type.</summary>
    internal sealed class UnregisteredOpenGenericResolver
    {
        private readonly Dictionary<Type, Registration> lifestyleRegistrationCache =
            new Dictionary<Type, Registration>();

        internal Type OpenGenericServiceType { get; set; }

        internal Type OpenGenericImplementation { get; set; }

        internal Container Container { get; set; }

        internal Lifestyle Lifestyle { get; set; }

        internal Predicate<OpenGenericPredicateContext> Predicate { get; set; }

        internal void ResolveUnregisteredType(object sender, UnregisteredTypeEventArgs e)
        {
            if (!this.OpenGenericServiceType.IsGenericTypeDefinitionOf(e.UnregisteredServiceType))
            {
                return;
            }

            var builder = new GenericTypeBuilder(e.UnregisteredServiceType, this.OpenGenericImplementation);

            var result = builder.BuildClosedGenericImplementation();

            if (result.ClosedServiceTypeSatisfiesAllTypeConstraints &&
                this.ClosedServiceTypeSatisfiesPredicate(e.UnregisteredServiceType,
                    result.ClosedGenericImplementation, e.Handled))
            {
                this.RegisterType(e, result.ClosedGenericImplementation);
            }
        }

        private bool ClosedServiceTypeSatisfiesPredicate(Type service, Type implementation, bool handled)
        {
            var context = new OpenGenericPredicateContext(service, implementation, handled);
            return this.Predicate(context);
        }

        private void RegisterType(UnregisteredTypeEventArgs e, Type closedGenericImplementation)
        {
            var registration =
                this.GetRegistrationFromCache(e.UnregisteredServiceType, closedGenericImplementation);

            this.ThrowWhenExpressionCanNotBeBuilt(registration, closedGenericImplementation);

            e.Register(registration);
        }

        private void ThrowWhenExpressionCanNotBeBuilt(Registration registration, Type implementationType)
        {
            try
            {
                // The core library will also throw a quite expressive exception if we don't do it here,
                // but we can do better and explain that the type is registered as open generic type
                // (instead of it being registered using open generic batch registration).
                registration.BuildExpression();
            }
            catch (Exception ex)
            {
                throw new ActivationException(StringResources.ErrorInRegisterOpenGenericRegistration(
                    this.OpenGenericServiceType, implementationType, ex.Message), ex);
            }
        }

        private Registration GetRegistrationFromCache(Type serviceType, Type implementationType)
        {
            // We must cache the returned lifestyles to prevent any multi-threading issues in case the
            // returned lifestyle does some caching internally (as the singleton lifestyle does).
            lock (this.lifestyleRegistrationCache)
            {
                Registration registration;

                if (!this.lifestyleRegistrationCache.TryGetValue(serviceType, out registration))
                {
                    registration = this.GetRegistration(serviceType, implementationType);

                    this.lifestyleRegistrationCache[serviceType] = registration;
                }

                return registration;
            }
        }

        private Registration GetRegistration(Type serviceType, Type implementationType)
        {
            try
            {
                return this.Lifestyle.CreateRegistration(serviceType, implementationType, this.Container);
            }
            catch (ArgumentException ex)
            {
                throw new ActivationException(ex.Message);
            }
        }
    }
}
