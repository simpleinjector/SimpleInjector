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
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Reflection.Emit;
    using System.Threading;

    /// <summary>
    /// Helper methods for the container.
    /// </summary>
    internal static class Helpers
    {
        // Will be null when we're not running in a .NET 4.5 AppDomain.
        internal static readonly Type IReadOnlyCollectionType = 
            GetMsCorLibInterfaceType("System.Collections.Generic.IReadOnlyCollection`1");

        internal static readonly Type IReadOnlyListType =
            GetMsCorLibInterfaceType("System.Collections.Generic.IReadOnlyList`1");

        private static readonly Type[] AmbiguousTypes = new[] { typeof(Type), typeof(string) };
#if !SILVERLIGHT
        private static long dynamicClassCounter;
#endif
        internal static bool IsAmbiguousType(Type type)
        {
            return AmbiguousTypes.Contains(type);
        }

        internal static Lazy<T> ToLazy<T>(T value)
        {
            return new Lazy<T>(() => value, LazyThreadSafetyMode.None);
        }

        internal static IEnumerable<T> Concat<T>(this IEnumerable<T> source, T element)
        {
            return source.Concat(Enumerable.Repeat(element, 1));
        }

        internal static string ToCommaSeparatedText(this IEnumerable<string> values)
        {
            var names = values.ToArray();

            if (names.Length <= 1)
            {
                return names.FirstOrDefault() ?? string.Empty;
            }

            return string.Join(", ", names.Take(names.Length - 1)) + " and " + names.Last();
        }

        internal static string ToFriendlyName(this Type type)
        {
            if (type.IsArray)
            {
                return type.GetElementType().ToFriendlyName() + "[]";
            }

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

        internal static void VerifyCollection(IEnumerable collection, Type serviceType)
        {
            // This construct looks a bit weird, but prevents the collection from being iterated twice.
            bool collectionContainsNullElements = false;

            ThrowWhenCollectionCanNotBeIterated(collection, serviceType, item =>
            {
                collectionContainsNullElements |= item == null;
            });

            ThrowWhenCollectionContainsNullElements(serviceType, collectionContainsNullElements);
        }

        internal static bool IsConcreteType(Type serviceType)
        {
            // While array types are in fact concrete, we can not create them and creating them would be
            // pretty useless.
            return !serviceType.IsAbstract && !serviceType.ContainsGenericParameters && !serviceType.IsArray;
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

            if (actionArgumentType.IsAssignableFrom(typeof(T)))
            {
                // In most cases, the given T is a concrete type such as ServiceImpl, and supplied action
                // object can be everything from Action<ServiceImpl>, to Action<IService>, to Action<object>.
                // Since Action<T> is contravariant (we're running under .NET 4.0) we can simply cast it.
                return (Action<T>)action;
            }

            // If we come here, the given T is most likely System.Object and this means that the caller needs
            // a Action<object>, the instance that needs to be casted, so we we need to build the following
            // delegate:
            // instance => action((ActionType)instance);
            var parameter = Expression.Parameter(typeof(T), "instance");

            Expression argument = Expression.Convert(parameter, actionArgumentType);

            var instanceInitializer = Expression.Lambda<Action<T>>(
                Expression.Invoke(Expression.Constant(action), argument),
                parameter);

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

        // Compile the expression. If the expression is compiled in a dynamic assembly, the compiled delegate
        // is called (to ensure that it will run, because it tends to fail now and then) and the created
        // instance is returned through the out parameter. Note that NO created instance will be returned when
        // the expression is compiled using Expression.Compile)(.
        internal static Func<object> CompileAndRun(Container container, Expression expression,
            out object createdInstance)
        {
            createdInstance = null;

            var constantExpression = expression as ConstantExpression;

            // Skip compiling if all we need to do is return a singleton.
            if (constantExpression != null)
            {
                return CreateConstantOptimizedExpression(constantExpression);
            }

#if !SILVERLIGHT
            // Skip optimization in the Silverlight sandbox. Low memory use is much more important in this
            // environment.
            // In the common case, the developer will/should only create a single container during the 
            // lifetime of the application (this is the recommended approach). In this case, we can optimize
            // the perf by compiling delegates in an dynamic assembly. We can't do this when the developer 
            // creates many containers, because this will create a memory leak (dynamic assemblies are never 
            // unloaded). We might however relax this constraint and optimize the first N container instances.
            // (where N is configurable)
            if (container.Options.EnableDynamicAssemblyCompilation && 
                !ExpressionNeedsAccessToInternals(expression))
            {
                return CompileAndExecuteInDynamicAssemblyWithFallback(container, expression, out createdInstance);
            }
#endif
            return CompileLambda(expression);
        }

        internal static Delegate CompileLambdaInDynamicAssembly(Container container, LambdaExpression lambda, 
            string typeName, string methodName)
        {
            TypeBuilder typeBuilder = container.ModuleBuilder.DefineType(typeName, TypeAttributes.Public);

            MethodBuilder methodBuilder = typeBuilder.DefineMethod(methodName,
                MethodAttributes.Static | MethodAttributes.Public);

            lambda.CompileToMethod(methodBuilder);

            Type type = typeBuilder.CreateType();

            return Delegate.CreateDelegate(lambda.Type, type.GetMethod(methodName), true);
        }

#if !SILVERLIGHT
        // This doesn't find all possible cases, but get's us close enough.
        internal static bool ExpressionNeedsAccessToInternals(Expression expression)
        {
            var visitor = new InternalUseFinderVisitor();
            visitor.Visit(expression);
            return visitor.NeedsAccessToInternals;
        }
#endif

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

        private static Func<object> CompileLambda(Expression expression)
        {
            return Expression.Lambda<Func<object>>(expression).Compile();
        }

        private static void ThrowWhenCollectionCanNotBeIterated(IEnumerable collection, Type serviceType,
            Action<object> itemProcessor)
        {
            try
            {
                var enumerator = collection.GetEnumerator();
                try
                {
                    // Just iterate the collection.
                    while (enumerator.MoveNext())
                    {
                        itemProcessor(enumerator.Current);
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

        private static void ThrowWhenCollectionContainsNullElements(Type serviceType,
            bool collectionContainsNullItems)
        {
            if (collectionContainsNullItems)
            {
                throw new InvalidOperationException(
                    StringResources.ConfigurationInvalidCollectionContainsNullElements(serviceType));
            }
        }

        private static Type GetMsCorLibInterfaceType(string fullname)
        {
            var mscorlib = typeof(IEnumerable).Assembly;

            return (
                from type in mscorlib.GetExportedTypes()
                where type.IsInterface
                where type.FullName == fullname
                select type)
                .SingleOrDefault();
        }

#if !SILVERLIGHT
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes",
            Justification = "We can skip the exception, because we call the fallbackDelegate.")]
        private static Func<object> CompileAndExecuteInDynamicAssemblyWithFallback(Container container,
            Expression expression, out object createdInstance)
        {
            try
            {
                var @delegate = CompileInDynamicAssembly(container, expression);

                // Test the creation. Since we're using a dynamically created assembly, we can't create every
                // delegate we can create using expression.Compile(), so we need to test this. We need to 
                // store the created instance because we are not allowed to ditch that instance.
                createdInstance = @delegate();

                return @delegate;
            }
            catch
            {
                // The fallback. Here we don't execute the lambda, because this would mean that when the
                // execution fails the lambda is not returned and the compiled delegate would never be cached,
                // forcing a compilation hit on each call.
                createdInstance = null;
                return CompileLambda(expression);
            }
        }

        private static Func<object> CompileInDynamicAssembly(Container container, Expression expression)
        {
            ConstantExpression[] constantExpressions = GetConstants(expression).Distinct().ToArray();

            if (constantExpressions.Any())
            {
                return CompileInDynamicAssemblyAsClosure(container, expression, constantExpressions);
            }
            else
            {
                return CompileInDynamicAssemblyAsStatic(container, expression);
            }
        }

        private static Func<object> CompileInDynamicAssemblyAsClosure(Container container,
            Expression originalExpression, ConstantExpression[] constantExpressions)
        {
            // ConstantExpressions can't be compiled to a delegate using a MethodBuilder. We will have
            // to replace them to something that can be compiled: an object[] with constants.
            var constantsParameter = Expression.Parameter(typeof(object[]), "constants");

            var replacedExpression =
                ReplaceConstantsWithArrayLookup(originalExpression, constantExpressions, constantsParameter);

            var lambda = Expression.Lambda<Func<object[], object>>(replacedExpression, constantsParameter);

            Func<object[], object> create = CompileDelegateInDynamicAssembly(container, lambda);

            object[] contants = constantExpressions.Select(c => c.Value).ToArray();

            return () => create(contants);
        }

        private static Func<object> CompileInDynamicAssemblyAsStatic(Container container, Expression expression)
        {
            var lambda = Expression.Lambda<Func<object>>(expression, new ParameterExpression[0]);

            return CompileDelegateInDynamicAssembly(container, lambda);
        }

        private static Expression ReplaceConstantsWithArrayLookup(Expression expression,
            ConstantExpression[] constants, ParameterExpression constantsParameter)
        {
            var indexizer = new ConstantArrayIndexizerVisitor(constants, constantsParameter);

            return indexizer.Visit(expression);
        }

        private static TDelegate CompileDelegateInDynamicAssembly<TDelegate>(Container container,
            Expression<TDelegate> lambda)
        {
            return (TDelegate)(object)CompileLambdaInDynamicAssembly(container, lambda, 
                "DynamicInstanceProducer" + GetNextDynamicClassId(), "GetInstance");
        }

        private static List<ConstantExpression> GetConstants(Expression expression)
        {
            var constantFinder = new ConstantFinderVisitor();

            constantFinder.Visit(expression);

            return constantFinder.Constants;
        }

        private static long GetNextDynamicClassId()
        {
            return Interlocked.Increment(ref dynamicClassCounter);
        }

        private sealed class InternalUseFinderVisitor : ExpressionVisitor
        {
            public bool NeedsAccessToInternals { get; private set; }

            protected override Expression VisitNew(NewExpression node)
            {
                this.MayAccessExpression(node.Constructor.IsPublic && IsPublic(node.Constructor.DeclaringType));

                return base.VisitNew(node);
            }

            protected override Expression VisitMethodCall(MethodCallExpression node)
            {
                this.MayAccessExpression(node.Method.IsPublic && IsPublic(node.Method.DeclaringType));

                return base.VisitMethodCall(node);
            }

            protected override Expression VisitMember(MemberExpression node)
            {
                var property = node.Member as PropertyInfo;

                if (node.NodeType == ExpressionType.MemberAccess && property != null)
                {
                    this.MayAccessExpression(IsPublic(property.DeclaringType) && property.GetSetMethod() != null);
                }

                return base.VisitMember(node);
            }

            private void MayAccessExpression(bool mayAccess)
            {
                if (!mayAccess)
                {
                    this.NeedsAccessToInternals = true;
                }
            }

            private static bool IsPublic(Type type)
            {
                return type.IsPublic || type.IsNestedPublic;
            }
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