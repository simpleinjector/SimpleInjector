namespace SimpleInjector.CodeSamples
{
    using System;

    public static class ExpirationExtensions
    {
        public static void RegisterWithAbsoluteExpiration<TService, TImplementation>(
            this Container container, TimeSpan timeout)
            where TService : class
            where TImplementation : class, TService
        {
            var lifestyle = CreateExpirationLifestyle(timeout, sliding: false);
            container.Register<TService, TImplementation>(lifestyle);
        }

        public static void RegisterWithSlidingExpiration<TService, TImplementation>(
            this Container container, TimeSpan timeout)
            where TService : class
            where TImplementation : class, TService
        {
            var lifestyle = CreateExpirationLifestyle(timeout, sliding: true);
            container.Register<TService, TImplementation>(lifestyle);
        }

        private static Lifestyle CreateExpirationLifestyle(TimeSpan timeout, bool sliding)
        {
            string name = sliding ? "Sliding" : "Absolute";

            return Lifestyle.CreateCustom(name + " Expiration", instanceCreator =>
            {
                var syncRoot = new object();
                var expirationTime = DateTime.MinValue;
                object instance = null;

                return () =>
                {
                    lock (syncRoot)
                    {
                        if (expirationTime < DateTime.UtcNow)
                        {
                            instance = instanceCreator();

                            if (!sliding)
                            {
                                expirationTime = DateTime.UtcNow.Add(timeout);
                            }
                        }

                        if (sliding)
                        {
                            expirationTime = DateTime.UtcNow.Add(timeout);
                        }

                        return instance;
                    }
                };
            });
        }
    }
}