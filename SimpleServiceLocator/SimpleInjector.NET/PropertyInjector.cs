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
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Reflection.Emit;

    internal sealed class PropertyInjector
    {
        private static readonly Type[] ActionDelegates = (
            from type in typeof(Action).Assembly.GetExportedTypes()
            where type.FullName.StartsWith("System.Action`", StringComparison.Ordinal)
            select type)
            .ToArray();

        private static readonly int RuntimeMaximumActionTypeArguments =
            ActionDelegates.Max(d => d.GetGenericArguments().Length);

        private readonly Container container;

        // This class builds a list of delegates where each delegate injects at most 3 properties (on .NET 3.5)
        // or at most 7 properties (on .NET 4.0 and up).
        private Action<object>[] injectors;

        internal PropertyInjector(Container container, Type type)
        {
            this.container = container;
            this.Type = type;
        }

        internal Type Type { get; set; }

        internal void Inject(object instance)
        {
            if (this.injectors == null)
            {
                this.CreateInjectorDelegate();
            }

            try
            {
                for (int index = 0; index < this.injectors.Length; index++)
                {
                    this.injectors[index](instance);
                }
            }
            catch (TypeLoadException ex)
            {
                // This happens when the user tries to resolve an internal type inside a (Silverlight) sandbox.
                throw new ActivationException(
                    StringResources.UnableToInjectPropertiesDueToSecurityConfiguration(instance.GetType(), ex),
                    ex);
            }
        }

        private void CreateInjectorDelegate()
        {
            lock (this)
            {
                if (this.injectors == null)
                {
                    this.injectors = this.BuildDelegates();
                }
            }
        }

        private Action<object>[] BuildDelegates()
        {
            var propertiesToInject = this.GetInjectableProperties();

            int maximumNumberOfPropertiesPerGroup = RuntimeMaximumActionTypeArguments - 1;
            
            var propertyGroups =
                from indexedProperty in propertiesToInject.Select((property, index) => new { property, index })
                let groupNumber = indexedProperty.index / maximumNumberOfPropertiesPerGroup
                group indexedProperty by groupNumber into g
                select g.OrderBy(i => i.index).Select(i => i.property).ToArray();

            return (
                from properties in propertyGroups
                let injectionDelegate = this.CompileInjectionDelegateForTypeAndProperties(properties)
                select this.CompileInjectionDelegateWithProducers(injectionDelegate, properties))
                .ToArray();
        }

        private PropertyInfo[] GetInjectableProperties()
        {
            return (
                from property in this.Type.GetProperties()
                where property.CanWrite
                where property.GetSetMethod() != null
                where !property.PropertyType.IsValueType
                let producer = this.container.GetRegistration(property.PropertyType)
                where producer != null
                select property)
                .ToArray();
        }

        private Delegate CompileInjectionDelegateForTypeAndProperties(PropertyInfo[] properties)
        {
            var propertyTypes = properties.Select(p => p.PropertyType).ToArray();

            var delegateType = this.GetCorrectDelegateForType(propertyTypes);

            var parameters = new List<Type> { this.Type };

            parameters.AddRange(propertyTypes);

            int numberOfEmptySlots = delegateType.GetGenericArguments().Length - properties.Length - 1;

            parameters.AddRange(Enumerable.Repeat(typeof(object), numberOfEmptySlots));

            var method = new DynamicMethod(this.Type.Name + "_Injector", typeof(void), parameters.ToArray());

            EmitMethodBody(method, properties);

            return method.CreateDelegate(delegateType);
        }

        private static void EmitMethodBody(DynamicMethod dynamicMethod, PropertyInfo[] properties)
        {
            ILGenerator generator = dynamicMethod.GetILGenerator();

            for (int index = 0; index < properties.Length; index++)
            {
                EmitInjectionOfProperty(generator, properties[index], index);
            }

            generator.Emit(OpCodes.Ret);
        }

        private static void EmitInjectionOfProperty(ILGenerator generator, PropertyInfo property, int index)
        {
            var local = generator.DeclareLocal(property.PropertyType);

            generator.Emit(OpCodes.Ldarg_0);

            switch (index)
            {
                case 0:
                    generator.Emit(OpCodes.Ldarg_1);
                    break;

                case 1:
                    generator.Emit(OpCodes.Ldarg_2);
                    break;

                case 2:
                    generator.Emit(OpCodes.Ldarg_3);
                    break;

                default:
                    generator.Emit(OpCodes.Ldarg_S, local);
                    break;
            }

            generator.Emit(OpCodes.Callvirt, property.GetSetMethod());
        }

        private Type GetCorrectDelegateForType(Type[] propertyTypes)
        {
            var delegateType = GetActionDelegateWithGenericArgumentCount(propertyTypes.Length + 1);

            var typeArguments = new List<Type> { this.Type };
            typeArguments.AddRange(propertyTypes);

            int numberOfEmptySlots = delegateType.GetGenericArguments().Length - propertyTypes.Length - 1;

            typeArguments.AddRange(Enumerable.Repeat(typeof(object), numberOfEmptySlots));

            return delegateType.MakeGenericType(typeArguments.ToArray());
        }

        private static Type GetActionDelegateWithGenericArgumentCount(int minimumNumberOfGenericArguments)
        {
            // Because there can be gaps in the number of arguments (Silverlight 3 for instance, 
            // seems to miss Action<T1, T2>) we need to the first delegate with at least that number of args.
            return (
                from actionDelegate in ActionDelegates
                let numberOfGenericArguments = actionDelegate.GetGenericArguments().Length
                where numberOfGenericArguments >= minimumNumberOfGenericArguments
                orderby numberOfGenericArguments
                select actionDelegate)
                .First();
        }

        private Action<object> CompileInjectionDelegateWithProducers(Delegate injectionDelegate,
            PropertyInfo[] properties)
        {
            IEnumerable<IInstanceProducer> producers =
                from property in properties
                select this.container.GetRegistration(property.PropertyType);

            ParameterExpression instanceParameter = Expression.Parameter(typeof(object), "instance");

            var arguments = new List<Expression> { Expression.Convert(instanceParameter, this.Type) };

            // We build up the delegate using the instance producers BuildExpression method, making the
            // delegate as efficient as technically possible.
            arguments.AddRange(producers.Select(producer => producer.BuildExpression()));

            int numberOfEmptySlots = 
                injectionDelegate.GetType().GetGenericArguments().Length - properties.Length - 1;

            var emptySlots =
                Enumerable.Repeat<Expression>(Expression.Constant(null, typeof(object)), numberOfEmptySlots);

            arguments.AddRange(emptySlots);

            // Create the following expression: instance => injectionDelegate(instance, {prop1}, {prop2}, ...)
            var injectorExpression = Expression.Lambda<Action<object>>(
                Expression.Invoke(Expression.Constant(injectionDelegate), arguments.ToArray()),
                new ParameterExpression[] { instanceParameter });

            return injectorExpression.Compile();
        }
    }
}