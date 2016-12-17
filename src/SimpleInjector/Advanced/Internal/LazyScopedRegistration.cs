#region Copyright Simple Injector Contributors
/* The Simple Injector is an easy-to-use Inversion of Control library for .NET
 * 
 * Copyright (c) 2013-2014 Simple Injector Contributors
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

namespace SimpleInjector.Advanced.Internal
{
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;
    using SimpleInjector.Lifestyles;

    /// <summary>
    /// This is an internal type. Only depend on this type when you want to be absolutely sure a future 
    /// version of the framework will break your code.
    /// </summary>
    /// <typeparam name="TImplementation">Implementation type.</typeparam>
    [EditorBrowsable(EditorBrowsableState.Never)]
    [SuppressMessage("Microsoft.Performance", "CA1815:OverrideEqualsAndOperatorEqualsOnValueTypes",
        Justification = "This struct is not intended for public use.")]
    public struct LazyScopedRegistration<TImplementation>
        where TImplementation : class
    {
        private readonly Registration registration;

        private TImplementation instance;

        /// <summary>
        /// Initializes a new instance of the <see cref="LazyScopedRegistration{TImplementation}"/> 
        /// struct.</summary>
        /// <param name="registration">The registration.</param>
        public LazyScopedRegistration(Registration registration)
        {
            this.registration = registration;
            this.instance = null;
        }

        /// <summary>Gets the lazily initialized instance for the of the current LazyScopedRegistration.</summary>
        /// <param name="scope">The scope that is used to retrieve the instance.</param>
        /// <returns>The cached instance.</returns>
        public TImplementation GetInstance(Scope scope)
        {
            // NOTE: Never pass several scope instances into the GetInstance method of a single
            // LazyScopedRegistration. That would break shit. The scope is passed in here because:
            // -it can't be passed in through the ctor, since that would pre-load the scope which is invalid.
            // -a LazyScope can't be passed in through the ctor, since LazyScope is a struct and this means
            //  there will be multiple copies of the LazyScope defeating the purpose of the LazyScope.
            // -LazyScope can't be a class, since that would force extra pressure on the GC which must be 
            //  prevented.
            if (this.instance == null)
            {
                this.instance =
                    Scope.GetInstance((ScopedRegistration<TImplementation>)this.registration, scope);
            }

            return this.instance;
        }
    }
}