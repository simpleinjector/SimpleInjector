namespace SimpleInjector.Advanced
{
    using System;

    // Quite painful but this object must be a public non-generic type to prevent some nasty exception to be 
    // thrown when Activator.CreateInstance tries to create an instance of this type from within the
    // ConditionalWeakTable<TKey, TValue>.
    /// <summary>
    /// This class is for internal use only.
    /// </summary>
    [Obsolete("Do not use this type. It will be removed in a future release.", false)]
    public class ThreadLocalValue
    {
        /// <summary>Gets or sets the value.</summary>
        public object Value { get; set; }
    }
}