namespace SimpleInjector.Advanced.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Reflection.Emit;

    // This builds a type that inherits from IndexableEnumerable<T> and implements IReadOnlyList<T>.
    // IReadOnlyList<T> is only available in .NET 4.5 but Simple Injector needs to stay compatible with
    // .NET 4.0.
    // This code was generated once (and manually altered) with the Reflector ReflectionEmitLanguage plugin:
    // https://reflectoraddins.codeplex.com/wikipage?title=ReflectionEmitLanguage.
    internal sealed class ReadOnlyContainerControlledCollectionTypeBuilder
    {
        private ModuleBuilder module;
        private TypeBuilder typeBuilder;
        private GenericTypeParameterBuilder genericArgument;
        private FieldInfo listField;
        private FieldInfo collectionField;

        public static Type Build()
        {
            return new ReadOnlyContainerControlledCollectionTypeBuilder().BuildInternal();
        }

        private Type BuildInternal()
        {
            this.module = AppDomain.CurrentDomain.DefineDynamicAssembly(
                new AssemblyName("SimpleInjector.NET45"), AssemblyBuilderAccess.Run)
                .DefineDynamicModule("SimpleInjector.CompiledModule");

            this.typeBuilder = this.BuildReadOnlyContainerControlledCollection();

            this.listField = this.BuildListField();
            this.collectionField = this.BuildCollectionField();

            this.BuildConstructor();
            this.BuildMethodAppend();
            this.BuildMethodGetEnumerator();
            this.BuildMethodGetRelationships();
            this.BuildMethodCount();
            this.BuildMethodSetItem();
            this.BuildMethodGetItem();

            return this.typeBuilder.CreateType();
        }

        private TypeBuilder BuildReadOnlyContainerControlledCollection()
        {
            TypeBuilder typeBuilder = this.module.DefineType("ReadOnlyContainerControlledCollection`1",
                TypeAttributes.Public | TypeAttributes.Sealed);

            this.genericArgument = typeBuilder.DefineGenericParameters("TService").Single();

            typeBuilder.SetParent(typeof(IndexableEnumerable<>).MakeGenericType(this.genericArgument));
            typeBuilder.AddInterfaceImplementation(Helpers.IReadOnlyListType.MakeGenericType(this.genericArgument));
            typeBuilder.AddInterfaceImplementation(Helpers.IReadOnlyCollectionType.MakeGenericType(this.genericArgument));
            typeBuilder.AddInterfaceImplementation(typeof(IContainerControlledCollection));

            return typeBuilder;
        }

        private FieldBuilder BuildListField()
        {
            return this.typeBuilder.DefineField("list", typeof(IList<>).MakeGenericType(this.genericArgument),
                FieldAttributes.Private);
        }

        private FieldBuilder BuildCollectionField()
        {
            return this.typeBuilder.DefineField("collection", typeof(IContainerControlledCollection),
                FieldAttributes.Private);
        }

        private MethodBuilder BuildConstructor()
        {
            MethodAttributes methodAttributes = MethodAttributes.Public | MethodAttributes.HideBySig;

            MethodBuilder method = this.typeBuilder.DefineMethod(".ctor", methodAttributes);

            var ctor = typeof(IndexableEnumerable<>).GetConstructor(
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                null, Type.EmptyTypes, null);

            method.SetReturnType(typeof(void));

            method.SetParameters(typeof(IContainerControlledCollection));

            ParameterBuilder collection = method.DefineParameter(1, ParameterAttributes.None, "collection");

            ILGenerator gen = method.GetILGenerator();

            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Call, ctor);
            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Ldarg_1);
            gen.Emit(OpCodes.Castclass, typeof(IList<>).MakeGenericType(this.genericArgument));
            gen.Emit(OpCodes.Stfld, this.listField);
            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Ldarg_1);
            gen.Emit(OpCodes.Stfld, this.collectionField);
            gen.Emit(OpCodes.Ret);

            return method;
        }

        private MethodBuilder BuildMethodAppend()
        {
            MethodAttributes methodAttributes = MethodAttributes.Public | MethodAttributes.Virtual |
                MethodAttributes.Final | MethodAttributes.HideBySig | MethodAttributes.NewSlot;

            MethodBuilder method = this.typeBuilder.DefineMethod("Append", methodAttributes);

            MethodInfo interfaceAppendMethod = typeof(IContainerControlledCollection).GetMethod("Append",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                null, new Type[] { typeof(Registration) }, null);

            method.SetReturnType(typeof(void));

            method.SetParameters(typeof(Registration));

            method.DefineParameter(1, ParameterAttributes.None, "registration");

            ILGenerator gen = method.GetILGenerator();

            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Ldfld, this.collectionField);
            gen.Emit(OpCodes.Ldarg_1);
            gen.Emit(OpCodes.Callvirt, interfaceAppendMethod);
            gen.Emit(OpCodes.Ret);

            return method;
        }

        private MethodBuilder BuildMethodGetEnumerator()
        {
            MethodAttributes methodAttributes =
                MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.HideBySig;

            MethodBuilder method = this.typeBuilder.DefineMethod("GetEnumerator", methodAttributes);

            var baseEnumeratorMethod =
                typeof(IEnumerable<>).GetMethod("GetEnumerator",
                BindingFlags.Instance | BindingFlags.Public, null, new Type[] { }, null);

            method.SetReturnType(typeof(IEnumerator<>).MakeGenericType(this.genericArgument));

            ILGenerator gen = method.GetILGenerator();

            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Ldfld, this.listField);
            gen.Emit(OpCodes.Callvirt, baseEnumeratorMethod);
            gen.Emit(OpCodes.Ret);

            return method;
        }

        private MethodBuilder BuildMethodGetRelationships()
        {
            MethodAttributes methodAttributes =
                MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.Final |
                MethodAttributes.HideBySig | MethodAttributes.NewSlot;

            MethodBuilder method = this.typeBuilder.DefineMethod("GetRelationships", methodAttributes);

            MethodInfo interfaceGetRelationshipsMethod =
                typeof(IContainerControlledCollection).GetMethod("GetRelationships",
                BindingFlags.Instance | BindingFlags.Public);

            method.SetReturnType(typeof(KnownRelationship[]));

            ILGenerator gen = method.GetILGenerator();

            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Ldfld, this.collectionField);
            gen.Emit(OpCodes.Callvirt, interfaceGetRelationshipsMethod);
            gen.Emit(OpCodes.Ret);

            return method;
        }

        private MethodBuilder BuildMethodCount()
        {
            MethodAttributes methodAttributes = MethodAttributes.Public | MethodAttributes.Virtual |
                MethodAttributes.HideBySig;

            MethodBuilder method = this.typeBuilder.DefineMethod("get_Count", methodAttributes);

            MethodInfo interfaceCountMethod =
                typeof(ICollection<>).GetMethod("get_Count", BindingFlags.Instance | BindingFlags.Public);

            method.SetReturnType(typeof(int));

            ILGenerator gen = method.GetILGenerator();

            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Ldfld, this.listField);
            gen.Emit(OpCodes.Callvirt, interfaceCountMethod);
            gen.Emit(OpCodes.Ret);

            return method;
        }

        private MethodBuilder BuildMethodSetItem()
        {
            MethodAttributes methodAttributes = MethodAttributes.Public | MethodAttributes.Virtual |
                MethodAttributes.HideBySig;

            MethodBuilder method = this.typeBuilder.DefineMethod("set_Item", methodAttributes);

            MethodInfo interfaceSetItemMethod =
                typeof(IList<>).GetMethod("set_Item", BindingFlags.Instance | BindingFlags.Public);

            method.SetReturnType(typeof(void));

            method.SetParameters(typeof(int), this.genericArgument);

            method.DefineParameter(1, ParameterAttributes.None, "index");
            method.DefineParameter(2, ParameterAttributes.None, "value");

            ILGenerator gen = method.GetILGenerator();

            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Ldfld, this.listField);
            gen.Emit(OpCodes.Ldarg_1);
            gen.Emit(OpCodes.Ldarg_2);
            gen.Emit(OpCodes.Callvirt, interfaceSetItemMethod);
            gen.Emit(OpCodes.Ret);

            return method;
        }

        private MethodBuilder BuildMethodGetItem()
        {
            MethodAttributes methodAttributes = MethodAttributes.Public | MethodAttributes.Virtual
                | MethodAttributes.HideBySig;

            MethodBuilder method = this.typeBuilder.DefineMethod("get_Item", methodAttributes);

            MethodInfo interfaceGetItemMethod = typeof(IList<>).GetMethod(
                "get_Item", BindingFlags.Instance | BindingFlags.Public);

            method.SetReturnType(this.genericArgument);

            method.SetParameters(typeof(int));

            ParameterBuilder index = method.DefineParameter(1, ParameterAttributes.None, "index");
            ILGenerator gen = method.GetILGenerator();

            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Ldfld, this.listField);
            gen.Emit(OpCodes.Ldarg_1);
            gen.Emit(OpCodes.Callvirt, interfaceGetItemMethod);
            gen.Emit(OpCodes.Ret);

            return method;
        }
    }
}
