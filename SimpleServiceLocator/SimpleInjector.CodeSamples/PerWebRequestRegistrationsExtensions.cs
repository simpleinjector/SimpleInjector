namespace SimpleInjector.CodeSamples
{
    using System;
    using System.Web;

    using SimpleInjector;

    /// <summary>
    /// Extension methods for registering types on a per web request basis.
    /// </summary>
    public static partial class PerWebRequestRegistrationsExtensions
    {
        public static void RegisterPerWebRequest<TService, TImplementation>(this Container container)
            where TService : class
            where TImplementation : class, TService
        {
            container.RegisterPerWebRequest(() => container.GetInstance<TImplementation>());
        }

        public static void RegisterPerWebRequest<TService>(this Container container,
            Func<TService> instanceCreator) where TService : class
        {
            var creator = new PerWebRequestInstanceCreator<TService>(instanceCreator);
            container.Register<TService>(creator.GetInstance);
        }

        private sealed class PerWebRequestInstanceCreator<T> where T : class
        {
            private static readonly string key = "SimpleInjector_" + typeof(T).FullName;
            private Func<T> instanceCreator;

            internal PerWebRequestInstanceCreator(Func<T> instanceCreator)
            {
                this.instanceCreator = instanceCreator;
            }

            internal T GetInstance()
            {
                if (HttpContext.Current == null)
                {
                    // No HttpContext: Let's create a transient object.
                    return this.instanceCreator();
                }

                T instance = (T)HttpContext.Current.Items[key];

                if (instance == null)
                {
                    HttpContext.Current.Items[key] = instance = this.instanceCreator();
                }

                return instance;
            }
        }
    }
}