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

namespace SimpleInjector.Integration.GenericHost
{
    using System;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using SimpleInjector.Integration.ServiceCollection;

    /// <summary>
    /// Extension methods for integrating Simple Injector with Generic Hosts.
    /// </summary>
    public static class SimpleInjectorGenericHostExtensions
    {
        /// <summary>
        /// Registers the given <typeparamref name="THostedService"/> in the Container as Singleton and
        /// adds it to the host's pipeline of hosted services.
        /// </summary>
        /// <typeparam name="THostedService">An <see cref="IHostedService"/> to register.</typeparam>
        /// <param name="options">The options.</param>
        /// <returns>The <paramref name="options"/>.</returns>
        public static SimpleInjectorAddOptions AddHostedService<THostedService>(
            this SimpleInjectorAddOptions options)
            where THostedService : class, IHostedService
        {
            if (options is null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            var registration = Lifestyle.Singleton.CreateRegistration<THostedService>(options.Container);

            // Let the built-in configuration system dispose this instance.
            registration.SuppressDisposal = true;

            options.Container.AddRegistration<THostedService>(registration);

            options.Services.AddSingleton<IHostedService>(
                _ => options.Container.GetInstance<THostedService>());

            return options;
        }
    }
}