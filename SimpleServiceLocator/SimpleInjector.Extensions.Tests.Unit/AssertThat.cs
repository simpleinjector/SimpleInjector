namespace SimpleInjector.Extensions.Tests.Unit
{
    using System;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    internal static class AssertThat
    {
        internal static string TrimInside(this string value)
        {
            if (value == null)
            {
                return value;
            }

            var whiteSpaceCharacters = (
                from c in value
                where char.IsWhiteSpace(c)
                where c != ' '
                select c)
                .Distinct()
                .ToArray();

            foreach (char whiteSpaceCharacter in whiteSpaceCharacters)
            {
                value = value.Replace(whiteSpaceCharacter, ' ');
            }

            while (value.Contains("  "))
            {
                value = value.Replace("  ", " ");
            }

            return value.Trim();
        }

        internal static void AreEqual(Type expectedType, Type actualType, string message = null)
        {
            if (expectedType != actualType)
            {
                Assert.Fail(string.Format("Expected: {0}. Actual: {1}. {2}",
                    ToFriendlyName(expectedType), ToFriendlyName(actualType), message));
            }
        }

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

        internal static void ExceptionMessageContains(string expectedMessage, Exception actualException,
            string assertMessage = null)
        {
            if (expectedMessage == null)
            {
                return;
            }

            string stackTrace = "stackTrace: " + Environment.NewLine + Environment.NewLine;

            Exception exception = actualException;

            while (exception != null)
            {
                stackTrace += exception.StackTrace +
                    Environment.NewLine + " <-----------> " + Environment.NewLine;

                exception = exception.InnerException;
            }

            string actualMessage = actualException.Message;

            Assert.IsTrue(actualMessage != null && actualMessage.Contains(expectedMessage),
                assertMessage +
                " The string did not contain the expected value. " +
                "Actual string: \"" + actualMessage + "\". " +
                "Expected value to be in the string: \"" + expectedMessage + "\"." + Environment.NewLine +
                stackTrace);

            StringContains(expectedMessage, actualException.Message, "stackTrace: " + stackTrace);
        }

        private static string ToFriendlyName(Type type)
        {
            if (type == null)
            {
                return "null";
            }

            if (!type.IsGenericType)
            {
                return type.Name;
            }

            string name = type.Name.Substring(0, type.Name.IndexOf('`'));

            var genericArguments =
                type.GetGenericArguments().Select(argument => ToFriendlyName(argument));

            return name + "<" + string.Join(", ", genericArguments.ToArray()) + ">";
        }
    }
}