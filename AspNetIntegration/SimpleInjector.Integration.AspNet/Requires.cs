using System;

namespace SimpleInjector.Integration.AspNet
{
    internal static class Requires
    {
        internal static void IsNotNull(object instance, string paramName)
        {
            if (object.ReferenceEquals(instance, null))
            {
                throw new ArgumentNullException(paramName);
            }
        }
    }
}