namespace SimpleInjector.Interception
{
    using System.Collections.Generic;
    using System.Reflection;

    /// <summary>
    /// Interface describing method invocation details.
    /// </summary>
    public interface IInvocation
    {
        /// <summary>Gets the intercepted object.</summary>
        /// <value>An object.</value>
        object Target { get; }

        /// <summary>Gets the intercepted interface method.</summary>
        /// <value>A <see cref="MethodBase"/>.</value>
        MethodBase Method { get; }

        /// <summary>Gets the list of supplied method arguments.</summary>
        /// <value>An array.</value>
        IEnumerable<object> Arguments { get; }

        /// <summary>Gets or sets the return value.</summary>
        /// <value>An object.</value>
        object ReturnValue { get; set; }

        /// <summary>Exectues the intercepted method.</summary>
        void Proceed();
    }
}