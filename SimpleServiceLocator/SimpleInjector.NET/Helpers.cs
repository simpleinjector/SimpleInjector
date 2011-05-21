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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace SimpleInjector
{
    /// <summary>
    /// Helper methods for the container.
    /// </summary>
    internal static class Helpers
    {
        internal const string InstanceProviderDebuggerDisplayString =
            "ServiceType: {((IInstanceProducer)this).ServiceType}, Expression: {((IInstanceProducer)this).BuildExpression().ToString()}";    

        private static readonly MethodInfo GetInstanceOfT = GetGenericMethod(c => c.GetInstance<object>());

        internal static IInstanceProducer CreateTransientInstanceProducerFor(Type serviceType, 
            Container container)
        {
            Type instanceProducerType = typeof(TransientInstanceProducer<>).MakeGenericType(serviceType);

            // HACK: Because of the security level of Silverlight applications, we can't create an transient
            // instance producer using reflection; it is an internal type. We can however, abuse the 
            // container.GetInstance<T> method to create a new instance, because GetInstance<T> is public :-).
            var factory = new Container();

            var genericGetInstanceMethod = GetInstanceOfT.MakeGenericMethod(instanceProducerType);

            object instanceProducer;

            try
            {
                // Here we call: "factory.GetInstance<TransientInstanceProducer<[ServiceType], [Implementation]>>()".
                // We don't use the incoming container, because we don't want to polute the actual container. 
                instanceProducer = genericGetInstanceMethod.Invoke(factory, null);    
            }
            catch (MemberAccessException ex)
            {
                // This happens when the user tries to resolve an internal type inside a (Silverlight) sandbox.
                throw new ActivationException(
                    StringResources.UnableToResolveTypeDueToSecurityConfiguration(serviceType), ex);
            }

            ((ITransientInstanceProducer)instanceProducer).Container = container;

            return (IInstanceProducer)instanceProducer;
        }

        internal static IEnumerable<T> MakeImmutable<T>(this IEnumerable<T> collection)
        {
            bool typeIsReadOnlyCollection = collection is ReadOnlyCollection<T>;

            bool typeIsMutable = collection is T[] || collection is IList || collection is ICollection<T>;

            if (typeIsReadOnlyCollection || !typeIsMutable)
            {
                return collection;
            }
            else
            {
                return CreateImmutableCollection(collection);
            }
        }

        // Throws an InvalidOperationException on failure.
        internal static void Verify(this IInstanceProducer instanceProducer, Type serviceType)
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
                throw new InvalidOperationException(
                    StringResources.ConfigurationInvalidCreatingInstanceFailed(serviceType, ex), ex);
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

        internal static void ThrowActivationExceptionWhenTypeIsNotConstructable(Type serviceType)
        {
            string exceptionMessage;

            if (!IsConcreteConstructableType(serviceType, out exceptionMessage))
            {
                throw new ActivationException(
                    StringResources.ImplicitRegistrationCouldNotBeMadeForType(serviceType) + exceptionMessage);
            }
        }

        internal static void ThrowArgumentExceptionWhenTypeIsNotConstructable(Type serviceType, 
            string parameterName)
        {
            string exceptionMessage;

            if (!IsConcreteConstructableType(serviceType, out exceptionMessage))
            {
                // After some doubt (and even after reading http://bit.ly/1CPDv9) I decided to throw an
                // ArgumentException when the given generic type argument was invalid. Mainly because a
                // generic type argument is just an argument, and ArgumentException even allows us to supply 
                // the name of the argument. No developer will be surprise to see an ArgEx in this case.
                throw new ArgumentException(exceptionMessage, parameterName);
            }
        }

        internal static void ValidateIfCollectionCanBeIterated(IEnumerable collection, Type serviceType)
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
            return !serviceType.IsAbstract && !serviceType.IsGenericTypeDefinition && !serviceType.IsArray;
        }

        internal static void ValidateIfCollectionForNullElements(IEnumerable collection, Type serviceType)
        {
            bool collectionContainsNullItems = collection.Cast<object>().Any(c => c == null);

            if (collectionContainsNullItems)
            {
                throw new InvalidOperationException(
                    StringResources.ConfigurationInvalidCollectionContainsNullElements(serviceType));
            }
        }

        /// <summary>Return a list of all base types T inherits, all interfaces T implements and T itself.</summary>
        /// <typeparam name="T">The type for get the type hierarchy from.</typeparam>
        /// <returns>A list of type objects.</returns>
        internal static Type[] GetTypeHierarchyFor<T>()
        {
            List<Type> types = new List<Type>();
            types.Add(typeof(T));
            types.AddRange(GetBaseTypes(typeof(T)));
            types.AddRange(typeof(T).GetInterfaces());

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

        internal static bool IsConcreteConstructableType(Type serviceType)
        {
            string errorMesssage;

            return IsConcreteConstructableType(serviceType, out errorMesssage);
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

        private static bool HasSinglePublicConstructor(Type serviceType)
        {
            return serviceType.GetConstructors().Length == 1;
        }

        private static bool HasConstructorWithOnlyValidParameters(Type serviceType)
        {
            return GetFirstInvalidConstructorParameter(serviceType) == null;
        }

        private static ParameterInfo GetFirstInvalidConstructorParameter(Type serviceType)
        {
            return (
                from constructor in serviceType.GetConstructors()
                from parameter in constructor.GetParameters()
                let type = parameter.ParameterType
                where type.IsValueType || type == typeof(string)
                select parameter).FirstOrDefault();
        }

        private static bool IsConcreteConstructableType(Type serviceType, out string errorMessage)
        {
            errorMessage = null;

            if (!IsConcreteType(serviceType))
            {
                errorMessage = StringResources.TypeShouldBeConcreteToBeUsedOnThisMethod(serviceType);
                return false;
            }

            if (!HasSinglePublicConstructor(serviceType))
            {
                errorMessage = StringResources.TypeMustHaveASinglePublicConstructor(serviceType);
                return false;
            }

            if (!HasConstructorWithOnlyValidParameters(serviceType))
            {
                var invalidParameter = Helpers.GetFirstInvalidConstructorParameter(serviceType);

                errorMessage =
                    StringResources.ConstructorMustNotContainInvalidParameter(serviceType, invalidParameter);

                return false;
            }

            return true;
        }

        private static IEnumerable<T> CreateImmutableCollection<T>(IEnumerable<T> collection)
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

        private static MethodInfo GetGenericMethod(Expression<Action<Container>> methodCall)
        {
            var body = methodCall.Body as MethodCallExpression;
            return body.Method.GetGenericMethodDefinition();
        }
     }
}