using System;
using System.Web;

using CuttingEdge.ServiceLocation;

namespace CuttingEdge.ServiceLocator.Extensions
{
    /// <summary>
    /// Extension methods for registering types on a per web request basis.
    /// </summary>
    public static partial class SslPerWebRequestRegistrationsExtensions
    {
        public static void RegisterPerWebRequest<T>(this SimpleServiceLocator container,
            Func<T> instanceCreator) where T : class
        {
            var creator = new PerWebRequestInstanceCreator<T>(instanceCreator);

            container.Register<T>(creator.GetInstance);
        }

        private sealed class PerWebRequestInstanceCreator<T> where T : class
        {
            private static readonly string key = "SimpleServiceLocator_" + typeof(T).FullName;
            private Func<T> instanceCreator;

            internal PerWebRequestInstanceCreator(Func<T> instanceCreator)
            {
                this.instanceCreator = instanceCreator;
            }

            internal T GetInstance()
            {
                if (HttpContext.Current == null)
                {
                    throw new InvalidOperationException("The instance can not be created. " +
                        "The current thread is not running in the context of an HTTP request.");
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