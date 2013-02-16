#region Copyright (c) 2010 S. van Deursen
/* The Simple Injector is an easy-to-use Inversion of Control library for .NET
 * 
 * Copyright (C) 2010 S. van Deursen
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

namespace SimpleInjector
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    
    /// <summary>
    /// Helper methods for the container.
    /// </summary>
    internal static class Helpers
    {
        internal static string ToFriendlyName(this Type type)
        {
            string name = type.Name;

            if (type.IsNested && !type.IsGenericParameter)
            {
                name = type.DeclaringType.ToFriendlyName() + "+" + type.Name;
            }

            var genericArguments = GetGenericArguments(type);

            if (genericArguments.Length == 0)
            {
                return name;
            }

            name = name.Substring(0, name.IndexOf('`'));

            var argumentNames = genericArguments.Select(argument => argument.ToFriendlyName()).ToArray();

            return name + "<" + string.Join(", ", argumentNames) + ">";
        }

        internal static IEnumerable<T> MakeReadOnly<T>(this IEnumerable<T> collection)
        {
            bool typeIsReadOnlyCollection = collection is ReadOnlyCollection<T>;

            bool typeIsMutable = collection is T[] || collection is IList || collection is ICollection<T>;

            if (typeIsReadOnlyCollection || !typeIsMutable)
            {
                return collection;
            }
            else
            {
                return CreateReadOnlyCollection(collection);
            }
        }
        
        // Throws an InvalidOperationException on failure.
        internal static void Verify(this InstanceProducer instanceProducer)
        {
            try
            {
                // Test the creator
                // NOTE: We've got our first quirk in the design here: The returned object could implement
                // IDisposable, but there is no way for us to know if we should actually dispose this 
                // instance or not :-(. Disposing it could make us prevent a singleton from ever being
                // used; not disposing it could make us leak resources :-(.
                instanceProducer.GetInstance();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(StringResources.ConfigurationInvalidCreatingInstanceFailed(
                    instanceProducer.ServiceType, ex), ex);
            }
        }

        internal static Dictionary<TKey, TValue> MakeCopyOf<TKey, TValue>(Dictionary<TKey, TValue> source)
        {
            // We choose an initial capacity of count + 1, because we'll be adding 1 item to this copy.
            int initialCapacity = source.Count + 1;

            var copy = new Dictionary<TKey, TValue>(initialCapacity);

            foreach (var pair in source)
            {
                copy.Add(pair.Key, pair.Value);
            }

            return copy;
        }

        internal static void ThrowWhenCollectionCanNotBeIterated(IEnumerable collection, Type serviceType)
        {
            try
            {
                var enumerator = collection.GetEnumerator();
                try
                {
                    // Just iterate the collection.
                    while (enumerator.MoveNext())
                    {
                    }
                }
                finally
                {
                    IDisposable disposable = enumerator as IDisposable;

                    if (disposable != null)
                    {
                        disposable.Dispose();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    StringResources.ConfigurationInvalidIteratingCollectionFailed(serviceType, ex), ex);
            }
        }

        internal static bool IsConcreteType(Type serviceType)
        {
            // While array types are in fact concrete, we can not create them and creating them would be
            // pretty useless.
            return !serviceType.IsAbstract && !serviceType.ContainsGenericParameters && !serviceType.IsArray;
        }

        internal static void ThrowWhenCollectionContainsNullArguments(IEnumerable collection, Type serviceType)
        {
            bool collectionContainsNullItems = collection.Cast<object>().Any(c => c == null);

            if (collectionContainsNullItems)
            {
                throw new InvalidOperationException(
                    StringResources.ConfigurationInvalidCollectionContainsNullElements(serviceType));
            }
        }

        // Return a list of all base types T inherits, all interfaces T implements and T itself.
        internal static Type[] GetTypeHierarchyFor(Type type)
        {
            var types = new List<Type>();

            types.Add(type);
            types.AddRange(GetBaseTypes(type));
            types.AddRange(type.GetInterfaces());

            return types.ToArray();
        }

        internal static Action<T> CreateAction<T>(object action)
        {
            Type actionArgumentType = action.GetType().GetGenericArguments()[0];

            ParameterExpression objParameter = Expression.Parameter(typeof(T), "obj");

            // Build the following expression: obj => action(obj);
            var instanceInitializer = Expression.Lambda<Action<T>>(
                Expression.Invoke(
                    Expression.Constant(action),
                    new[] { Expression.Convert(objParameter, actionArgumentType) }),
                new ParameterExpression[] { objParameter });

            return instanceInitializer.Compile();
        }

        internal static IEnumerable CastCollection(IEnumerable collection, Type resultType)
        {
            IEnumerable castedCollection;

            if (typeof(IEnumerable<>).MakeGenericType(resultType).IsAssignableFrom(collection.GetType()))
            {
                // The collection is a IEnumerable<[ServiceType]>. We can simply cast it. 
                // Better for performance
                castedCollection = collection;
            }
            else
            {
                // The collection is not a IEnumerable<[ServiceType]>. We wrap it in a 
                // CastEnumerator<[ServiceType]> to be able to supply it to the RegisterAll<T> method.
                var castMethod = typeof(Enumerable).GetMethod("Cast").MakeGenericMethod(resultType);

                castedCollection = (IEnumerable)castMethod.Invoke(null, new[] { collection });
            }

            return castedCollection;
        }
        
        internal static Func<object> OptimizeInstanceCreator(Func<object> instanceCreator, Expression expression)
        {
            if (expression is ConstantExpression)
            {
                object singleton = instanceCreator();

                // This lambda will be a tiny little bit faster than the instanceCreator.
                return () => singleton;
            }

            return instanceCreator;
        }

        internal static Expression OptimizeExpressionForBuildingDelegate(Expression expression)
        {
            // Can't create an Tuple with more than 7 arguments.
            const int ItemMax = 4;

            var collector = new ConstantCollector();
            collector.Visit(expression);

            if (collector.Constants.Count > 1)
            {
                var constants = collector.Constants.Distinct().Take(ItemMax).ToList();

                return new ConstantTuplizer(constants).Visit(expression);
            }

            return expression;
        }

        private static IEnumerable<Type> GetBaseTypes(Type type)
        {
            Type baseType = type.BaseType;

            while (baseType != null)
            {
                yield return baseType;

                baseType = baseType.BaseType;
            }
        }

        private static IEnumerable<T> CreateReadOnlyCollection<T>(IEnumerable<T> collection)
        {
            return RegisterAllEnumerable(collection);
        }

        // This method name does not describe what it does, but since the C# compiler will create a iterator
        // type named after this method, it allows us to return a type that has a nice name that will show up
        // during debugging.
        private static IEnumerable<T> RegisterAllEnumerable<T>(IEnumerable<T> collection)
        {
            foreach (var item in collection)
            {
                yield return item;
            }
        }

        private static Type[] GetGenericArguments(Type type)
        {
            if (!type.Name.Contains('`'))
            {
                return Type.EmptyTypes;
            }

            int numberOfGenericArguments = Convert.ToInt32(type.Name.Substring(type.Name.IndexOf('`') + 1),
                 CultureInfo.InvariantCulture);

            var argumentOfTypeAndOuterType = type.GetGenericArguments();

            return argumentOfTypeAndOuterType
                .Skip(argumentOfTypeAndOuterType.Length - numberOfGenericArguments)
                .ToArray();
        }

        private sealed class ConstantCollector : ExpressionVisitor
        {
            internal readonly List<ConstantExpression> Constants = new List<ConstantExpression>();

            protected override Expression VisitConstant(ConstantExpression node)
            {
                if (!node.Type.IsValueType)
                {
                    this.Constants.Add(node);
                }

                return base.VisitConstant(node);
            }
        }

        private sealed class ConstantTuplizer : ExpressionVisitor
        {
            private static readonly IEnumerable<MethodInfo> TupleCreateMethods = (
                from method in typeof(Tuple).GetMethods()
                where method.Name == "Create"
                select method)
                .ToArray();

            private readonly ConstantExpression tupleConstant;
            private readonly List<ConstantExpression> constants;
            private readonly PropertyInfo[] properties;

            public ConstantTuplizer(List<ConstantExpression> constants)
            {
                this.constants = constants;
                var tuple = CreateTuple(constants);
                this.tupleConstant = Expression.Constant(tuple);
                this.properties = (
                    from property in tuple.GetType().GetProperties()
                    orderby property.Name
                    select property)
                    .ToArray();
            }

            protected override Expression VisitConstant(ConstantExpression node)
            {
                if (this.constants.Contains(node))
                {
                    var property = this.properties[this.constants.IndexOf(node)];

                    return Expression.Convert(Expression.Property(this.tupleConstant, property), node.Type);
                }

                return base.VisitConstant(node);
            }

            private static object CreateTuple(List<ConstantExpression> constants)
            {
                var constantTypes = constants.Select(c => c.Type).ToArray();

                var createMethodDefinition = (
                    from method in TupleCreateMethods
                    where method.GetGenericArguments().Length == constants.Count
                    select method)
                    .Single();

                var createMethod = createMethodDefinition.MakeGenericMethod(constantTypes);

                var constantValues = constants.Select(c => c.Value).ToArray();
                
                return createMethod.Invoke(null, constantValues);
            }
        }
    }
}