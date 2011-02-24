namespace CuttingEdge.ServiceLocator.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;

    using CuttingEdge.ServiceLocation;

    /// <summary>
    /// Extension methods for registering open generic types.
    /// </summary>
    public static partial class SslOpenGenericBatchRegistrationExtensions
    {
        private static readonly MethodInfo registerMethodByFunc = GetMethod("Register", typeof(Func<>));

        public static void RegisterManyForOpenGeneric(this SimpleServiceLocator container, 
            Type openGenericServiceType, params Assembly[] assemblies)
        {
            RegisterManyForOpenGeneric(container, openGenericServiceType, (IEnumerable<Assembly>)assemblies);
        }

        public static void RegisterManyForOpenGeneric(this SimpleServiceLocator container,
            Type openGenericServiceType, IEnumerable<Assembly> assemblies)
        {
            RegisterManyForOpenGeneric(container, openGenericServiceType, false, assemblies);
        }

        public static void RegisterManyForOpenGeneric(this SimpleServiceLocator container,
            Type openGenericServiceType, bool includeInternalTypes, params Assembly[] assemblies)
        {
            RegisterManyForOpenGeneric(container, openGenericServiceType, includeInternalTypes,
                (IEnumerable<Assembly>)assemblies);
        }

        public static void RegisterManyForOpenGeneric(this SimpleServiceLocator container, 
            Type openGenericServiceType, bool includeInternalTypes, IEnumerable<Assembly> assemblies)
        {
            if (assemblies == null)
            {
                throw new ArgumentNullException("assemblies");
            }

            var typesToRegister =
                from assembly in assemblies
                from type in GetTypesFromAssembly(assembly, includeInternalTypes)
                where IsConcreteType(type)
                where TypeImplementsOpenGenericType(type, openGenericServiceType)
                select type;

            RegisterManyForOpenGeneric(container, openGenericServiceType, typesToRegister);
        }

        public static void RegisterManyForOpenGeneric(this SimpleServiceLocator container, 
            Type openGenericServiceType, params Type[] typesToRegister)
        {
            RegisterManyForOpenGeneric(container, openGenericServiceType, (IEnumerable<Type>)typesToRegister);
        }

        public static void RegisterManyForOpenGeneric(this SimpleServiceLocator container, 
            Type openGenericServiceType, IEnumerable<Type> typesToRegister)
        {
            // Make a copy of the collection for performance and correctness.
            typesToRegister = typesToRegister != null ? typesToRegister.ToArray() : null;

            if (typesToRegister == null)
            {
                throw new ArgumentNullException("typesToRegister");
            }

            if (container == null)
            {
                throw new ArgumentNullException("container");
            }

            if (openGenericServiceType == null)
            {
                throw new ArgumentNullException("openGenericServiceType");
            }

            if (typesToRegister.Contains(null))
            {
                throw new ArgumentException("The collection contains null elements.", "typesToRegister");
            }

            ThrowWhenTypeIsNotOpenGenericType(openGenericServiceType, "openGenericType");

            ThrowWhenTypesDontImplementServiceType(typesToRegister, openGenericServiceType);

            ThrowOnDuplicateRegistrations(typesToRegister, openGenericServiceType);

            RegisterOpenGenericInternal(container, openGenericServiceType, typesToRegister);
        }

        private static void RegisterOpenGenericInternal(this SimpleServiceLocator container,
            Type openGenericType, IEnumerable<Type> typesToRegister)
        {
            var registrations =
                from implementation in typesToRegister
                from service in GetServiceTypesForTypeBasedOnOpenGeneric(implementation, openGenericType)
                select new { ServiceType = service, Implementation = implementation };

            foreach (var registration in registrations)
            {
                RegisterInternal(container, registration.ServiceType, registration.Implementation);
            }
        }

        private static void RegisterInternal(SimpleServiceLocator container, Type serviceType, 
            Type implementationType)
        {
            Func<object> instanceCreator = () => container.GetInstance(implementationType);

            // Build the following expression: () => (T)instanceCreator();
            object creator = Expression.Lambda(
                Expression.Convert(
                    Expression.Invoke(Expression.Constant(instanceCreator), new Expression[0]),
                    serviceType),
                new ParameterExpression[0])
                .Compile();

            registerMethodByFunc.MakeGenericMethod(serviceType).Invoke(container, new[] { creator });
        }

        private static void ThrowWhenTypeIsNotOpenGenericType(Type openGenericType, string paramName)
        {
            if (!openGenericType.IsGenericTypeDefinition)
            {
                throw new ArgumentException(string.Format(CultureInfo.InvariantCulture,
                    "The supplied type '{0}' is not an open generic type.", openGenericType),
                    paramName);
            }
        }

        private static void ThrowWhenTypesDontImplementServiceType(IEnumerable<Type> types, Type openGeneric)
        {
            var invalidType = (
                from type in types
                where !TypeImplementsOpenGenericType(type, openGeneric)
                select type).FirstOrDefault();

            if (invalidType != null)
            {
                throw new ArgumentException(string.Format(CultureInfo.InvariantCulture,
                    "The supplied type '{0}' does not implement '{1}'.", invalidType, openGeneric),
                    "typesToRegister");
            }
        }

        private static void ThrowOnDuplicateRegistrations(IEnumerable<Type> typesToRegister, Type openGeneric)
        {
            var invalidTypes = (
                from type in typesToRegister
                from service in GetServiceTypesForTypeBasedOnOpenGeneric(type, openGeneric)
                group type by service into g
                where g.Count() > 1
                select new { ClosedType = g.Key, Types = g.ToArray() }).FirstOrDefault();

            if (invalidTypes != null)
            {
                var typeDescription = string.Join(", ", (
                    from type in invalidTypes.Types
                    select string.Format(CultureInfo.InvariantCulture, "'{0}'", type)).ToArray());

                throw new ArgumentException(string.Format(CultureInfo.InvariantCulture,
                    "There are {0} types that represent the closed generic type '{1}'. Types: {2}.",
                    invalidTypes.Types.Length, invalidTypes.ClosedType, typeDescription));
            }
        }

        private static bool IsConcreteType(Type type)
        {
            return !type.IsAbstract && !type.IsGenericTypeDefinition;
        }

        private static bool TypeImplementsOpenGenericType(Type type, Type openGenericType)
        {
            return GetServiceTypesForTypeBasedOnOpenGeneric(type, openGenericType).Any();
        }

        private static IEnumerable<Type> GetServiceTypesForTypeBasedOnOpenGeneric(Type type,
            Type openGenericType)
        {
            return
                from baseType in type.GetBaseTypesAndInterfaces()
                where @baseType.IsGenericType
                where (@baseType.IsGenericTypeDefinition && @baseType == openGenericType) ||
                    @baseType.GetGenericTypeDefinition() == openGenericType
                select baseType;
        }

        private static IEnumerable<Type> GetBaseTypesAndInterfaces(this Type type)
        {
            return type.GetInterfaces().Concat(type.GetBaseTypes());
        }

        private static IEnumerable<Type> GetBaseTypes(this Type type)
        {
            Type baseType = type.BaseType;

            while (baseType != null)
            {
                yield return baseType;

                baseType = baseType.BaseType;
            }
        }
        
        private static MethodInfo GetMethod(string name, Type parameterType)
        {
            return (
                from method in typeof(SimpleServiceLocator).GetMethods()
                where method.Name == name
                where method.GetParameters().Length > 0
                let parameter = method.GetParameters()[0].ParameterType
                where parameter.ContainsGenericParameters
                where parameter.IsGenericType
                where parameter.GetGenericTypeDefinition() == parameterType
                select method).Single();
        }

        private static IEnumerable<Type> GetTypesFromAssembly(Assembly assembly, bool includeInternalTypes)
        {
            if (includeInternalTypes)
            {
                return assembly.GetTypes();
            }
            else
            {
                return assembly.GetExportedTypes();
            }
        }
    }
}