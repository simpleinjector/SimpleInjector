using System;
using System.Collections.Generic;
using System.Threading;

using Microsoft.Practices.ServiceLocation;

namespace CuttingEdge.ServiceLocation
{
    /// <summary>
    /// Allows verifying whether a given type has a direct or indirect dependency on itself. Verifying is done
    /// by preventing recursive calls to a IInstanceProvider. An instance of this type is related to a single 
    /// instance of a IInstanceProvider. A RecursiveDependencyValidator instance checks a single 
    /// IInstanceProvider and therefore a single service type.
    /// </summary>
    internal sealed class RecursiveDependencyValidator
    {
        private readonly Type typeToValidate;
        private readonly List<Thread> threads = new List<Thread>();

        internal RecursiveDependencyValidator(Type typeToValidate)
        {
            this.typeToValidate = typeToValidate;
        }

        internal void Check()
        {
            // We can lock on this, because RecursiveDependencyValidator is an internal type.
            lock (this)
            {
                // We store the current thread to prevent the validator to incorrectly fail when two threads
                // simultaneously trigger the validation.
                if (this.threads.Contains(Thread.CurrentThread))
                {
                    // We currently don't supply any information through the exception message about the 
                    // actual dependency cycle that causes the problem. Using call stack analysis we would be 
                    // able to build a dependency graph and supply it in this exception message, but not 
                    // something we currently do.
                    throw new ActivationException(StringResources.TypeDependsOnItself(this.typeToValidate));
                }

                this.threads.Add(Thread.CurrentThread);
            }
        }

        internal void RollBack()
        {
            // We can lock on this, because RecursiveDependencyValidator is an internal type.
            lock (this)
            {
                this.threads.Remove(Thread.CurrentThread);
            }            
        }
    }
}