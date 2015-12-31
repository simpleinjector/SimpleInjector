namespace System.Diagnostics.CodeAnalysis
{
    [Conditional("EXCLUDED")]
    internal sealed class ExcludeFromCodeCoverageAttribute : Attribute
    {
    }
}