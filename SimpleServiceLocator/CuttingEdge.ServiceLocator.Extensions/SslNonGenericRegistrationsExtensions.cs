using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

using CuttingEdge.ServiceLocation;

namespace CuttingEdge.ServiceLocator.Extensions
{
    /// <summary>
    /// Extension methods with non-generic method overloads.
    /// </summary>
    public static class SslNonGenericRegistrationsExtensions
    {
        private const object Obj = null;
        private const Func<object> Func = null;
        private const IEnumerable<object> Collection = null;

        private static readonly MethodInfo getInstance = Method(c => c.GetInstance<object>());
        private static readonly MethodInfo register = Method(c => c.Register<object>(Func));
        private static readonly MethodInfo registerAll = Method(c => c.RegisterAll<object>(Collection));
        private static readonly MethodInfo registerSingleByFunc = Method(c => c.RegisterSingle<object>(Func));      
        private static readonly MethodInfo registerSingleByT = Method(c => c.RegisterSingle<object>(Obj));

        public static void RegisterSingle(this SimpleServiceLocator container, Type serviceType,
            Type implementation)
        {
            if (serviceType == null)
            {
                throw new ArgumentNullException("serviceType");
            }

            if (implementation == null)
            {
                throw new ArgumentNullException("implementation");
            }

            if (serviceType == implementation)
            {
                throw new ArgumentException("serviceType and implementation must be different.", "serviceType");
            }

            if (!serviceType.IsAssignableFrom(implementation))
            {
                throw new ArgumentException(implementation.ToString() + " is not an implementation of " +
                    serviceType.ToString() + ".", "implementation");
            }

            // Build the following expression: () => container.GetInstance<Implementation>();
            Delegate instanceCreator = Expression.Lambda(
                Expression.Convert(
                    Expression.Call(
                        Expression.Constant(container), 
                        getInstance.MakeGenericMethod(implementation)),
                        serviceType),
                new ParameterExpression[0])
                .Compile();
            
            registerSingleByFunc.MakeGenericMethod(serviceType).Invoke(container, new[] { instanceCreator });
        }

        public static void RegisterSingle(this SimpleServiceLocator container, Type serviceType,
            Func<object> instanceCreator)
        {
            if (container == null)
            {
                throw new ArgumentNullException("container");
            }

            if (serviceType == null)
            {
                throw new ArgumentNullException("serviceType");
            }

            if (instanceCreator == null)
            {
                throw new ArgumentNullException("instanceCreator");
            }

            // Build the following delegate: () => (ServiceType)instanceCreator();
            object creator = Expression.Lambda(
                Expression.Convert(
                    Expression.Invoke(Expression.Constant(instanceCreator), new Expression[0]),
                    serviceType),
                new ParameterExpression[0])
                .Compile();

            registerSingleByFunc.MakeGenericMethod(serviceType).Invoke(container, new[] { creator });
        }

        public static void RegisterSingle(this SimpleServiceLocator container, Type serviceType,
            object instance)
        {
            if (container == null)
            {
                throw new ArgumentNullException("container");
            }

            if (serviceType == null)
            {
                throw new ArgumentNullException("serviceType");
            }

            if (instance == null)
            {
                throw new ArgumentNullException("instance");
            }

            if (!serviceType.IsAssignableFrom(instance.GetType()))
            {
                throw new ArgumentException(instance.GetType() + " is not an implementation of " +
                    serviceType.ToString() + ".", "instance");
            }
            
            registerSingleByT.MakeGenericMethod(serviceType).Invoke(container, new[] { instance });
        }

        public static void RegisterAll(this SimpleServiceLocator container, Type serviceType,
            IEnumerable collection)
        {
            if (container == null)
            {
                throw new ArgumentNullException("container");
            }

            var castMethod = typeof(Enumerable).GetMethod("Cast").MakeGenericMethod(serviceType);

            object castedCollection = castMethod.Invoke(null, new[] { collection });

            registerAll.MakeGenericMethod(serviceType).Invoke(container, new[] { castedCollection });
        }

        public static void Register(this SimpleServiceLocator container, Type serviceType, Type implementation)
        {
            if (implementation == null)
            {
                throw new ArgumentNullException("implementation");
            }

            if (serviceType == implementation)
            {
                throw new ArgumentException("serviceType and implementation must be different.", "serviceType");
            }

            Func<object> instanceCreator = () => container.GetInstance(implementation);

            Register(container, serviceType, instanceCreator);
        }

        public static void Register(this SimpleServiceLocator container, Type serviceType,
            Func<object> instanceCreator)
        {
            if (container == null)
            {
                throw new ArgumentNullException("container");
            }

            if (serviceType == null)
            {
                throw new ArgumentNullException("serviceType");
            }

            if (instanceCreator == null)
            {
                throw new ArgumentNullException("instanceCreator");
            }

            // Build the following delegate: () => (T)instanceCreator();
            object creator = Expression.Lambda(
                Expression.Convert(
                    Expression.Invoke(Expression.Constant(instanceCreator), new Expression[0]),
                    serviceType),
                new ParameterExpression[0])
                .Compile();

            register.MakeGenericMethod(serviceType).Invoke(container, new[] { creator });
        }

        private static MethodInfo Method(Expression<Action<SimpleServiceLocator>> methodCall)
        {
            var body = methodCall.Body as MethodCallExpression;

            return body.Method.GetGenericMethodDefinition();
        }
    }
}