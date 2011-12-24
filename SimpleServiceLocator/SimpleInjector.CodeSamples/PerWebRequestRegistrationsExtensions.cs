namespace SimpleInjector.CodeSamples
{
    // http://simpleinjector.codeplex.com/wikipage?title=PerWebRequestExtensionMethod
    using System;
    using System.Diagnostics;
    using System.Web;
    using SimpleInjector;

    /// <summary>
    /// Extension methods for registering types on a per web request basis.
    /// </summary>
    public static partial class SimpleInjectorPerWebRequestExtensions
    {
        public static void RegisterPerWebRequest<TService, TImplementation>(
            this Container container)
            where TService : class
            where TImplementation : class, TService
        {
            Func<TService> instanceCreator = 
                () => container.GetInstance<TImplementation>();

            container.RegisterPerWebRequest<TService>(instanceCreator);
        }

        public static void RegisterPerWebRequest<TService>(this Container container,
            Func<TService> instanceCreator) where TService : class
        {
            var creator = 
                new PerWebRequestInstanceCreator<TService>(instanceCreator);

            container.Register<TService>(creator.GetInstance);
        }

        public static void DisposeInstance<TService>() where TService : class
        {
            object key = typeof(PerWebRequestInstanceCreator<TService>);

            var instance = HttpContext.Current.Items[key] as IDisposable;

            if (instance != null)
            {
                instance.Dispose();
            }
        }

        private sealed class PerWebRequestInstanceCreator<T> where T : class
        {
            private readonly Func<T> instanceCreator;

            internal PerWebRequestInstanceCreator(Func<T> instanceCreator)
            {
                this.instanceCreator = instanceCreator;
            }

            [DebuggerStepThrough]
            internal T GetInstance()
            {
                var context = HttpContext.Current;

                if (context == null)
                {
                    // No HttpContext: Let's create a transient object.
                    return this.instanceCreator();
                }

                object key = this.GetType();

                T instance = (T)context.Items[key];

                if (instance == null)
                {
                    context.Items[key] = instance = this.instanceCreator();
                }

                return instance;
            }
        }
    }
}