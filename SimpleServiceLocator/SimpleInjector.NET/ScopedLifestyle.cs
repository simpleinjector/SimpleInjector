namespace SimpleInjector
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

        /// <summary>
        /// Adds the <paramref name="disposable"/> to the list of items that will get disposed when the
        /// scope ends.
        /// </summary>
        /// <remarks>
        /// Note to implementers: Instances registered for disposal will have to be disposed in the opposite
        /// order of registration, since disposable components might still need to call disposable dependencies
        /// in their Dispose() method.
        /// </remarks>
        /// <param name="container">The <see cref="Container"/> instance.</param>
        /// <param name="disposable">The instance that should be disposed when the scope ends.</param>
        /// <exception cref="ArgumentNullException">Thrown when one of the arguments is a null reference
        /// (Nothing in VB).</exception>
        /// <exception cref="InvalidOperationException">Will be thrown when there is currently no active
        /// scope for the supplied <paramref name="container"/>.</exception>
        public abstract void RegisterForDisposal(Container container, IDisposable disposable);

        /// <summary>
        /// Disposes the list of supplied <paramref name="disposables"/>. The list is iterated in reverse 
        /// order (the first element in the list will be disposed last) and the method ensures that the
        /// Dispose method of each element is called, regardless of any exceptions raised from any previously
        /// called Dispose methods. If multiple exceptions are thrown, the last thrown exception will bubble
        /// up the call stack.
        /// </summary>
        /// <param name="disposables">The list of objects to be disposed.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="disposables"/> is a null
        /// reference.</exception>
        protected internal static void DisposeInstances(IList<IDisposable> disposables)
        {
            Requires.IsNotNull(disposables, "disposables");

            DisposeInstances(disposables, disposables.Count - 1);
        }

        // This method simulates the behavior of a set of nested 'using' statements: It ensures that dispose
        // is called on each element, even if a previous instance threw an exception. 
        private static void DisposeInstances(IList<IDisposable> disposables, int index)
        {
            try
            {
                while (index >= 0)
                {
                    disposables[index].Dispose();

                    index--;
                }
            }
            finally
            {
                if (index >= 0)
                {
                    DisposeInstances(disposables, index - 1);
                }
            }
        }
    }
}