namespace SimpleInjector.CodeSamples
{
    using System;
    using System.Diagnostics;

    public static class ExpirationExtensions
    {
        public static void RegisterWithAbsoluteExpiration<TService, TImplementation>(
            this Container container, TimeSpan timeout)
            where TService : class
            where TImplementation : class, TService
        {
            TService instance = null;
            var syncRoot = new object();
            var expirationTime = DateTime.MinValue;

            container.Register<TService>(() =>
            {
                lock (syncRoot)
                {
                    if (expirationTime < DateTime.UtcNow)
                    {
                        instance = container.GetInstance<TImplementation>();
                        expirationTime = DateTime.UtcNow.Add(timeout);
                    }

                    return instance;
                }
            });
        }

        public static void RegisterWithSlidingExpiration<TService, TImplementation>(
            this Container container, TimeSpan timeout)
            where TService : class
            where TImplementation : class, TService
        {
            TService instance = null;
            var syncRoot = new object();
            var expirationTime = DateTime.MinValue;

            container.Register<TService>(() =>
            {
                lock (syncRoot)
                {
                    if (expirationTime < DateTime.UtcNow)
                    {
                        instance = container.GetInstance<TImplementation>();
                    }

                    expirationTime = DateTime.UtcNow.Add(timeout);

                    return instance;
                }
            });
        }
    }
}