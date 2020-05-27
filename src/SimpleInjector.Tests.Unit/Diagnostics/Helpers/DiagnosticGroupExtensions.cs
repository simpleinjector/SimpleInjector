namespace SimpleInjector.Diagnostics.Tests.Unit.Helpers
{
    internal static class DiagnosticGroupExtensions
    {
        public static DiagnosticGroup Root(this DiagnosticGroup group)
        {
            while (true)
            {
                if (group.Parent is null)
                {
                    return group;
                }

                group = group.Parent;
            }
        }
    }
}