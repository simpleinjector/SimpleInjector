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

namespace SimpleInjector.Advanced
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// Defines the constructor resolution behavior that will be used by the container to select the
    /// constructor of any type that is auto-wired by the container. Inherit from this class to change the
    /// container's default constructor resolution behavior.
    /// </summary>
    /// <example>
    /// The following example shows how to register a custom constructor resolution behavior:
    /// <code lang="cs"><![CDATA[
    /// var options = new ContainerOptions();
    /// 
    /// options.ConstructorResolutionBehavior = new MyCustomConstructorResolutionBehavior();
    /// 
    /// var container = new Container(options);
    /// ]]></code>
    /// </example>
    public abstract class ConstructorResolutionBehavior
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConstructorResolutionBehavior"/> class.
        /// </summary>
        protected ConstructorResolutionBehavior()
        {
        }

        /// <summary>
        /// Gets the container this instance is applied to, or <b>null</b> when this instance is not part of
        /// any container (yet).
        /// </summary>
        /// <value>The container this instance is applied to, or null.</value>
        protected internal Container Container { get; internal set; }

        /// <summary>
        /// Gets a value indicating whether the container is currently in registration phase or if the 
        /// container is locked for any changes.
        /// </summary>
        /// <value><c>true</c> if this instance is registration phase; otherwise, <c>false</c>.</value>
        protected bool IsRegistrationPhase
        {
            get { return this.Container == null || !this.Container.IsLocked; }
        }

        /// <summary>
        /// Determines whether the given <paramref name="type"/> can be constructed by the container, by
        /// verifying it is not abstract, not an array, <see cref="GetConstructor"/> returns a value, and
        /// that constructor contains no value type and string arguments.
        /// </summary>
        /// <param name="type">The type to check.</param>
        /// <param name="errorMessage">The error message that explains why the type is not suitable.</param>
        /// <returns>
        ///   <c>true</c> if the <paramref name="type"/> is constructable; otherwise, <c>false</c>.
        /// </returns>
        [SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "1#",
            Justification = "I can't think of any better design.")]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly",
            MessageId = "Constructable", Justification = "I do think 'constructable' is a word.")]
        public bool IsConstructableType(Type type, out string errorMessage)
        {
            errorMessage = null;

            if (type.IsAbstract || type.IsArray)
            {
                // While array types are in fact concrete, we can not create them and creating them would be
                // pretty useless.
                errorMessage = StringResources.TypeShouldBeConcreteToBeUsedOnThisMethod(type);
                return false;
            }

            var constructor = this.GetConstructor(type);

            bool hasSuitableConstructor = constructor != null;

            if (!hasSuitableConstructor)
            {
                errorMessage = this.BuildErrorMessageForTypeWithoutSuitableConstructor(type);
                return false;
            }

            if (!ConstructorContainsOnlyValidParameters(constructor))
            {
                var invalidParameter = GetFirstInvalidConstructorParameter(constructor);

                errorMessage =
                    StringResources.ConstructorMustNotContainInvalidParameter(constructor, invalidParameter);

                return false;
            }

            return true;
        }

        /// <summary>
        /// Gets the constructor that should be used by the container for auto-wiring the type. Null will be
        /// returned when the type has no suitable constructor.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>The constructor that should be used by the container.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="type"/> is a null reference.
        /// </exception>
        public abstract ConstructorInfo GetConstructor(Type type);

        /// <summary>Builds the error message for the type without a suitable constructor.</summary>
        /// <remarks>Override this method when you've overridden <see cref="GetConstructor"/>.</remarks>
        /// <param name="type">The invalid type for which the error message should be built.</param>
        /// <returns>A message explaining the reason this type can not be created by the container.</returns>
        protected virtual string BuildErrorMessageForTypeWithoutSuitableConstructor(Type type)
        {
            return StringResources.TypeMustHaveASinglePublicConstructor(type);
        }

        private static bool ConstructorContainsOnlyValidParameters(ConstructorInfo constructor)
        {
            return GetFirstInvalidConstructorParameter(constructor) == null;
        }

        private static ParameterInfo GetFirstInvalidConstructorParameter(ConstructorInfo constructor)
        {
            return (
                from parameter in constructor.GetParameters()
                let type = parameter.ParameterType
                where type.IsValueType || type == typeof(string)
                select parameter)
                .FirstOrDefault();
        }
    }
}