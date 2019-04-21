#region Copyright Simple Injector Contributors
/* The Simple Injector is an easy-to-use Inversion of Control library for .NET
 * 
 * Copyright (c) 2019 Simple Injector Contributors
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
    using Microsoft.Extensions.DependencyInjection;
    using SimpleInjector.Integration.ServiceCollection;

    /// <summary>
    /// Provides programmatic configuration for the Simple Injector on top of <see cref="IServiceProvider"/>.
    /// </summary>
    public class SimpleInjectorUseOptions
    {
        internal SimpleInjectorUseOptions(
            SimpleInjectorAddOptions builder, IServiceProvider applicationServices)
        {
            this.Builder = builder;
            this.ApplicationServices = applicationServices;
        }

        /// <summary>
        /// Gets or sets a value indicating whether or not Simple Injector should try to load framework
        /// components from the framework's configuration system or not. The default is <c>true</c>.
        /// </summary>
        /// <value>A boolean value.</value>
        public bool AutoCrossWireFrameworkComponents { get; set; } = true;

        /// <summary>
        /// Gets the <see cref="IServiceProvider"/> that provides access to the framework's singleton
        /// services.
        /// </summary>
        /// <value>The <see cref="IServiceProvider"/> instance.</value>
        public IServiceProvider ApplicationServices { get; }

        /// <summary>
        /// Gets the application's Simple Injector <see cref="Container"/>.
        /// </summary>
        /// <value>The <see cref="Container"/> instance.</value>
        public Container Container => this.Builder.Container;

        /// <summary>
        /// Gets the <see cref="IServiceCollection"/> that contains the collection of framework components.
        /// </summary>
        /// <value>The <see cref="IServiceCollection"/> instance.</value>
        public IServiceCollection Services => this.Builder.Services;

        internal SimpleInjectorAddOptions Builder { get; }

        /// <inheritdoc />
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        public override string ToString() => base.ToString();

        /// <inheritdoc />
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        public override bool Equals(object obj) => base.Equals(obj);

        /// <summary>Gets the <see cref="System.Type"/> of the current instance.</summary>
        /// <returns>The <see cref="System.Type"/> instance that represents the exact runtime 
        /// type of the current instance.</returns>
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        public new Type GetType() => base.GetType();

        /// <inheritdoc />
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        public override int GetHashCode() => base.GetHashCode();
    }
}