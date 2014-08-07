#region Copyright Simple Injector Contributors
/* The Simple Injector is an easy-to-use Inversion of Control library for .NET
 * 
 * Copyright (c) 2013-2014 Simple Injector Contributors
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

namespace SimpleInjector
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Runtime.CompilerServices;
    using SimpleInjector.Advanced;
    using SimpleInjector.Advanced.Internal;
    using SimpleInjector.Lifestyles;

    internal static partial class CompilationHelpers
    {
        // Compile the expression. If the expression is compiled in a dynamic assembly, the compiled delegate
        // is called (to ensure that it will run, because it tends to fail now and then) and the created
        // instance is returned through the out parameter. Note that NO created instance will be returned when
        // the expression is compiled using Expression.Compile)(.
        internal static Func<TResult> CompileExpression<TResult>(Container container, Expression expression)
        {
            // Skip compiling if all we need to do is return a singleton.
            if (expression is ConstantExpression)
            {
                return CreateConstantValueDelegate<TResult>(expression);
            }

            Func<TResult> compiledLambda = null;

            expression = OptimizeObjectGraph(container, expression);

            // In the common case, the developer will/should only create a single container during the 
            // lifetime of the application (this is the recommended approach). In this case, we can optimize
            // the performance by compiling delegates in an dynamic assembly. We can't do this when the developer
            // creates many containers, because this will create a memory leak (dynamic assemblies are never 
            // unloaded).
            if (container.Options.EnableDynamicAssemblyCompilation)
            {
                TryCompileInDynamicAssembly<TResult>(expression, ref compiledLambda);
            }

            return compiledLambda ?? CompileLambda<TResult>(expression);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static Expression OptimizeObjectGraph(Container container, Expression expression)
        {
            var lifestyleInfos = PerObjectGraphOptimizableRegistrationFinder.Find(expression, container);

            if (lifestyleInfos.Any())
            {
                return OptimizeExpression(container, expression, lifestyleInfos);
            }

            return expression;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static Func<TResult> CompileLambda<TResult>(Expression expression)
        {
            return Expression.Lambda<Func<TResult>>(expression).Compile();
        }

        static partial void TryCompileInDynamicAssembly<TResult>(Expression expression, 
            ref Func<TResult> compiledLambda);

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static Func<TResult> CreateConstantValueDelegate<TResult>(Expression expression)
        {
            object value = ((ConstantExpression)expression).Value;

            TResult singleton = (TResult)value;

            // This lambda will be a tiny little bit faster than a compiled delegate.
            return () => singleton;
        }

        // OptimizeExpression will implement caching of the scopes of ScopedLifestyles which will optimize
        // performance in case multiple scoped registrations are used within a single delegate. Here's an
        // example of how the expression gets optimized:
        // Before:
        // Func<HomeController> factory = () =>
        // {
        //     return new HomeController(
        //         reg1.GetInstance(), // Hits ThreadLocal, hits dictionary
        //         new SomeQueryHandler(reg1.GetInstance()), // Hits ThreadLocal, hits dictionary
        //         new SomeCommandHandler(reg2.GetInstance())); // Hits ThreadLocal, hits dictionary
        // };
        // After: 
        // Func<HomeController> factory = () =>
        // {
        //     var scope1 = new LazyScope(scopeFactory1, container);
        //     var value1 = new LazyScopedRegistration<IRepository, RepositoryImpl>(reg1);
        //     var value2 = new LazyScopedRegistration<IService, ServiceImpl>(reg2);
        //
        //     return new HomeController(
        //         value1.GetInstance(scope1.Value), // Hits ThreadLocal, hits dictionary
        //         new SomeQueryHandler(value1.GetInstance(scope1.Value)),
        //         new SomeCommandHandler(value2.GetInstance(scope1.Value))); // Hits dictionary
        // };
        private static Expression OptimizeExpression(Container container, Expression expression,
            OptimizableLifestyleInfo[] lifestyleInfos)
        {
            var lifestyleAssigmentExpressions = (
                from lifestyleInfo in lifestyleInfos
                let scopeFactory = lifestyleInfo.Lifestyle.CreateCurrentScopeProvider(container)
                let newExpression = CreateNewLazyScopeExpression(scopeFactory, container)
                select Expression.Assign(lifestyleInfo.Variable, newExpression))
                .ToArray();

            var registrationInfos = (
                from lifestyleInfo in lifestyleInfos
                from registrationInfo in lifestyleInfo.Registrations
                select registrationInfo)
                .ToArray();
            
            var registrationAssignmentExpressions = (
                from registrationInfo in registrationInfos
                let newExpression = CreateNewLazyScopedRegistration(registrationInfo.Registration)
                select Expression.Assign(registrationInfo.Variable, newExpression))
                .ToArray();

            var optimizedExpression = ObjectGraphOptimizerExpressionVisitor.Optimize(expression, registrationInfos);

            return Expression.Block(
                variables: lifestyleInfos.Select(l => l.Variable)
                    .Concat(registrationInfos.Select(r => r.Variable)),
                expressions: lifestyleAssigmentExpressions
                    .Concat(registrationAssignmentExpressions
                        .Concat(new[] { optimizedExpression })));
        }

        private static NewExpression CreateNewLazyScopeExpression(Func<Scope> scopeFactory, Container container)
        {
            return Expression.New(
                typeof(LazyScope).GetConstructor(new[] { typeof(Func<Scope>), typeof(Container) }),
                Expression.Constant(scopeFactory, typeof(Func<Scope>)),
                Expression.Constant(container, typeof(Container)));
        }

        private static NewExpression CreateNewLazyScopedRegistration(Registration registration)
        {
            var type = typeof(LazyScopedRegistration<,>)
                .MakeGenericType(registration.GetType().GetGenericArguments());

            return Expression.New(
                type.GetConstructor(new[] { typeof(Registration) }),
                Expression.Constant(registration, typeof(Registration)));
        }

        private sealed class PerObjectGraphOptimizableRegistrationFinder : ExpressionVisitor
        {
            private readonly List<OptimizableRegistrationInfo> perObjectGraphRegistrations =
                new List<OptimizableRegistrationInfo>();

            private Container container;

            public static OptimizableLifestyleInfo[] Find(Expression expression, Container container)
            {
                var finder = new PerObjectGraphOptimizableRegistrationFinder();

                finder.container = container;

                finder.Visit(expression);

                var lifestyleGroups = finder.perObjectGraphRegistrations
                    .GroupBy(r => r.Registration.Lifestyle, ReferenceEqualityComparer<Lifestyle>.Instance);

                return (
                    from lifestyleGroup in lifestyleGroups
                    select new OptimizableLifestyleInfo(
                        (ScopedLifestyle)lifestyleGroup.Key,
                        lifestyleGroup.GroupBy(info => info.Registration,
                            ReferenceEqualityComparer<Registration>.Instance)
                            .Select(g => g.First())))
                    .ToArray();
            }

            protected override Expression VisitMethodCall(MethodCallExpression node)
            {
                Registration registration = this.GetScopedRegistration(node);

                if (registration != null)
                {
                    this.perObjectGraphRegistrations.Add(new OptimizableRegistrationInfo(registration, node));
                }

                return base.VisitMethodCall(node);
            }

            private Registration GetScopedRegistration(MethodCallExpression node)
            {
                var instance = node.Object as ConstantExpression;

                var registration = instance != null ? instance.Value as Registration : null;

                if (registration != null && object.ReferenceEquals(registration.Container, this.container))
                {
                    Type type = registration.GetType();

                    if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(ScopedRegistration<,>))
                    {
                        return registration;
                    }
                }

                return null;
            }
        }

        private sealed class ObjectGraphOptimizerExpressionVisitor : ExpressionVisitor
        {
            private OptimizableRegistrationInfo[] registrationsToOptimize;

            public static Expression Optimize(Expression expression,
                OptimizableRegistrationInfo[] registrationsToOptimize)
            {
                var optimizer = new ObjectGraphOptimizerExpressionVisitor();

                optimizer.registrationsToOptimize = registrationsToOptimize;

                return optimizer.Visit(expression);
            }

            protected override Expression VisitMethodCall(MethodCallExpression node)
            {
                var registration = this.registrationsToOptimize.FirstOrDefault(r => r.OriginalExpression == node);

                if (registration != null)
                {
                    return registration.LazyScopeRegistrationGetInstanceExpression;
                }

                return base.VisitMethodCall(node);
            }
        }

        private sealed class OptimizableLifestyleInfo
        {
            internal readonly ScopedLifestyle Lifestyle;
            internal readonly ParameterExpression Variable;
            internal readonly IEnumerable<OptimizableRegistrationInfo> Registrations;

            public OptimizableLifestyleInfo(ScopedLifestyle lifestyle,
                IEnumerable<OptimizableRegistrationInfo> registrations)
            {
                this.Lifestyle = lifestyle;
                this.Registrations = registrations.ToArray();
                this.Variable = Expression.Variable(typeof(LazyScope));
                
                this.InitializeRegistrations();
            }

            private void InitializeRegistrations()
            {
                foreach (var registration in this.Registrations)
                {
                    registration.LifestyleInfo = this;
                }
            }
        }

        private sealed class OptimizableRegistrationInfo
        {
            internal readonly Registration Registration;
            internal readonly MethodCallExpression OriginalExpression;

            private readonly Type lazyScopeRegistrationType;

            private ParameterExpression variable;

            internal OptimizableRegistrationInfo(Registration registration, 
                MethodCallExpression originalExpression)
            {
                this.Registration = registration;
                this.OriginalExpression = originalExpression;

                this.lazyScopeRegistrationType = typeof(LazyScopedRegistration<,>)
                    .MakeGenericType(this.Registration.GetType().GetGenericArguments());
            }

            internal ParameterExpression Variable
            {
                get
                {
                    if (this.variable == null)
                    {
                        this.variable = Expression.Variable(this.lazyScopeRegistrationType);
                    }

                    return this.variable;
                }
            }

            internal OptimizableLifestyleInfo LifestyleInfo { get; set; }

            internal Expression LazyScopeRegistrationGetInstanceExpression
            {
                get
                {
                    return Expression.Call(
                        this.Variable, 
                        this.lazyScopeRegistrationType.GetMethod("GetInstance"),
                        Expression.Property(this.LifestyleInfo.Variable, "Value"));
                }
            }
        }
    }
}