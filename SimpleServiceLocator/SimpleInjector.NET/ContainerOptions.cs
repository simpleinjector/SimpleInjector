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
    using SimpleInjector.Advanced;

    /// <summary>Configuration options for the <see cref="Container"/>.</summary>
    /// <example>
    /// The following example shows the typical usage of the <b>ContainerOptions</b> class.
    /// <code lang="cs"><![CDATA[
    /// var container = new Container(new ContainerOptions { AllowOverridingRegistrations = true });
    /// 
    /// container.Register<ITimeProvider, DefaultTimeProvider>();
    /// 
    /// // Replaces the previous registration of ITimeProvider
    /// container.Register<ITimeProvider, CustomTimeProvider>();
    /// ]]></code>
    /// Instead of applying the created <b>ContainerOptions</b> directly to the container's constructor, the
    /// options class can be stored in a local variable. This allows changing the behavior of the container
    /// during the initialization process.
    /// <code lang="cs"><![CDATA[
    /// var options = new ContainerOptions { AllowOverridingRegistrations = false };
    /// 
    /// var container = new Container(options);
    /// 
    /// BusinessLayerBootstrapper.Bootstrap(container);
    /// 
    /// options.AllowOverridingRegistrations = true;
    /// 
    /// // Replaces a possibly former registration of ITimeProvider
    /// container.Register<ITimeProvider, CustomTimeProvider>();
    /// ]]></code>
    /// </example>
    public class ContainerOptions
    {
        private ConstructorResolutionBehavior constructorResolutionBehavior =
            new DefaultConstructorResolutionBehavior();

        /// <summary>
        /// Gets or sets a value indicating whether the container allows overriding registrations. The default
        /// is false.
        /// </summary>
        /// <value>The value indicating whether the container allows overriding registrations.</value>
        public bool AllowOverridingRegistrations { get; set; }

        /// <summary>Gets or sets the constructor resolution behavior.</summary>
        /// <value>The constructor resolution behavior.</value>
        public ConstructorResolutionBehavior ConstructorResolutionBehavior
        {
            get
            {
                return this.constructorResolutionBehavior;
            }

            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }

                this.ThrowWhenContainerHasRegistrations();

                if (value.Container != null && !object.ReferenceEquals(value.Container, this.Container))
                {
                    throw new ArgumentException(
                        StringResources.ConstructorResolutionBehaviorBelongsToAnotherContainer(), "value");
                }

                this.constructorResolutionBehavior = value;
                this.constructorResolutionBehavior.Container = this.Container;
            }
        }

        internal Container Container { get; set; }

        private void ThrowWhenContainerHasRegistrations()
        {
            if (this.Container == null)
            {
                return;
            }

            if (this.Container.HasRegistrations || this.Container.IsLocked)
            {
                throw new InvalidOperationException(
                    StringResources.ConstructorResolutionBehaviorCanNotBeChangedAfterTheFirstRegistration());
            }
        }
    }
}