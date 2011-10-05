namespace SimpleInjector.CodeSamples
{
    // http://simpleinjector.codeplex.com/wikipage?title=PerWebRequestAndDisposalExtensions
    using System;
    using System.Diagnostics;
    using System.Web;
    using SimpleInjector;

    /// <summary>
    /// Extension methods for registering types on a per web request basis and ensuring that the object will
    /// get disposed after the request ends.
    /// </summary>
    public static class PerWebRequestRegistrationsWithDisposalExtensions
    {
        public static void RegisterPerWebRequestWithDisposal<TService, TImplementation>(
            this Container container)
            where TService : class
            where TImplementation : class, TService
        {
            container.RegisterPerWebRequest(() => container.GetInstance<TImplementation>());
        }

        public static void RegisterPerWebRequestWithDisposal<TService>(this Container container,
            Func<TService> instanceCreator) where TService : class
        {
            var creator = new DisposablePerWebRequestInstanceCreator<TService>(instanceCreator);
            container.Register<TService>(creator.GetInstance);
        }

        private sealed class DisposablePerWebRequestInstanceCreator<T> where T : class
        {
            private static readonly string key = "DisposablePerWebRequestInstanceCreator_" + typeof(T).FullName;
            private readonly Func<T> instanceCreator;
            private bool endRequestIsRegistered;

            internal DisposablePerWebRequestInstanceCreator(Func<T> instanceCreator)
            {
                this.instanceCreator = instanceCreator;
            }

            [DebuggerStepThrough]
            internal T GetInstance()
            {
                var context = HttpContext.Current;
        
                if (context == null)
                {
                    // No HttpContext: Create the object as transient.
                    return this.instanceCreator();
                }
            
                if (!this.endRequestIsRegistered)
                {
                    // EndRequest must be registered here, because the HttpApplication does not exist during
                    // startup.
                    this.RegisterEndRequest();
                }
            
                T instance = (T)context.Items[key];

                if (instance == null)
                {
                    context.Items[key] = instance = this.instanceCreator();
                }

                return instance;
            }
            
            [DebuggerStepThrough]
            private void RegisterEndRequest()
            {
                HttpContext.Current.ApplicationInstance.EndRequest += this.Dispose;
            
                this.endRequestIsRegistered = true;
            }
        
            [DebuggerStepThrough]
            private void Dispose(object sender, EventArgs e)
            {
                var instance = HttpContext.Current.Items[key] as IDisposable;
            
                if (instance != null)
                {
                    instance.Dispose();
                }
            }
        }
    }
}