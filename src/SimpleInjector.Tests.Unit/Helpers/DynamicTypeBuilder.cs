namespace SimpleInjector.Core.Tests.Unit.Helpers
{
    using System;
    using System.Linq;
    using System.Reflection;
    using System.Reflection.Emit;

    public static class DynamicTypeBuilder
    {
        public static Type BuildType(string typeName, params string[] genericTypeArguments)
        {
            var moduleBuilder = GetModuleBuilder(typeName);

            TypeBuilder typeBuilder = moduleBuilder.DefineType(typeName,
                TypeAttributes.Public |
                TypeAttributes.Class |
                TypeAttributes.AutoClass |
                TypeAttributes.AnsiClass |
                TypeAttributes.BeforeFieldInit |
                TypeAttributes.AutoLayout,
                null);

            typeBuilder.DefineDefaultConstructor(
                MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName);

            if (genericTypeArguments.Any())
            {
                typeBuilder.DefineGenericParameters(genericTypeArguments);
            }

            return typeBuilder.CreateType();
        }

        private static ModuleBuilder GetModuleBuilder(string typeName)
        {
            var assemblyName = new AssemblyName(typeName + "_dll");
            AssemblyBuilder assemblyBuilder =
                AppDomain.CurrentDomain.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
            return assemblyBuilder.DefineDynamicModule("MainModule" + Guid.NewGuid().ToString());
        }
    }
}
