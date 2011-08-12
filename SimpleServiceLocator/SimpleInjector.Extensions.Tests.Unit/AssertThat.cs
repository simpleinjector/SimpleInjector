using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SimpleInjector.Extensions.Tests.Unit
{
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
    }
}