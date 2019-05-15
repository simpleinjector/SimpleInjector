// Copyright (c) Simple Injector Contributors. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace SimpleInjector
{
    using System;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Produces instances for a given registration. Instances of this type are generally created by the
    /// container when calling one of the <b>Register</b> overloads. Instances can be retrieved by calling
    /// <see cref="Container.GetCurrentRegistrations()"/> or <see cref="Container.GetRegistration(Type, bool)"/>.
    /// </summary>
    /// <typeparam name="TService">The service type.</typeparam>
    public class InstanceProducer<TService> : InstanceProducer where TService : class
    {
        /// <summary>Initializes a new instance of the <see cref="InstanceProducer{TService}"/> class.</summary>
        /// <param name="registration">The <see cref="Registration"/>.</param>
        public InstanceProducer(Registration registration)
            : base(typeof(TService), registration)
        {
        }

        /// <summary>Produces an instance.</summary>
        /// <returns>An instance. Will never return null.</returns>
        /// <exception cref="ActivationException">When the instance could not be retrieved or is null.</exception>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification =
            "A property is not appropriate, because get instance could possibly be a heavy operation.")]
        public new TService GetInstance() => (TService)base.GetInstance();
    }
}