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
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;

    using SimpleInjector.Diagnostics;

    public abstract class Registration
    {
        private readonly HashSet<KnownRelationship> dependencies = new HashSet<KnownRelationship>();

        private Dictionary<ParameterInfo, OverriddenParameter> overriddenParameters;

        protected Registration(Lifestyle lifestyle, Container container)
        {
            Requires.IsNotNull(lifestyle, "lifestyle");
            Requires.IsNotNull(container, "container");
            
            this.Lifestyle = lifestyle;
            this.Container = container;
        }
        
        public abstract Type ImplementationType { get; }

        public Lifestyle Lifestyle { get; private set; }

        protected internal Container Container { get; private set; }

        public abstract Expression BuildExpression();

        internal KnownRelationship[] GetRelationships()
        {
            lock (this.dependencies)
            {
                return this.dependencies.ToArray();
            }
        }

        internal void ReplaceRelationships(IEnumerable<KnownRelationship> dependencies)
        {
            lock (this.dependencies)
            {
                this.dependencies.Clear();

                foreach (var dependency in dependencies)
                {
                    this.dependencies.Add(dependency);
                }
            }
        }

        internal Expression InterceptInstanceCreation(Type serviceType, Expression instanceCreatorExpression)
        {
            var e = new ExpressionBuildingEventArgs(serviceType, instanceCreatorExpression);

            this.Container.OnExpressionBuilding(e);

            return e.Expression;
        }

        internal void AddRelationship(KnownRelationship relationship) 
        {
            Requires.IsNotNull(relationship, "relationship");

            lock (this.dependencies)
            {
                this.dependencies.Add(relationship);
            }
        }

        // This method should only be called by the Lifestyle base class.
        internal void SetParameterOverrides(IEnumerable<Tuple<ParameterInfo, Expression>> overriddenParameters)
        {
            this.overriddenParameters = overriddenParameters.ToDictionary(
                p => p.Item1,
                p => new OverriddenParameter(Expression.Constant(null, p.Item1.ParameterType), p.Item2));
        }

        protected Func<TService> BuildTransientDelegate<TService>(Func<TService> instanceCreator)
            where TService : class
        {
            Requires.IsNotNull(instanceCreator, "instanceCreator");

            Expression expression = this.BuildTransientExpression<TService>(instanceCreator);

            // NOTE: The returned delegate could still return null (caused by the ExpressionBuilding event),
            // but I don't feel like protecting us against such an obscure user bug.
            return BuildDelegate<TService>(expression);
        }

        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter",
            Justification = "Supplying the generic type arguments is needed, since internal types can not " +
                            "be created using the non-generic overloads in a sandbox.")]
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

            Expression expression = Expression.Invoke(Expression.Constant(instanceCreator));

            expression = this.InterceptInstanceCreation(typeof(TService), expression);
            
            // We have to decorate the given instanceCreator to add a null check and throw an expressive
            // exception when the instanceCreator returned null. By preventing to polute the expression given
            // to the ExpressionBuilding event (triggered by the previous call), we wrap the null checker
            // after the ExpressionBuilding returned. But we need to wrap it before any initializer is ran,
            // since that could lead to null reference exceptions in user code.
            expression = this.WrapWithNullChecker<TService>(expression);

            expression = this.WrapWithInitializer<TService, TService>(expression);

            return expression;
        }

        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter",
            Justification = "Supplying the generic type arguments is needed, since internal types can not " +
                            "be created using the non-generic overloads in a sandbox.")]
        protected Expression BuildTransientExpression<TService, TImplementation>()
            where TImplementation : class, TService
            where TService : class
        {
            Expression expression = this.BuildNewExpression(typeof(TService), typeof(TImplementation));

            expression = this.InterceptInstanceCreation(typeof(TService), expression);

            expression = this.WrapWithInitializer<TService, TImplementation>(expression);

            return this.ReplacePlaceHolderExpressionWithOverriddenParameterExpressions(expression);
        }

        private NewExpression BuildNewExpression(Type serviceType, Type implemenationType)
        {
            var resolutionBehavior = this.Container.Options.ConstructorResolutionBehavior;

            ConstructorInfo constructor = resolutionBehavior.GetConstructor(serviceType, implemenationType);

            this.AddConstructorParametersAsKnownRelationship(constructor);

            var parameters =
                from parameter in constructor.GetParameters()
                let placeHolder = this.GetOverriddenParameter(parameter).PlaceHolder
                select placeHolder ?? this.BuildParameterExpression(parameter);

            return Expression.New(constructor, parameters.ToArray());
        }

        private Expression ReplacePlaceHolderExpressionWithOverriddenParameterExpressions(Expression expression)
        {
            if (this.overriddenParameters != null)
            {
                foreach (var value in this.overriddenParameters.Values)
                {
                    expression = SubExpressionReplacer.Replace(expression, value.PlaceHolder, value.Expression);
                }
            }

            return expression;
        }

        private OverriddenParameter GetOverriddenParameter(ParameterInfo parameter)
        {
            if (this.overriddenParameters != null && this.overriddenParameters.ContainsKey(parameter))
            {
                return this.overriddenParameters[parameter];
            }

            return new OverriddenParameter();
        }

        private void AddConstructorParametersAsKnownRelationship(ConstructorInfo constructor)
        {
            var relationships =
                from parameter in constructor.GetParameters()
                let producer = this.Container.GetRegistrationEvenIfInvalid(parameter.ParameterType)
                where producer != null
                select new KnownRelationship(
                    parameter.Member.DeclaringType, this.Lifestyle, producer);

            foreach (var relationship in relationships)
            {
                this.AddRelationship(relationship);
            }
        }

        private Expression BuildParameterExpression(ParameterInfo parameter)
        {
            var injectionBehavior = this.Container.Options.ConstructorInjectionBehavior;

            var expression = injectionBehavior.BuildParameterExpression(parameter);

            if (expression == null)
            {
                throw new ActivationException(
                    StringResources.ConstructorInjectionBehaviorReturnedNull(injectionBehavior, parameter));
            }

            return expression;
        }

        private Expression WrapWithNullChecker<TService>(Expression expression)
        {
            Func<TService, TService> nullChecker = ThrowWhenNull<TService>;

            return Expression.Invoke(Expression.Constant(nullChecker), expression);
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

        private static TService ThrowWhenNull<TService>(TService instance)
        {
            if (instance == null)
            {
                throw new ActivationException(StringResources.DelegateForTypeReturnedNull(typeof(TService)));
            }

            return instance;
        }
        
        private struct OverriddenParameter
        {
            private readonly ConstantExpression placeHolder;
            private readonly Expression expression;

            public OverriddenParameter(ConstantExpression placeHolder, Expression expression)
            {
                this.placeHolder = placeHolder;
                this.expression = expression;
            }

            // A placeholder is a fake expression that we inject into the NewExpression. After the 
            // NewExpression is created, it is ran through the ExpressionBuilding interception. By using
            // placeholders instead of the real overridden expressions we prevent those expressions from
            // being processed twice by the ExpressionBuilding event (since we expect the supplied expressions
            // to already be processed). After the event has ran we replace the placeholders with the real
            // expressions again (using an ExpressionVisitor).
            internal ConstantExpression PlaceHolder 
            { 
                get { return this.placeHolder; } 
            }

            internal Expression Expression 
            {
                get { return this.expression; }
            }
        }

        // Searches an expression for a specific sub expression and replaces that sub expression with a
        // different supplied expression.
        private sealed class SubExpressionReplacer : ExpressionVisitor
        {
            private readonly ConstantExpression subExpressionToFind;
            private readonly Expression replacementExpression;

            private SubExpressionReplacer(ConstantExpression subExpressionToFind,
                Expression replacementExpression)
            {
                this.subExpressionToFind = subExpressionToFind;
                this.replacementExpression = replacementExpression;
            }

            public override Expression Visit(Expression node)
            {
                return base.Visit(node);
            }

            internal static Expression Replace(Expression expressionToAlter,
                ConstantExpression subExpressionToFind, Expression replacementExpression)
            {
                var visitor = new SubExpressionReplacer(subExpressionToFind, replacementExpression);

                return visitor.Visit(expressionToAlter);
            }

            protected override Expression VisitConstant(ConstantExpression node)
            {
                return node == this.subExpressionToFind ? this.replacementExpression : base.VisitConstant(node);
            }
        }
    }
}