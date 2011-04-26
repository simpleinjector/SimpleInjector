namespace SimpleInjector.CodeSamples
{
    using System;

    using SimpleInjector;

    /// <summary>
    /// Extension methods for registering types on a thread-static basis.
    /// </summary>
    public static partial class PerThreadRegistrationsExtensions
    {
        public static void RegisterPerThread<TService, TImplementation>(
            this Container container) 
            where TService : class
            where TImplementation : class, TService
        {
            Func<TService> instanceCreator = 
                () => container.GetInstance<TImplementation>();

            RegisterPerThread<TService>(container, instanceCreator);
        }

        public static void RegisterPerThread<TService>(this Container container,
            Func<TService> instanceCreator) where TService : class
        {
            var creator = new PerThreadInstanceCreator<TService>(instanceCreator);
            container.Register<TService>(creator.GetInstance);
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