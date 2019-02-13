namespace SimpleInjector.Tests.Unit.Diagnostics
{
    using SimpleInjector.Diagnostics;

    public static class ContainerCollectionRegistratorExtensions
    {
        public static void AppendCollection<TService, TImplementation>(
            this Container container,
            Lifestyle lifestyle,
            DiagnosticType suppression)
            where TImplementation : class, TService
        {
            var reg = lifestyle.CreateRegistration<TImplementation>(container);
            reg.SuppressDiagnosticWarning(suppression, "For testing");

            container.Collection.Append(typeof(TService), reg);
        }
    }
}