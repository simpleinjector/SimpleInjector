namespace SimpleInjector.CodeSamples
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;

    public static class ExplicitPropertyInjectionExtensions
    {
        public static void AutowireProperties<TAttribute>(this Container container)
            where TAttribute : Attribute
        {
            if (container == null)
            {
                throw new ArgumentNullException("container");
            }

            EnsureNoRegistrationsHaveBeenMade(container);

            var helper = new PropertyInjectionHelper
            {
                AttributeType = typeof(TAttribute),
                Container = container,
            };

            container.ExpressionBuilding += helper.ReplaceExpression;
        }

        private static void EnsureNoRegistrationsHaveBeenMade(Container container)
        {
            try
            {
                // set_ConstructorResolutionBehavior throws after the first registration.
                container.Options.ConstructorResolutionBehavior = container.Options.ConstructorResolutionBehavior;
            }
            catch (InvalidOperationException)
            {
                throw new InvalidOperationException(
                    "This method must be called before any registration has been made to the container.");
            }
        }

        private sealed class PropertyInjectionHelper
        {
            private const int MaximumNumberOfFuncArguments = 16;
            private const int MaximumNumberOfPropertiesPerDelegate = MaximumNumberOfFuncArguments - 1;

            private static readonly ReadOnlyCollection<Type> FuncTypes = new ReadOnlyCollection<Type>(new Type[]
            {
                null,
                typeof(Func<>),
                typeof(Func<,>),
                typeof(Func<,,>),
                typeof(Func<,,,>),
                typeof(Func<,,,,>),
                typeof(Func<,,,,,>),
                typeof(Func<,,,,,,>),
                typeof(Func<,,,,,,,>),
                typeof(Func<,,,,,,,,>),
                typeof(Func<,,,,,,,,,>),
                typeof(Func<,,,,,,,,,,>),
                typeof(Func<,,,,,,,,,,,>),
                typeof(Func<,,,,,,,,,,,,>),
                typeof(Func<,,,,,,,,,,,,,>),
                typeof(Func<,,,,,,,,,,,,,,>),
                typeof(Func<,,,,,,,,,,,,,,,>),
                typeof(Func<,,,,,,,,,,,,,,,,>),
            });

            internal Type AttributeType { get; set; }

            internal Container Container { get; set; }

            public void ReplaceExpression(object sender, ExpressionBuildingEventArgs e)
            {
                var expression = e.Expression as NewExpression;

                if (expression == null)
                {
                    return;
                }
                
                Type type = expression.Constructor.DeclaringType;

                var propertiesToInject = this.GetPropertiesToInject(type);

                if (propertiesToInject.Any())
                {
                    e.Expression = this.BuildPropertyInjectionExpression(type, expression, propertiesToInject);
                }
            }
            
            private PropertyInfo[] GetPropertiesToInject(Type type)
            {
                var everything =
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;

                var propertiesWithAttribute = (
                    from property in type.GetProperties(everything)
                    where property.GetCustomAttributes(this.AttributeType, true).Any()
                    select property)
                    .ToArray();

                ValidateProperties(propertiesWithAttribute);

                return propertiesWithAttribute;
            }

            private static void ValidateProperties(PropertyInfo[] properties)
            {
                foreach (var property in properties)
                {
                    ValidateProperty(property);
                }
            }

            private static void ValidateProperty(PropertyInfo property)
            {
                var setter = property.GetSetMethod();

                if (!property.CanWrite || setter == null)
                {
                    throw new Exception("No set method.");
                }

                if (!setter.IsPublic)
                {
                    throw new Exception("Setter not public.");
                }
            }

            private Expression BuildPropertyInjectionExpression(Type targetType, Expression expression, 
                PropertyInfo[] properties)
            {
                // Build up an expression like this:
                // () => func1(func2(func3(new TargetType(...), Dep7), Dep4, Dep5, Dep6), Dep1, Dep2, Dep3)
                if (properties.Length > MaximumNumberOfPropertiesPerDelegate)
                {
                    // Expression becomes: Func<TargetType, Prop8, Prop9, ... , PropN>
                    expression = this.BuildPropertyInjectionExpression(targetType, expression, 
                        properties.Skip(MaximumNumberOfPropertiesPerDelegate).ToArray());

                    // Properties becomes { Prop1, Prop2, ..., Prop7 }.
                    properties = properties.Take(MaximumNumberOfPropertiesPerDelegate).ToArray();
                }
                
                Expression[] dependencyExpressions = this.GetPropertyExpressions(properties);

                var arguments = new[] { expression }.Concat(dependencyExpressions);

                Delegate propertyInjectionDelegate = BuildPropertyInjectionDelegate(targetType, properties);

                return Expression.Invoke(Expression.Constant(propertyInjectionDelegate), arguments);
            }

            private static Delegate BuildPropertyInjectionDelegate(Type targetType, PropertyInfo[] properties)
            {
                var targetParameter = Expression.Parameter(targetType, targetType.Name);

                var dependencyParameters = (
                    from property in properties
                    select Expression.Parameter(property.PropertyType, property.Name))
                    .ToArray();

                var returnTarget = Expression.Label(targetType);
                var returnExpression = Expression.Return(returnTarget, targetParameter, targetType);
                var returnLabel = Expression.Label(returnTarget, Expression.Constant(null, targetType));

                var blockExpressions = (
                    from pair in properties.Zip(dependencyParameters, (prop, param) => new { prop, param })
                    select Expression.Assign(Expression.Property(targetParameter, pair.prop), pair.param))
                    .Cast<Expression>()
                    .ToList();

                blockExpressions.Add(returnExpression);
                blockExpressions.Add(returnLabel);

                Type funcType = GetFuncType(targetType, properties);

                var parameters = new[] { targetParameter }.Concat(dependencyParameters);

                var lambda = 
                    Expression.Lambda(funcType, Expression.Block(targetType, blockExpressions), parameters);

                return lambda.Compile();
            }

            private Expression[] GetPropertyExpressions(PropertyInfo[] properties)
            {
                return (
                    from property in properties
                    select this.GetPropertyExpression(property))
                    .ToArray();
            }

            private Expression GetPropertyExpression(PropertyInfo property)
            {
                var registration = this.Container.GetRegistration(property.PropertyType, throwOnFailure: true);

                return registration.BuildExpression();
            }

            private static Type GetFuncType(Type injecteeType, PropertyInfo[] properties)
            {
                var genericTypeArguments = new List<Type> { injecteeType };

                genericTypeArguments.AddRange(from property in properties select property.PropertyType);

                // Return type
                genericTypeArguments.Add(injecteeType);

                int numberOfInputArguments = genericTypeArguments.Count;

                Type openGenericFuncType = FuncTypes[numberOfInputArguments];

                return openGenericFuncType.MakeGenericType(genericTypeArguments.ToArray());
            }
        }
    }
}