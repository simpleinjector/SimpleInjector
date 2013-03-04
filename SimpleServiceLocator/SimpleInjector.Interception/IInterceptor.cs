namespace SimpleInjector.Interception
{
    /// <summary>
    /// Abstraction over interceptors.
    /// </summary>
    public interface IInterceptor
    {
        /// <summary>Intercepts the specified invocation.</summary>
        /// <param name="invocation">The invocation to intercept.</param>
         void Intercept(IInvocation invocation);
    }
}