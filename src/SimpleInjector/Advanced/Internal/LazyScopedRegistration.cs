// Copyright (c) Simple Injector Contributors. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace SimpleInjector.Advanced.Internal
{
    using System.ComponentModel;
    using SimpleInjector.Lifestyles;

    /// <summary>
    /// This is an internal type. Only depend on this type when you want to be absolutely sure a future
    /// version of the framework will break your code.
    /// </summary>
    /// <typeparam name="TImplementation">Implementation type.</typeparam>
    /// <remarks>
    /// Initializes a new instance of the <see cref="LazyScopedRegistration{TImplementation}"/>
    /// struct.</remarks>
    /// <param name="registration">The registration.</param>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public struct LazyScopedRegistration<TImplementation>(Registration registration)
        where TImplementation : class
    {
        private TImplementation? instance = null;

        /// <summary>
        /// Gets the lazily initialized instance for the of the current LazyScopedRegistration.
        /// </summary>
        /// <param name="scope">The scope that is used to retrieve the instance.</param>
        /// <returns>The cached instance.</returns>
        public TImplementation GetInstance(Scope scope)
        {
            // NOTE: Never pass several scope instances into the GetInstance method of a single
            // LazyScopedRegistration. That would break shit. The scope is passed in here because:
            // -it can't be passed in through the ctor; that would pre-load the scope which is invalid.
            // -a LazyScope can't be passed in through the ctor, since LazyScope is a struct and this means
            //  there will be multiple copies of the LazyScope defeating the purpose of the LazyScope.
            // -LazyScope can't be a class, since that would force extra pressure on the GC which must be
            //  prevented.
            if (this.instance is null)
            {
                this.instance =
                    Scope.GetInstance<TImplementation>((ScopedRegistration)registration, scope);
            }

            return this.instance;
        }
    }
}