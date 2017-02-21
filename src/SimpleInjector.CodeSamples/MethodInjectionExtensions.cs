namespace SimpleInjector.CodeSamples
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using SimpleInjector.Advanced;

    public static class MethodInjectionExtensions
    {
        [DebuggerStepThrough]
        public static void EnableMethodInjectionWith<TAttribute>(this ContainerOptions options)
            where TAttribute : Attribute
        {
            // Dirty check to prevent this method from being called after other registrations have been made.
            options.PropertySelectionBehavior = options.PropertySelectionBehavior;

            var helper = new MethodInjectionHelper(options.Container, typeof(TAttribute));

            options.Container.ExpressionBuilding += helper.ExpressionBuilding;
        }

        private sealed class MethodInjectionHelper
        {
            private static readonly IEnumerable<Type> FuncTypes = (
                from type in typeof(Func<>).Assembly.GetExportedTypes()
                where type.IsGenericTypeDefinition
                where type.FullName.StartsWith("System.Func`")
                select type)
                .ToArray();

            private readonly Container container;
            private readonly Type attribute;

            public MethodInjectionHelper(Container container, Type attribute)
            {
                this.container = container;
                this.attribute = attribute;
            }

            internal void ExpressionBuilding(object sender, ExpressionBuildingEventArgs e)
            {
                var methods = this.GetMethodsToInject(e.KnownImplementationType);

                this.ApplyMethodInjectors(e, methods);
            }

            private MethodInfo[] GetMethodsToInject(Type type)
            {
                var all = 
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance;

                return (
                    from method in type.GetMethods(all)
                    where method.GetCustomAttributes(this.attribute, true).Any()
                    select method)
                    .ToArray();
            }

            private void ApplyMethodInjectors(ExpressionBuildingEventArgs e, MethodInfo[] methods)
            {
                foreach (var method in methods)
                {
                    this.ApplyMethodInjector(e, method);
                }
            }

            private void ApplyMethodInjector(ExpressionBuildingEventArgs e, MethodInfo method)
            {
                VerifyMethod(method);

                var dependencies = (
                    from parameter in method.GetParameters()
                    select this.container.GetRegistration(parameter.ParameterType, throwOnFailure: true))
                    .ToArray();

                ReplaceExpression(e, method, dependencies);

                AddKnownRelationships(e, dependencies);
            }

            private static void ReplaceExpression(ExpressionBuildingEventArgs e, MethodInfo method, 
                InstanceProducer[] dependencies)
            {
                Delegate injectorDelegate = BuildInjectorDelegate(e.KnownImplementationType, method);

                var dependencyExpressions =
                    from dependency in dependencies
                    select dependency.BuildExpression();

                var parameters = (new Expression[] { e.Expression }).Concat(dependencyExpressions);

                e.Expression = Expression.Invoke(Expression.Constant(injectorDelegate), parameters);
            }

            private static void AddKnownRelationships(ExpressionBuildingEventArgs e, 
                InstanceProducer[] dependencies)
            {
                var relationships =
                    from dependency in dependencies
                    select new KnownRelationship(e.KnownImplementationType, e.Lifestyle, dependency);

                foreach (KnownRelationship relationship in relationships)
                {
                    e.KnownRelationships.Add(relationship);
                }
            }

            private static Delegate BuildInjectorDelegate(Type type, MethodInfo method)
            {
                var targetParameter = Expression.Parameter(type, type.Name);

                var dependencyParameters = (
                    from parameter in method.GetParameters()
                    select Expression.Parameter(parameter.ParameterType, parameter.Name))
                    .ToArray();

                var expressions = BuildBlockExpressions(targetParameter, method, dependencyParameters);

                Type funcType = GetFuncType(type, method);

                var parameters = new[] { targetParameter }.Concat(dependencyParameters);

                var lambda = Expression.Lambda(funcType, Expression.Block(type, expressions), parameters);

                return lambda.Compile();
            }

            private static Type GetFuncType(Type injecteeType, MethodInfo method)
            {
                var genericTypeArguments = new List<Type> { injecteeType };

                genericTypeArguments.AddRange(
                    from parameter in method.GetParameters()
                    select parameter.ParameterType);

                // Return type is TResult. This is always the last generic type.
                genericTypeArguments.Add(injecteeType);

                var func = (
                    from funcType in FuncTypes
                    where funcType.GetGenericArguments().Length == genericTypeArguments.Count
                    select funcType)
                    .Single();

                return func.MakeGenericType(genericTypeArguments.ToArray());
            }

            private static List<Expression> BuildBlockExpressions(ParameterExpression targetParameter,
                MethodInfo method, ParameterExpression[] dependencyParameters)
            {
                var expressions = new List<Expression>();

                expressions.Add(Expression.Call(targetParameter, method, dependencyParameters));

                var returnTarget = Expression.Label(targetParameter.Type);

                expressions.Add(Expression.Return(returnTarget, targetParameter, targetParameter.Type));
                expressions.Add(Expression.Label(returnTarget, Expression.Constant(null, targetParameter.Type)));

                return expressions;
            }

            private static void VerifyMethod(MethodInfo method)
            {
                if (method.IsStatic)
                {
                    throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture,
                        "Method {0}.{1} is static, but only instance methods can be used for method injection.",
                        method.DeclaringType.ToFriendlyName(), method.Name));
                }

                if (method.ReturnType != typeof(void))
                {
                    throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture,
                        "Method {0}.{1} returns {2}, but injection is only supported the return type is " +
                        "void.", 
                        method.DeclaringType.ToFriendlyName(), method.Name, method.ReturnType.ToFriendlyName()));
                }

                if (!method.GetParameters().Any())
                {
                    throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture,
                        "Method {0}.{1} has no parameters. What do you expect me to inject here?", 
                        method.DeclaringType.ToFriendlyName(), method.Name));
                }

                if (method.GetParameters().Length > 15)
                {
                    throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture,
                        "Method {0}.{1} has more than 15 parameters. This is not supported.",
                        method.DeclaringType.ToFriendlyName(), method.Name));
                }
            }
        }
    }
}