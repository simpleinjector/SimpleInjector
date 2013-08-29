namespace SimpleInjector.Integration.Web.Forms
{
    using System;

    internal static class Requires
    {
        internal static void IsNotNull(object instance, string paramName)
        {
            if (instance == null)
            {
                throw new ArgumentNullException(paramName);
            }
        }
    }
}