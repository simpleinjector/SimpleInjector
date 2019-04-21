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

namespace SimpleInjector.Integration.ServiceCollection
{
    using System;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// Provides programmatic configuration for the Simple Injector on top of <see cref="IServiceCollection"/>.
    /// </summary>
    public sealed class SimpleInjectorAddOptions
    {
        private IServiceProviderAccessor serviceProviderAccessor;

        internal SimpleInjectorAddOptions(
            IServiceCollection services, Container container, IServiceProviderAccessor accessor)
        {
            this.Services = services;
            this.Container = container;
            this.ServiceProviderAccessor = accessor;
        }

        /// <summary>
        /// Gets the <see cref="IServiceCollection"/> that contains the collection of framework components.
        /// </summary>
        /// <value>The <see cref="IServiceCollection"/> instance.</value>
        public IServiceCollection Services { get; }

        /// <summary>
        /// Gets the <see cref="Container"/> instance used by the application.
        /// </summary>
        /// <value>The <see cref="Container"/> instance.</value>
        public Container Container { get; }

        /// <summary>
        /// Gets or sets an <see cref="IServiceProviderAccessor"/> instance that will be used by Simple
        /// Injector to resolve cross-wired framework components.
        /// </summary>
        /// <value>The <see cref="IServiceProviderAccessor"/> instance.</value>
        /// <exception cref="ArgumentNullException">Thrown when a null value is provided.</exception>
        public IServiceProviderAccessor ServiceProviderAccessor
        {
            get
            {
                return this.serviceProviderAccessor;
            }

            set
            {
                if (value is null)
                {
                    throw new ArgumentNullException(nameof(value));
                }

                this.serviceProviderAccessor = value;
            }
        }

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