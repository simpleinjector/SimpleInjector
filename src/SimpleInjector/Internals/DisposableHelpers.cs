// Copyright (c) Simple Injector Contributors. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace SimpleInjector.Internals
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;

    internal static class DisposableHelpers
    {
        // Only allows diposing of IAsyncDisposable; not for IDisposable
        private static Func<object, Task>? asyncDisposer;

#if NETSTANDARD2_1
        private static Type? AsyncDisposableInterface = typeof(IAsyncDisposable);
#else
        private static Type? AsyncDisposableInterface;
#endif

        internal static bool AsyncDisposableInterfaceFound => AsyncDisposableInterface != null;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool IsAsyncOrAsyncDisposable(object instance) =>
            instance is IDisposable || IsAsyncDisposable(instance);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool IsAsyncDisposable(object instance) =>
#if NETSTANDARD2_1
            instance is IAsyncDisposable;
#else
            IsAsyncDisposableType(instance.GetType());
#endif

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool IsSyncOrAsyncDisposableType(Type type) =>
            typeof(IDisposable).IsAssignableFrom(type) || IsAsyncDisposableType(type);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool IsAsyncDisposableType(Type type) =>
            AsyncDisposableInterface != null
                ? AsyncDisposableInterface.IsAssignableFrom(type)
                : InitializeAsyncDisposableInterface(type);

        // instance must be IAsyncDisposable; not IDisposable.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static Task DisposeAsync(object asyncDisposableInstance)
        {
            if (asyncDisposer is null)
            {
                InitializeAsyncDisposableInterface(asyncDisposableInstance.GetType());
                asyncDisposer = CreateDisposer(AsyncDisposableInterface!);
            }

            return asyncDisposer(asyncDisposableInstance);
        }

        private static Func<object, Task> CreateDisposer(Type asyncDisposableInterface)
        {
            var DisposeAsync = asyncDisposableInterface.GetMethod("DisposeAsync");

            var param = Expression.Parameter(typeof(object), "disposable");

            try
            {
                // ((IAsyncDisposable)instance).DisposeAsync();
                var expression =
                    Expression.Call(
                        instance: Expression.Convert(param, AsyncDisposableInterface),
                        method: DisposeAsync);

                // We also allow for situations where the IAsyncDisposable.DisposeAsync() method returns a Task.
                // This is useful for situations where runs < .NET 4.6.1 (where there is no IAsyncDisposable
                // support). This allows users to define their own System.IAsyncDisposable interface with a method
                // that returns Task.
                if (DisposeAsync.ReturnType != typeof(Task))
                {
                    // ((IAsyncDisposable)instance).DisposeAsync().AsTask();
                    expression = Expression.Call(expression, DisposeAsync.ReturnType.GetMethod("AsTask"));
                }

                return Expression.Lambda<Func<object, Task>>(expression, param).Compile();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"There was an error using {asyncDisposableInterface.AssemblyQualifiedName} as " +
                    $"interface for applying asynchronous disposal. Make sure the interface contains a " +
                    $"'DisposeAsync()' method that returns {typeof(Task)}. " + ex.Message, ex);
            }
        }

        private static bool InitializeAsyncDisposableInterface(Type type)
        {
            IEnumerable<Type> implementedInterfaces = type.GetTypeInfo().ImplementedInterfaces;
            var interfaces = implementedInterfaces as Type[] ?? implementedInterfaces.ToArray();

            foreach (Type iface in interfaces)
            {
                if (iface.FullName == "System.IAsyncDisposable")
                {
                    AsyncDisposableInterface = iface;
                    return true;
                }
            }

            return false;
        }
    }
}