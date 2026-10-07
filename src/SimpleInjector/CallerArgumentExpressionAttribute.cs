namespace System.Runtime.CompilerServices;

#if !NET6_0
[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
sealed class CallerArgumentExpressionAttribute(string parameterName) : Attribute
{
    public string ParameterName { get; } = parameterName;
}
#endif