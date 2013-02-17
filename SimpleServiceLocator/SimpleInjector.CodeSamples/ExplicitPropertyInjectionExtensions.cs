namespace SimpleInjector.CodeSamples
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using SimpleInjector.Advanced;

    public static class ExplicitPropertyInjectionExtensions
    {
        public static void AutowireProperties<TAttribute>(this Container container)
            where TAttribute : Attribute
        {
            Predicate<PropertyInfo> selector = 
                property => property.GetCustomAttributes(typeof(TAttribute), true).Any();

            AutowireProperties(container, selector);
        }

        public static void AutowireProperties(this Container container, Predicate<PropertyInfo> selector)
        {
            if (container == null)
            {
                throw new ArgumentNullException("container");
            }

            if (selector == null)
            {
                throw new ArgumentNullException("selector");
            }
            
            EnsureNoRegistrationsHaveBeenMade(container);

            var helper = new PropertyInjectionHelper { Container = container, Selector = selector};

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
            private readonly HashSet<object> initializedSingletons = new HashSet<object>();

            private const int MaximumNumberOfFuncArguments = 16;
            private const int MaximumNumberOfPropertiesPerDelegate = MaximumNumberOfFuncArguments - 1;

            private static readonly ReadOnlyCollection<Type> FuncTypes = new ReadOnlyCollection<Type>(new Type[]
            {
                // This is how I like my christmas tree :-)
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

            internal Predicate<PropertyInfo> Selector { get; set; }

            internal Container Container { get; set; }

            public void ReplaceExpression(object sender, ExpressionBuildingEventArgs e)
            {
                var propertiesToInject = this.GetPropertiesToInject(e.KnownImplementationType);

                if (propertiesToInject.Any())
                {
                    var expressionWithPropertyInjection = 
                        this.BuildPropertyInjectionExpression(e.KnownImplementationType, e.Expression, 
                            propertiesToInject);

                    if (e.Expression is ConstantExpression)
                    {
                        expressionWithPropertyInjection = MakeConstantAgain(e.Expression as ConstantExpression, 
                            expressionWithPropertyInjection, e.KnownImplementationType);
                    }

                    e.Expression = expressionWithPropertyInjection;

                    this.AddKnownRelationships(e, propertiesToInject);
                }
            }

            private void AddKnownRelationships(ExpressionBuildingEventArgs e, PropertyInfo[] propertiesToInject)
            {
                var propertyRelationships =
                    from property in propertiesToInject
                    let dependency = this.Container.GetRegistration(property.PropertyType, true)
                    select new KnownRelationship(e.KnownImplementationType, e.Lifestyle, dependency);

                foreach (var relationship in propertyRelationships)
                {
                    e.KnownRelationships.Add(relationship);
                }
            }

            private PropertyInfo[] GetPropertiesToInject(Type type)
            {
                var everything =
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;

                var propertiesWithAttribute = (
                    from property in type.GetProperties(everything)
                    where this.Selector(property)
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

            private Expression MakeConstantAgain(ConstantExpression oldExpression, Expression newExpression,
                Type knownImplementationType)
            {
                lock (this.initializedSingletons)
                {
                    if (!this.initializedSingletons.Contains(oldExpression.Value))
                    {
                        var instanceCreator = Expression.Lambda<Func<object>>(newExpression).Compile();

                        // Injects properties.
                        instanceCreator();

                        this.initializedSingletons.Add(oldExpression.Value);
                    }
                }

                // We can simply return the old expression now, the properties have been injected.
                return oldExpression;
            }
        }
    }
}