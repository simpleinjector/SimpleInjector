// Copyright (c) Simple Injector Contributors. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace SimpleInjector.Advanced
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;

    internal sealed class PropertyInjectionHelper(Container container, Type implementationType)
    {
        private const int MaximumNumberOfFuncArguments = 16;
        private const int MaximumNumberOfPropertiesPerDelegate = MaximumNumberOfFuncArguments - 1;

        private static readonly ReadOnlyCollection<Type> FuncTypes = new(
            [
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
            ]);

        internal static PropertyInjectionData BuildPropertyInjectionExpression(
            Container container,
            Type implementationType,
            PropertyInfo[] properties,
            Expression expressionToWrap)
        {
            var helper = new PropertyInjectionHelper(container, implementationType);

            return helper.BuildPropertyInjectionExpression(expressionToWrap, properties);
        }

        // #893: Behavior has changed in v6. It now returns private properties from base types (and internal
        // properties from base types in different libraries) as well. In v5 these properties were skipped,
        // but this lead to 'fail silent' behavior.
        internal static PropertyInfo[] GetCandidateInjectionPropertiesFor(Type implementationType)
        {
            List<PropertyWrapper> properties = [];

            // Iterates the type hierarchy from deepest base type to current implementationType
            foreach (var type in GetTypeHierarchy(implementationType))
            {
                AddPropertiesForTypeToList(type, properties);
            }

            return properties.Count == 0 ? [] : properties.Select(p => p.Property).ToArray();
        }

        private static void AddPropertiesForTypeToList(Type type, List<PropertyWrapper> properties)
        {
            IEnumerable<PropertyInfo> typeProperties = type.GetTypeInfo().DeclaredProperties;

            foreach (var typeProperty in typeProperties)
            {
                if (IsPropertyOverride(typeProperty, out Type? overriddenFromType))
                {
                    // Remove the property that was overridden, because we shouldn't inject a
                    // dependency twice into the same property.
                    RemovePropertyFromList(properties, typeProperty.Name, overriddenFromType!);

                    properties.Add(new PropertyWrapper(typeProperty, DeclaringBaseType: overriddenFromType!));
                }
                else
                {
                    properties.Add(new PropertyWrapper(typeProperty, DeclaringBaseType: type));
                }
            }
        }

        private static void RemovePropertyFromList(
            List<PropertyWrapper> properties, string propertyName, Type declaringType)
        {
            int index = FindPropertyInList(properties, propertyName, declaringType);

            properties.RemoveAt(index);
        }

        private static int FindPropertyInList(
            List<PropertyWrapper> properties, string propertyName, Type declaringType)
        {
            for (int i = 0; i < properties.Count; i++)
            {
                if (properties[i].Name == propertyName
                    && properties[i].DeclaringBaseType == declaringType)
                {
                    return i;
                }
            }

            throw new InvalidOperationException(
                $"Property {declaringType.Name}.{propertyName} not found. Actual items: " +
                string.Join(" + ", properties.Select(p => $"{p.DeclaringBaseType}.{p.Name}")));
        }

        private static bool IsPropertyOverride(PropertyInfo property, out Type? overriddenFromType)
        {
            overriddenFromType = null;

            var accessor = property.GetMethod ?? property.SetMethod;

            if (accessor is null) return false;

            // If the base definition is different, this accessor overrides a base accessor.
            MethodInfo baseMethod = accessor.GetBaseDefinition();

            if (baseMethod != accessor)
            {
                // The declaring type here will be the type that defined the property, not an intermediate
                // type that overridden the property.
                overriddenFromType = baseMethod.DeclaringType;

                return true;
            }
            else
            {
                return false;
            }
        }

        // Returns the type hierarchy, skipping System.Object, starting with the deepest type and ending with
        // the supplied type.
        private static List<Type> GetTypeHierarchy(Type type)
        {
            if (type.BaseType is null || type.BaseType == typeof(object))
            {
                return [type];
            }
            else
            {
                var hierarchy = GetTypeHierarchy(type.BaseType);
                hierarchy.Add(type);
                return hierarchy;
            }
        }

        internal static void VerifyProperties(PropertyInfo[] properties)
        {
            foreach (var property in properties)
            {
                VerifyProperty(property);
            }
        }

        private Delegate BuildPropertyInjectionDelegate(PropertyInfo[] properties)
        {
            try
            {
                return this.BuildPropertyInjectionDelegateInternal(properties);
            }
            catch (MemberAccessException ex)
            {
                // This happens when the user tries to resolve an internal type inside a (Silverlight) sandbox.
                throw new ActivationException(
                    StringResources.UnableToInjectPropertiesDueToSecurityConfiguration(implementationType,
                        ex),
                    ex);
            }
        }

        private Delegate BuildPropertyInjectionDelegateInternal(PropertyInfo[] properties)
        {
            var targetParameter = Expression.Parameter(implementationType, implementationType.Name);

            var dependencyParameters = (
                from property in properties
                select Expression.Parameter(property.PropertyType, property.Name))
                .ToArray();

            var propertyInjectionExpressions =
                this.BuildPropertyInjectionExpressions(targetParameter, properties, dependencyParameters);

            Type funcType = GetFuncType(properties, implementationType);

            var parameters = dependencyParameters.Concat([targetParameter]);

            var lambda = Expression.Lambda(
                funcType,
                Expression.Block(implementationType, propertyInjectionExpressions),
                parameters);

            return container.Options.ExpressionCompilationBehavior.Compile(lambda);
        }

        private List<Expression> BuildPropertyInjectionExpressions(ParameterExpression targetParameter,
            PropertyInfo[] properties,
            ParameterExpression[] dependencyParameters)
        {
            var blockExpressions = (
                from pair in properties.Zip(dependencyParameters, (prop, param) => new { prop, param })
                select Expression.Assign(Expression.Property(targetParameter, pair.prop), pair.param))
                .Cast<Expression>()
                .ToList();

            var returnTarget = Expression.Label(implementationType);

            blockExpressions.Add(Expression.Return(returnTarget, targetParameter, implementationType));
            blockExpressions.Add(Expression.Label(returnTarget, Expression.Constant(null, implementationType)));

            return blockExpressions;
        }

        private static void VerifyProperty(PropertyInfo property)
        {
            MethodInfo? setMethod = property.GetSetMethod(nonPublic: true)
                ?? throw new ActivationException(StringResources.PropertyHasNoSetter(property));

            if (setMethod.IsStatic)
            {
                throw new ActivationException(StringResources.PropertyIsStatic(property));
            }
        }

        private PropertyInjectionData BuildPropertyInjectionExpression(
            Expression expression, PropertyInfo[] properties)
        {
            PropertyInjectionData data;

            // With MaximumNumberOfPropertiesPerDelegate = 4
            // new Impl { P1 = Dep1, P2 = Dep2, P3 = Dep3, P4 = Dep4, P5 = Dep5, P6 = Dep6, P6 = Dep7)
            // We build up an expression like this:
            // () => func1(Dep1, Dep2, Dep3, func2(Dep4, Dep5, Dep6, func3(Dep7, new Impl())))
            if (properties.Length > MaximumNumberOfPropertiesPerDelegate)
            {
                // Expression becomes: Func<Prop8, Prop9, ... , PropN, TargetType>
                var restProperties = properties.Skip(MaximumNumberOfPropertiesPerDelegate).ToArray();

                // Properties becomes { Prop1, Prop2, ..., Prop7 }.
                properties = properties.Take(MaximumNumberOfPropertiesPerDelegate).ToArray();

                data = this.BuildPropertyInjectionExpression(expression, restProperties);
            }
            else
            {
                data = new PropertyInjectionData(expression);
            }

            InstanceProducer[] producers = this.GetPropertyInstanceProducers(properties);

            var arguments = producers.Select(p => p.BuildExpression()).Concat([data.Expression]);

            Delegate propertyInjectionDelegate = this.BuildPropertyInjectionDelegate(properties);

            return new PropertyInjectionData(
                Expression: Expression.Invoke(Expression.Constant(propertyInjectionDelegate), arguments),
                Producers: producers.Concat(data.Producers),
                Properties: properties.Concat(data.Properties));
        }

        private InstanceProducer[] GetPropertyInstanceProducers(PropertyInfo[] properties)
        {
            return properties.Select(this.GetPropertyExpression).ToArray();
        }

        private InstanceProducer GetPropertyExpression(PropertyInfo property)
        {
            var consumer = new InjectionConsumerInfo(implementationType, property);

            return container.Options.GetInstanceProducerFor(consumer);
        }

        private static Type GetFuncType(PropertyInfo[] properties, Type injecteeType)
        {
            var genericTypeArguments = new List<Type>();

            genericTypeArguments.AddRange(from property in properties select property.PropertyType);

            genericTypeArguments.Add(injecteeType);

            // Return type is TResult. This is always the last generic type.
            genericTypeArguments.Add(injecteeType);

            int numberOfInputArguments = genericTypeArguments.Count;

            Type openGenericFuncType = FuncTypes[numberOfInputArguments - 1];

            return openGenericFuncType.MakeGenericType(genericTypeArguments.ToArray());
        }

        internal readonly record struct PropertyInjectionData(
            Expression Expression,
            IEnumerable<InstanceProducer> Producers,
            IEnumerable<PropertyInfo> Properties)
        {
            public PropertyInjectionData(Expression expression) : this(expression, [], []) { }
        }

        private readonly record struct PropertyWrapper(PropertyInfo Property, Type DeclaringBaseType)
        {
            public string Name => this.Property.Name;
        }
    }
}