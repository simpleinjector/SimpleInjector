#region Copyright Simple Injector Contributors
/* The Simple Injector is an easy-to-use Inversion of Control library for .NET
 * 
 * Copyright (c) 2015 Simple Injector Contributors
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
    using System.Diagnostics;
    using System.Globalization;
    using System.Reflection;

    /// <summary>
    /// Discovers the attributes of the code element (a property or parameter) where a dependency will be
    /// injected into, and provides access to its meta data.
    /// </summary>
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public sealed class InjectionTargetInfo
#if !PCL
 : ICustomAttributeProvider
#endif
    {
        internal InjectionTargetInfo(ParameterInfo parameter)
        {
            this.Parameter = parameter;
        }

        internal InjectionTargetInfo(PropertyInfo property)
        {
            this.Property = property;
        }

        /// <summary>Gets the constructor argument of the consumer of the component where the dependency will be
        /// injected into. The property can return null.</summary>
        /// <value>The <see cref="ParameterInfo"/> or null when the dependency is injected into a property.</value>
        public ParameterInfo Parameter { get; private set; }

        /// <summary>Gets the property of the consumer of the component where the dependency will be injected into. 
        /// The property can return null.</summary>
        /// <value>The <see cref="PropertyInfo"/> or null when the dependency is injected into a constructor
        /// argument instead.</value>
        public PropertyInfo Property { get; private set; }

        /// <summary>Gets the name of the target.</summary>
        /// <value>A string containing the name of the target.</value>
        public string Name
        {
            get { return this.Parameter != null ? this.Parameter.Name : this.Property.Name; }
        }

        /// <summary>Gets the type of the target.</summary>
        /// <value>A <see cref="Type"/> containing the type of the target.</value>
        public Type TargetType
        {
            get { return this.Parameter != null ? this.Parameter.ParameterType : this.Property.PropertyType; }
        }

        /// <summary>Gets the member of the target. This is either the constructor of the parameter, or in
        /// case the target is a property, the property itself will be returned.</summary>
        /// <value>A <see cref="TargetType"/> containing the type of the target.</value>
        internal MemberInfo Member
        {
            get { return this.Parameter != null ? this.Parameter.Member : this.Property; }
        }

        internal string DebuggerDisplay
        {
            get
            {
                return string.Format(
                    CultureInfo.InvariantCulture,
                    "{0} {{ Name = \"{1}\", Type = {2} }}",
                    this.Parameter != null ? "Parameter" : "Property",
                    this.Name,
                    this.TargetType.ToFriendlyName());
            }
        }

        /// <summary>
        /// Returns an array of all of the custom attributes defined on either the <see cref="Parameter"/> or
        /// the <see cref="Property"/>, excluding named attributes, or an empty array if there are no custom 
        /// attributes.
        /// </summary>
        /// <param name="inherit">When true, look up the hierarchy chain for the inherited custom attribute.</param>
        /// <returns>An array of Objects representing custom attributes, or an empty array.</returns>
        /// <exception cref="TypeLoadException">The custom attribute type cannot be loaded.</exception>
        /// <exception cref="AmbiguousMatchException">There is more than one attribute of type attributeType 
        /// defined on this member.</exception>
        public object[] GetCustomAttributes(bool inherit)
        {
            return this.Parameter != null
                ? this.Parameter.GetCustomAttributes(inherit)
                : this.Property.GetCustomAttributes(inherit);
        }

        /// <summary>
        /// Returns an array of custom attributes defined on either the <see cref="Parameter"/> or
        /// the <see cref="Property"/>, identified by type, or an empty array if there are no custom 
        /// attributes of that type.
        /// </summary>
        /// <param name="attributeType">The type of the custom attributes.</param>
        /// <param name="inherit">When true, look up the hierarchy chain for the inherited custom attribute.</param>
        /// <returns>An array of Objects representing custom attributes, or an empty array.</returns>
        /// <exception cref="TypeLoadException">The custom attribute type cannot be loaded.</exception>
        /// <exception cref="ArgumentNullException">attributeType is null.</exception>
        public object[] GetCustomAttributes(Type attributeType, bool inherit)
        {
            return this.Parameter != null
                ? this.Parameter.GetCustomAttributes(attributeType, inherit)
                : this.Property.GetCustomAttributes(attributeType, inherit);
        }

        /// <summary>
        /// Indicates whether one or more instance of attributeType is defined on this either the 
        /// <see cref="Parameter"/> or the <see cref="Property"/>.
        /// </summary>
        /// <param name="attributeType">The type of the custom attributes.</param>
        /// <param name="inherit">When true, look up the hierarchy chain for the inherited custom attribute.</param>
        /// <returns>true if the attributeType is defined on this member; false otherwise.</returns>
        public bool IsDefined(Type attributeType, bool inherit)
        {
            return this.Parameter != null
                ? this.Parameter.IsDefined(attributeType, inherit)
                : this.Property.IsDefined(attributeType, inherit);
        }
    }
}