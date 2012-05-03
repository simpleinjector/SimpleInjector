namespace SimpleInjector.Extensions.Tests.Unit
{
    using System;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    internal static class AssertThat
    {
        internal static void StringContains(string expectedMessage, string actualMessage, string assertMessage)
        {
            if (expectedMessage == null)
            {
                return;
            }

            Assert.IsTrue(actualMessage != null && actualMessage.Contains(expectedMessage),
                assertMessage +
                " The string did not contain the expected value. " +
                "Actual string: \"" + actualMessage + "\". " +
                "Expected value to be in the string: \"" + expectedMessage + "\".");
        }

        internal static void StringContains(string expectedMessage, string actualMessage)
        {
            StringContains(expectedMessage, actualMessage, null);
        }

        internal static void ExceptionContainsParamName(string expectedParamName,
            ArgumentException exception)
        {
            string message = "Expected parameter name is not in the exception.";

#if SILVERLIGHT
            Assert.IsTrue(exception.Message.Contains(expectedParamName), message);           
#else
            Assert.AreEqual(expectedParamName, exception.ParamName, message);
#endif
        }
    }
}