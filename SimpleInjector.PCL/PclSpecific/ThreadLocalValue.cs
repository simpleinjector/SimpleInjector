namespace SimpleInjector.Advanced
{
    // Quite painful but this object must be a public non-generic type to prevent some nasty exception to be 
    // thrown when Activator.CreateInstance tries to create an instance of this type from within the
    // ConditionalWeakTable<TKey, TValue>.
    using System;
    using System.ComponentModel;

    /// <summary>
    /// This class is for internal use only.
    /// </summary>
    [Obsolete("Do not use this type. It will be removed in a future release.", false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class ThreadLocalValue
    {
        /// <summary>Gets or sets the value.</summary>
        /// <value>The value.</value>
        public object Value { get; set; }
    }
}