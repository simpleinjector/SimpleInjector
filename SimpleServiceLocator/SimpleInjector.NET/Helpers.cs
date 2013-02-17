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
    using System.Reflection.Emit;
    
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

        internal static Func<object> CompileExpression(Container container, Expression expression)
        {
            // Skip compiling if all we need to do is return a singleton.
            if (expression is ConstantExpression)
            {
                return CreateConstantOptimizedExpression(expression as ConstantExpression);
            }

#if !SILVERLIGHT
            // In the common case, the developer will only create a single container during the lifetime of
            // the application (this is the recommended approach). In this case, we can optimize the perf
            // by compiling delegates in an dynamic assembly. We can't do this when the developer creates
            // many containers, because this will create a memory leak (dynamic assemblies are never unloaded).
            if (container.IsFirst)
            {
                return CompileOptimizedWithFallback(expression);
            }
            else
            {
                return CompileUnoptimized(expression);
            }
#else
            // Skip optimization in the Silverlight sandbox. Low memory use is much more important in this
            // environment.
            return CompileUnoptimized(expression);
#endif
        }

        private static Func<object> CreateConstantOptimizedExpression(ConstantExpression expression)
        {
            object singleton = expression.Value;

            // This lambda will be a tiny little bit faster than the instanceCreator.
            return () => singleton;
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

        
        private static Func<object> CompileUnoptimized(Expression expression)
        {
            return Expression.Lambda<Func<object>>(expression).Compile();
        }

#if !SILVERLIGHT
        private static Func<object> CompileOptimizedWithFallback(Expression expression)
        {
            Func<object> fastDelete = CompileOptimized(expression);
            Func<object> fallbackDelegate = null;

            return () =>
            {
                if (fallbackDelegate != null)
                {
                    return fallbackDelegate();
                }

                try
                {
                    return fastDelete();
                }
                catch
                {
                    // Sometimes the execution of a dynamic assembly delegate fails. In that case we must
                    // fallback to the unoptimized behavior.
                    fallbackDelegate = CompileUnoptimized(expression);
                    return fallbackDelegate();
                }
            };
        }

        private static Func<object> CompileOptimized(Expression expression)
        {
            ConstantExpression[] constantExpressions = GetConstants(expression).Distinct().ToArray();

            if (constantExpressions.Any())
            {
                return CompileAsClosure(expression, constantExpressions);
            }
            else
            {
                return CompileAsStatic(expression);
            }
        }

        private static Func<object> CompileAsClosure(Expression originalExpression, 
            ConstantExpression[] constantExpressions)
        {
            // ConstantExpressions can't be compiled to a delegate using a MethodBuilder. We will have
            // to replace them to something that can be compiled: an object[] with constants.
            var constantsParameter = Expression.Parameter(typeof(object[]), "constants");

            var replacedExpression = 
                ReplaceConstantsWithArrayLookup(originalExpression, constantExpressions, constantsParameter);

            var lambda = Expression.Lambda<Func<object[], object>>(replacedExpression, constantsParameter);

            var create = CompileDelegateInDynamicAssembly(lambda);

            object[] contants = constantExpressions.Select(c => c.Value).ToArray();

            return () => create(contants);
        }

        private static Func<object> CompileAsStatic(Expression expression)
        {
            var lambda = Expression.Lambda<Func<object>>(expression, new ParameterExpression[0]);

            return CompileDelegateInDynamicAssembly(lambda);
        }

        private static Expression ReplaceConstantsWithArrayLookup(Expression expression,
            ConstantExpression[] constants, ParameterExpression constantsParameter)
        {
            var indexizer = new ConstantArrayIndexizerVisitor(constants, constantsParameter);

            return indexizer.Visit(expression);
        }

        private static T CompileDelegateInDynamicAssembly<T>(Expression<T> lambda)
        {
            var assemblyName = new AssemblyName("SimpleInjector.Compiled");

            AssemblyBuilder assemblyBuilder =
                AppDomain.CurrentDomain.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);

            ModuleBuilder moduleBuilder = assemblyBuilder.DefineDynamicModule("SimpleInjector.CompiledModule");
            TypeBuilder typeBuilder = moduleBuilder.DefineType("DynamicInstanceProducer", TypeAttributes.Public);
            MethodBuilder methodBuilder = typeBuilder.DefineMethod("GetInstance",
                MethodAttributes.Static | MethodAttributes.Public);

            lambda.CompileToMethod(methodBuilder);

            Type type = typeBuilder.CreateType();

            return (T)(object)Delegate.CreateDelegate(lambda.Type, type.GetMethod("GetInstance"), true);
        }
        
        private static List<ConstantExpression> GetConstants(Expression expression)
        {
            var constantFinder = new ConstantFinderVisitor();

            constantFinder.Visit(expression);

            return constantFinder.Constants;
        }

        private sealed class ConstantFinderVisitor : ExpressionVisitor
        {
            internal readonly List<ConstantExpression> Constants = new List<ConstantExpression>();

            protected override Expression VisitConstant(ConstantExpression node)
            {
                if (!node.Type.IsPrimitive)
                {
                    this.Constants.Add(node);
                }

                return base.VisitConstant(node);
            }
        }

        private sealed class ConstantArrayIndexizerVisitor : ExpressionVisitor
        {
            private readonly List<ConstantExpression> constantExpressions;
            private readonly ParameterExpression constantsParameter;

            public ConstantArrayIndexizerVisitor(ConstantExpression[] constantExpressions,
                ParameterExpression constantsParameter)
            {
                this.constantExpressions = constantExpressions.ToList();
                this.constantsParameter = constantsParameter;
            }

            protected override Expression VisitConstant(ConstantExpression node)
            {
                int index = this.constantExpressions.IndexOf(node);

                if (index >= 0)
                {
                    return Expression.Convert(
                        Expression.ArrayIndex(
                            this.constantsParameter,
                            Expression.Constant(index, typeof(int))),
                        node.Type);
                }

                return base.VisitConstant(node);
            }
        }
#endif
    }
}