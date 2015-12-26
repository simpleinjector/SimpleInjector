#region Copyright Simple Injector Contributors
/* The Simple Injector is an easy-to-use Inversion of Control library for .NET
 * 
 * Copyright (c) 2013 Simple Injector Contributors
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

namespace SimpleInjector.Integration.Web.Forms
{
    using System;
    using System.Reflection;
    using System.Web;
    using System.Web.UI;
    using SimpleInjector.Advanced;

    /// <summary>
    /// Defines the behavior for selecting properties for <see cref="UserControl"/>, <see cref="Page"/> types
    /// and <see cref="IHttpHandler"/> implementations for ASP.NET Web Forms integration scenarios.
    /// </summary>
    /// <example>
    /// The following example shows the usage of the <b>WebFormsPropertySelectionBehavior</b> class:
    /// <code lang="cs"><![CDATA[
    /// var container = new Container();
    /// 
    /// container.Options.PropertySelectionBehavior =
    ///     new WebFormsPropertySelectionBehavior(container.Options.PropertySelectionBehavior);
    /// ]]></code>
    /// </example>
    public sealed class WebFormsPropertySelectionBehavior : IPropertySelectionBehavior
    {
        private readonly IPropertySelectionBehavior baseBehavior;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="WebFormsPropertySelectionBehavior"/> class.
        /// </summary>
        /// <param name="baseBehavior">The original behavior that this instance wraps.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="baseBehavior"/> is a null
        /// reference (Nothing in VB).</exception>
        public WebFormsPropertySelectionBehavior(IPropertySelectionBehavior baseBehavior)
        {
            if (baseBehavior == null)
            {
                throw new ArgumentNullException(nameof(baseBehavior));
            }

            this.baseBehavior = baseBehavior;
        }

        bool IPropertySelectionBehavior.SelectProperty(Type serviceType, PropertyInfo propertyInfo)
        {
            return ShouldInjectProperty(propertyInfo) ||
                this.baseBehavior.SelectProperty(serviceType, propertyInfo);
        }

        private static bool ShouldInjectProperty(PropertyInfo property)
        {
            MethodInfo setMethod = property.GetSetMethod(nonPublic: true);

            return setMethod != null &&
                !setMethod.IsStatic &&
                !IsAmbiguousType(property.PropertyType) &&
                IsPropertyDeclaredOnCustomType(property);
        }

        private static bool IsAmbiguousType(Type propertyType)
        {
            return propertyType.IsValueType || propertyType == typeof(string);
        }

        private static bool IsPropertyDeclaredOnCustomType(PropertyInfo property)
        {
            return IsPropertyDeclaredOnCustomTypeOnCustomHttpHandler(property) || 
                IsPropertyDeclaredOnCustomTypeOnCustomUserControl(property);
        }

        private static bool IsPropertyDeclaredOnCustomTypeOnCustomHttpHandler(PropertyInfo property)
        {
            // The property must be declared on a type that implements IHttpHandler, but must not be 
            // declared on Page, TemplateControl or Control.
            // In other words it can be declared on sub types of Page, but also on custom IHttpHandler
            // implementations that don't inherit from page.
            return 
                typeof(IHttpHandler).IsAssignableFrom(property.DeclaringType) &&
                !property.DeclaringType.IsAssignableFrom(typeof(Page));
        }

        private static bool IsPropertyDeclaredOnCustomTypeOnCustomUserControl(PropertyInfo property)
        {
            return property.DeclaringType.IsSubclassOf(typeof(UserControl));
        }
    }
}