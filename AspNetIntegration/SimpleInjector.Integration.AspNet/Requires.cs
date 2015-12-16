using System;

namespace SimpleInjector
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