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
    using System.Collections.Generic;
    using System.Diagnostics;

    using SimpleInjector.Advanced;

    /// <summary>Configuration options for the <see cref="Container"/>.</summary>
    /// <example>
    /// The following example shows the typical usage of the <b>ContainerOptions</b> class.
    /// <code lang="cs"><![CDATA[
    /// var container = new Container();
    /// 
    /// container.Register<ITimeProvider, DefaultTimeProvider>();
    /// 
    /// // Use of ContainerOptions clas here.
    /// container.Options.AllowOverridingRegistrations = true;
    /// 
    /// // Replaces the previous registration of ITimeProvider
    /// container.Register<ITimeProvider, CustomTimeProvider>();
    /// ]]></code>
    /// </example>
    [DebuggerDisplay("{DebuggerDisplayDescription,nq}")]
    public class ContainerOptions
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private bool? enableDynamicAssemblyCompilation;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private IConstructorResolutionBehavior resolutionBehavior;
        
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private IConstructorVerificationBehavior verificationBehavior;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private IConstructorInjectionBehavior injectionBehavior;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private IPropertySelectionBehavior propertyBehavior;

        /// <summary>Initializes a new instance of the <see cref="ContainerOptions"/> class.</summary>
        public ContainerOptions()
        {
            this.resolutionBehavior = new DefaultConstructorResolutionBehavior();
            this.verificationBehavior = new DefaultConstructorVerificationBehavior();
            this.injectionBehavior = new DefaultConstructorInjectionBehavior(() => this.Container);
            this.propertyBehavior = new DefaultPropertySelectionBehavior();
        }

        /// <summary>
        /// Gets or sets a value indicating whether the container allows overriding registrations. The default
        /// is false.
        /// </summary>
        /// <value>The value indicating whether the container allows overriding registrations.</value>
        public bool AllowOverridingRegistrations { get; set; }

        /// <summary>Gets or sets the constructor resolution behavior.</summary>
        /// <value>The constructor resolution behavior.</value>
        /// <exception cref="NullReferenceException">Thrown when the supplied value is a null reference.</exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the container already contains registrations.
        /// </exception>
        public IConstructorResolutionBehavior ConstructorResolutionBehavior
        {
            get
            {
                return this.resolutionBehavior;
            }

            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }

                this.ThrowWhenContainerHasRegistrations("ConstructorResolutionBehavior");

                this.resolutionBehavior = value;
            }
        }

        /// <summary>Gets or sets the constructor resolution behavior.</summary>
        /// <value>The constructor resolution behavior.</value>
        /// <exception cref="NullReferenceException">Thrown when the supplied value is a null reference.</exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the container already contains registrations.
        /// </exception>
        public IConstructorVerificationBehavior ConstructorVerificationBehavior
        {
            get
            {
                return this.verificationBehavior;
            }

            set
            {
                Requires.IsNotNull(value, "value");

                this.ThrowWhenContainerHasRegistrations("ConstructorVerificationBehavior");

                this.verificationBehavior = value;
            }
        }

        /// <summary>Gets or sets the constructor injection behavior.</summary>
        /// <value>The constructor injection behavior.</value>
        /// <exception cref="NullReferenceException">Thrown when the supplied value is a null reference.</exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the container already contains registrations.
        /// </exception>
        public IConstructorInjectionBehavior ConstructorInjectionBehavior
        {
            get
            {
                return this.injectionBehavior;
            }

            set
            {
                Requires.IsNotNull(value, "value");

                this.ThrowWhenContainerHasRegistrations("ConstructorInjectionBehavior");

                this.injectionBehavior = value;
            }
        }

        /// <summary>Gets or sets the property selection behavior.</summary>
        /// <value>The property selection behavior.</value>
        /// <exception cref="NullReferenceException">Thrown when the supplied value is a null reference.</exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the container already contains registrations.
        /// </exception>
        public IPropertySelectionBehavior PropertySelectionBehavior
        {
            get
            {
                return this.propertyBehavior;
            }

            set
            {
                Requires.IsNotNull(value, "value");

                this.ThrowWhenContainerHasRegistrations("PropertySelectionBehavior");

                this.propertyBehavior = value;
            }
        }
        
        /// <summary>
        /// Gets the container to which this <b>ContainerOptions</b> instance belongs to or <b>null</b> when
        /// this instance hasn't been applied to a <see cref="Container"/> yet.
        /// </summary>
        /// <value>The current <see cref="Container"/>.</value>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public Container Container { get; internal set; }
        
        /// <summary>
        /// Gets or sets a value indicating whether the container will use dynamic assemblies for compilation. 
        /// By default, this value is <b>true</b> for the first few containers that are created in an app 
        /// domain and <b>false</b> for all other containers. You can set this value explicitly to <b>false</b>
        /// to prevent the use of dynamic assemblies or you can set this value explicitly to <b>true</b> to
        /// force more container instances to use dynamic assemblies. Note that creating an infinite number
        /// of <see cref="Container"/> instances (for instance one per web request) with this property set to
        /// <b>true</b> will result in a memory leak; dynamic assemblies take up memory and will only be
        /// unloaded when the app domain is unloaded.
        /// </summary>
        /// <value>A boolean indicating whether the container should use a dynamic assembly for compilation.
        /// </value>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        internal bool EnableDynamicAssemblyCompilation
        {
            get
            {
                if (this.enableDynamicAssemblyCompilation != null)
                {
                    return this.enableDynamicAssemblyCompilation.Value;
                }

                if (this.Container != null)
                {
                    return this.Container.ContainerId < 5;
                }

                return false;
            }

            set
            {
                this.enableDynamicAssemblyCompilation = value;
            }
        }

        internal string DebuggerDisplayDescription
        {
            get
            {
                var descriptions = new List<string>();

                if (this.AllowOverridingRegistrations)
                {
                    descriptions.Add("Allows Overriding Registrations");
                }

                if (!(this.ConstructorResolutionBehavior is DefaultConstructorResolutionBehavior))
                {
                    descriptions.Add("Custom Constructor Resolution");
                }

                if (!(this.ConstructorVerificationBehavior is DefaultConstructorVerificationBehavior))
                {
                    descriptions.Add("Custom Constructor Verification");
                }

                if (!(this.ConstructorInjectionBehavior is DefaultConstructorInjectionBehavior))
                {
                    descriptions.Add("Custom Constructor Injection");
                }

                if (!(this.PropertySelectionBehavior is DefaultPropertySelectionBehavior))
                {
                    descriptions.Add("Custom Property Selection");
                }

                if (descriptions.Count == 0)
                {
                    descriptions.Add("Default Configuration");
                }

                return string.Join(", ", descriptions);
            }
        }

        private void ThrowWhenContainerHasRegistrations(string propertyName)
        {
            if (this.Container != null && (this.Container.HasRegistrations || this.Container.IsLocked))
            {
                throw new InvalidOperationException(
                    StringResources.PropertyCanNotBeChangedAfterTheFirstRegistration(propertyName));
            }
        }
    }
}