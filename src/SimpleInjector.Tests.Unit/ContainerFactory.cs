namespace SimpleInjector.Tests.Unit
{
    internal static class ContainerFactory
    {
        public static Container New()
        {
            var container = new Container();

            container.Options.ExpressionCompilationBehavior =
                new DynamicAssemblyExpressionCompilationBehavior();

            container.Options.EnableAutoVerification = false;

            return container;
        }
    }
}