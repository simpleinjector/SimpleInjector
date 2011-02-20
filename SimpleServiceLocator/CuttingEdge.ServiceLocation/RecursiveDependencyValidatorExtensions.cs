namespace CuttingEdge.ServiceLocation
{
    /// <summary>
    /// Extension methods for the RecursiveDependencyValidator class.
    /// </summary>
    internal static class RecursiveDependencyValidatorExtensions
    {
        // Prevents any recursive calls from taking place.
        // This method will be inlined by the JIT.
        internal static void Prevent(this RecursiveDependencyValidator validator)
        {
            if (validator != null)
            {
                validator.Check();
            }
        }

        // This method will be inlined by the JIT.
        // Resets the validator to its initial state. This is important when a IInstanceProvider threw an
        // exception, because a new call to that provider would otherwise make the validator think it is a
        // recursive call and throw an exception, and this would hide the exception that would otherwise be
        // thrown by the provider itself.
        internal static void Reset(this RecursiveDependencyValidator validator)
        {
            if (validator != null)
            {
                validator.RollBack();
            }
        }
    }
}