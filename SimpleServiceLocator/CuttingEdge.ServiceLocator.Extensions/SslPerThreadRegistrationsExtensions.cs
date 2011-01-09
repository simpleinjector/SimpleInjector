using System;

using CuttingEdge.ServiceLocation;

namespace CuttingEdge.ServiceLocator.Extensions
{
    /// <summary>
    /// Extension methods for registering types on a thread-static basis.
    /// </summary>
    public static partial class SslPerThreadRegistrationsExtensions
    {
        public static void RegisterPerThread<T>(this SimpleServiceLocator container,
            Func<T> instanceCreator) where T : class
        {
            var creator = new PerThreadInstanceCreator<T>(instanceCreator);
            container.Register<T>(creator.GetInstance);
        }

        private sealed class PerThreadInstanceCreator<T> where T : class
        {
            [ThreadStatic]
            private static T instance;

            private Func<T> instanceCreator;

            internal PerThreadInstanceCreator(Func<T> instanceCreator)
            {
                this.instanceCreator = instanceCreator;
            }

            internal T GetInstance()
            {
                if (instance == null)
                {
                    instance = this.instanceCreator();
                }

                return instance;
            }
        }
    }
}