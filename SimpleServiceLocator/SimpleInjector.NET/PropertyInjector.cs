#region Copyright (c) 2013 Simple Injector Contributors
/* The Simple Injector is an easy-to-use Inversion of Control library for .NET
 * 
 * Copyright (c) 2013 Simple Injector Contributors
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
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using SimpleInjector.Advanced;

    internal sealed class PropertyInjector
    {
        private readonly Container container;
        private readonly Lazy<Func<object, object>> injector;

        internal PropertyInjector(Container container, Type type)
        {
            this.container = container;
            this.Type = type;

            this.injector = new Lazy<Func<object, object>>(this.BuildDelegate);
        }

        internal Type Type { get; private set; }

        internal void Inject(object instance)
        {
            try
            {
                this.injector.Value(instance);
            }
            catch (TypeLoadException ex)
            {
                // This happens when the user tries to resolve an internal type inside a (Silverlight) sandbox.
                throw new ActivationException(
                    StringResources.UnableToInjectImplicitPropertiesDueToSecurityConfiguration(instance.GetType(), ex),
                    ex);
            }
        }

        private Func<object, object> BuildDelegate()
        {
            PropertyInfo[] propertiesToInject = this.GetInjectableProperties();

            var dummyExpression = Expression.Constant(null, this.Type);

            var propertyInjectionExpression = PropertyInjectionHelper.BuildPropertyInjectionExpression(
                this.container, this.Type, propertiesToInject, dummyExpression);

            var parameter = Expression.Parameter(typeof(object));

            propertyInjectionExpression = SubExpressionReplacer.Replace(
                expressionToAlter: propertyInjectionExpression,
                subExpressionToFind: dummyExpression,
                replacementExpression: Expression.Convert(parameter, this.Type));

            return Expression.Lambda<Func<object, object>>(propertyInjectionExpression, parameter).Compile();
        }

        private PropertyInfo[] GetInjectableProperties()
        {
            return (
                from property in this.Type.GetProperties()
                where property.CanWrite && property.GetSetMethod() != null
                where !property.PropertyType.IsValueType && !Helpers.IsAmbiguousType(property.PropertyType)
                where this.container.GetRegistration(property.PropertyType) != null
                select property)
                .ToArray();
        }
    }
}