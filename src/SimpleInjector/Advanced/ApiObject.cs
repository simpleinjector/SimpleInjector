namespace SimpleInjector.Advanced
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Common base class for Simple Injector API classes.
    /// </summary>
    public abstract class ApiObject
    {
        /// <inheritdoc />
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override bool Equals(object obj) => base.Equals(obj);

        /// <inheritdoc />
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override int GetHashCode() => base.GetHashCode();

        /// <inheritdoc />
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override string ToString() => base.ToString();

        /// <summary>Gets the <see cref="System.Type"/> of the current instance.</summary>
        /// <returns>The <see cref="System.Type"/> instance that represents the exact runtime 
        /// type of the current instance.</returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = @"
            This FxCop warning is valid, but this method is used to be able to attach an 
            EditorBrowsableAttribute to the GetType method, which will hide the method when the user browses 
            the methods of the Container class with IntelliSense. The GetType method has no value for the user
            who will only use this class for registration.")]
        public new Type GetType() => base.GetType();
    }
}