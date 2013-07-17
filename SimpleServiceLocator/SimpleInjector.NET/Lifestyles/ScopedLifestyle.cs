namespace SimpleInjector.Lifestyles
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// Base class for scoped lifestyles.
    /// </summary>
    public abstract class ScopedLifestyle : Lifestyle
    {
        /// <summary>Initializes a new instance of the <see cref="ScopedLifestyle"/> class.</summary>
        /// <param name="name">The user friendly name of this lifestyle.</param>
        /// <exception cref="ArgumentException">Thrown when <paramref name="name"/> is null (Nothing in VB) 
        /// or an empty string.</exception>
        protected ScopedLifestyle(string name) : base(name)
        {
        }

        /// <summary>
        /// Allows registering an <paramref name="action"/> delegate that will be called when the scope ends,
        /// but before the scope disposes any instances.
        /// </summary>
        /// <param name="container">The <see cref="Container"/> instance.</param>
        /// <param name="action">The delegate to run when the scope ends.</param>
        /// <exception cref="ArgumentNullException">Thrown when one of the arguments is a null reference
        /// (Nothing in VB).</exception>
        /// <exception cref="InvalidOperationException">Will be thrown when there is currently no active
        /// scope for the supplied <paramref name="container"/>.</exception>
        public abstract void WhenScopeEnds(Container container, Action action);
    }
}