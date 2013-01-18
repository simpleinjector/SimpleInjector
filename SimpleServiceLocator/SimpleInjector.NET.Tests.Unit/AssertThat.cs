namespace SimpleInjector.Tests.Unit
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    internal static class AssertThat
    {
        internal static void Throws<TException>(Action action) where TException : Exception
        {
            try
            {
                action();

                Assert.Fail("Exception expected.");
            }
            catch (TException ex)
            {
                Assert.IsInstanceOfType(ex, typeof(TException));
            }
        }

        internal static void ThrowsWithExceptionMessageContains<TException>(string expectedMessage, Action action)
            where TException : Exception
        {
            Throws<TException>(() =>
            {
                try
                {
                    action();
                }
                catch (Exception ex)
                {
                    ExceptionMessageContains(expectedMessage, ex);

                    throw;
                }
            });
        }

        internal static void ThrowsWithParamName<TArgumentException>(string expectedParamName, Action action)
            where TArgumentException : ArgumentException
        {
            try
            {
                action();

                Assert.Fail("Exception expected.");
            }
            catch (TArgumentException ex)
            {
                Assert.IsInstanceOfType(ex, typeof(TArgumentException));

                ExceptionContainsParamName(ex, expectedParamName);
            }
        }

        internal static IEnumerable<Exception> GetExceptionChain(this Exception exception)
        {
            while (exception != null)
            {
                yield return exception;
                exception = exception.InnerException;
            }
        }

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

        internal static void ExceptionContainsParamName(ArgumentException exception, string expectedParamName)
        {
            string assertMessage = "Exception does not contain parameter with name: " + expectedParamName;

#if !SILVERLIGHT
            Assert.AreEqual(exception.ParamName, expectedParamName, assertMessage);
#else
            Assert.IsTrue(exception.Message.Contains(expectedParamName), assertMessage);
#endif
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

        internal static void ExceptionMessageContains(string expectedMessage, Exception actualException,
            string assertMessage = null)
        {
            Assert.IsNotNull(actualException, "actualException should not be null.");

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

        internal static void StringContains(string expectedMessage, string actualMessage)
        {
            StringContains(expectedMessage, actualMessage, null);
        }

        internal static void WriteToConsole(this Exception exception)
        {
            while (exception != null)
            {
                Console.WriteLine(exception.GetType().FullName);
                Console.WriteLine(exception.Message);
                Console.WriteLine(exception.StackTrace);
                Console.WriteLine();

                exception = exception.InnerException;
            }
        }
    }
}