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

namespace SimpleInjector.Lifestyles
{
    using System;
    using System.Linq.Expressions;
    
    public abstract class LifestyleRegistration
    {
        protected LifestyleRegistration(Container container)
        {
            Requires.IsNotNull(container, "container");

            this.Container = container;
        }

        protected internal Container Container { get; private set; }

        public abstract Expression BuildExpression();

        internal Expression InterceptExpression(Type serviceType, Expression expression)
        {
            var e = new ExpressionBuildingEventArgs(serviceType, expression);

            this.Container.OnExpressionBuilding(e);

            return e.Expression;
        }

        protected Func<TService> BuildTransientDelegate<TService>(Func<TService> instanceCreator)
            where TService : class
        {
            Requires.IsNotNull(instanceCreator, "instanceCreator");

            Expression expression = this.BuildTransientExpression<TService>(instanceCreator);

            // NOTE: The returned delegate could still return null (caused by the ExpressionBuilding event),
            // but I don't feel like protecting us against such an obscure thing.
            return BuildDelegate<TService>(expression);
        }

        protected Func<TService> BuildTransientDelegate<TService, TImplementation>()
            where TImplementation : class, TService
            where TService : class
        {
            Expression expression = this.BuildTransientExpression<TService, TImplementation>();

            return BuildDelegate<TService>(expression);
        }

        protected Expression BuildTransientExpression<TService>(Func<TService> instanceCreator)
            where TService : class
        {
            Requires.IsNotNull(instanceCreator, "instanceCreator");

            // We have to decorate the given instanceCreator to add a null check and throw an expressive
            // exception when the instanceCreator returned null. By supplying the instanceCreator as argument
            // to this method, we ensure that the reference to this delegate keeps available in the built
            // expression tree. This allows the delegate to be searched for and replaced, if needed.
            // If we would wrap the instanceCreator in another Func<TService>, this reference would not be
            // visitable in the expression tree and this information would be lost.
            Func<Func<TService>, TService> safeInstanceCreator = SafeInstanceCreator<TService>;

            Expression expression = Expression.Invoke(
                expression: Expression.Constant(safeInstanceCreator), 
                arguments: Expression.Constant(instanceCreator));

            expression = this.InterceptAndWrapInitializer<TService, TService>(expression);

            return expression;
        }

        protected Expression BuildTransientExpression<TService, TImplementation>()
            where TImplementation : class, TService
            where TService : class
        {
            Expression expression = 
                Helpers.BuildNewExpression(this.Container, typeof(TService), typeof(TImplementation));

            expression = this.InterceptAndWrapInitializer<TService, TImplementation>(expression);

            return expression;
        }

        private Expression InterceptAndWrapInitializer<TService, TImplementation>(Expression expression)
            where TImplementation : class, TService
            where TService : class
        {
            expression = this.InterceptExpression(typeof(TService), expression);

            expression = this.WrapWithInitializer<TService, TImplementation>(expression);

            return expression;
        }

        private Expression WrapWithInitializer<TService, TImplementation>(Expression expression)
            where TImplementation : class, TService
            where TService : class
        {
            Action<TImplementation> instanceInitializer = this.Container.GetInitializer<TImplementation>();

            if (instanceInitializer != null)
            {
                // It's not possible to return a Expression that is as heavily optimized as the newExpression
                // simply is, because the instance initializer must be called as well.
                return BuildExpressionWithInstanceInitializer<TService, TImplementation>(expression, instanceInitializer);
            }

            return expression;
        }

        private static Expression BuildExpressionWithInstanceInitializer<TService, TImplementation>(
            Expression newExpression, Action<TImplementation> instanceInitializer)
            where TImplementation : class, TService
            where TService : class
        {
            Func<TImplementation, TImplementation> instanceCreatorWithInitializer = instance =>
            {
                instanceInitializer(instance);

                return instance;
            };

            try
            {
                return Expression.Invoke(Expression.Constant(instanceCreatorWithInitializer), newExpression);
            }
            catch (Exception ex)
            {
                throw new ActivationException(
                    StringResources.TheInitializersCouldNotBeApplied(typeof(TImplementation), ex), ex);
            }
        }

        private static Func<TService> BuildDelegate<TService>(Expression expression)
            where TService : class
        {
            try
            {
                var newInstanceMethod =
                    Expression.Lambda<Func<TService>>(expression, new ParameterExpression[0]);

                return newInstanceMethod.Compile();
            }
            catch (Exception ex)
            {
                string message = StringResources.ErrorWhileBuildingDelegateFromExpression(
                    typeof(TService), expression, ex);

                throw new ActivationException(message, ex);
            }
        }

        private static TService SafeInstanceCreator<TService>(Func<TService> instanceCreator)
        {
            var instance = instanceCreator();

            if (instance == null)
            {
                throw new ActivationException(StringResources.DelegateForTypeReturnedNull(typeof(TService)));
            }

            return instance;
        }
    }
}