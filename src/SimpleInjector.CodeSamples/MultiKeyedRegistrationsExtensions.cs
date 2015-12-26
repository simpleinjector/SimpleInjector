namespace SimpleInjector.CodeSamples
{
    // These extension methods are a simplification over doing three or for registrations like this:
    // container.Register<TImplementation>(lifestyle);
    // container.Register<TService1>(() => container.GetInstance<TImplementation>());
    // container.Register<TService2>(() => container.GetInstance<TImplementation>());
    // These extension methods bring two advantages:
    // 1. The TImplementation does not have to be registered explicitly.
    // 2. Any registered initializer on either TService1 or TService2 will not go of twice.
    public static class MultikeyedRegistrationsExtensions
    {
        public static void Register<TService1, TService2, TImplementation>(
            this Container container, Lifestyle lifestyle)
            where TImplementation : class, TService1, TService2
            where TService1 : class
            where TService2 : class
        {
            var registration = lifestyle.CreateRegistration<TImplementation, TImplementation>(container);

            container.AddRegistration(typeof(TService1), registration);
            container.AddRegistration(typeof(TService2), registration);
        }

        public static void Register<TService1, TService2, TService3, TImplementation>(
            this Container container, Lifestyle lifestyle)
            where TImplementation : class, TService1, TService2, TService3
            where TService1 : class
            where TService2 : class
            where TService3 : class
        {
            var registration = lifestyle.CreateRegistration<TImplementation, TImplementation>(container);

            container.AddRegistration(typeof(TService1), registration);
            container.AddRegistration(typeof(TService2), registration);
            container.AddRegistration(typeof(TService3), registration);
        }
    }
}