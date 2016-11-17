namespace System.Diagnostics.CodeAnalysis
{
#if NETSTANDARD1_0 || NETSTANDARD1_3
    [Conditional("EXCLUDED")]
    internal sealed class ExcludeFromCodeCoverageAttribute : Attribute
    {
    }
#endif
}