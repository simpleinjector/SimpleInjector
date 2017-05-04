namespace SimpleInjector.Tests.Unit
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    public static class AssertThat
    {
        public static void Throws<TException>(Action action, string assertMessage = null) 
            where TException : Exception
        {
            try
            {
                action();
            }
            catch (TException ex)
            {
                AssertThat.IsInstanceOfType(typeof(TException), ex, assertMessage);
                return;
            }

            Assert.Fail("Action was expected to throw an exception. " + assertMessage);
        }

        public static void ThrowsWithExceptionMessageDoesNotContain<TException>(string messageNotToBeExpected,
            Action action, string assertMessage = null)
            where TException : Exception
        {
            Throws<TException>(() =>
            {
                try
                {
                    action();
                }
                catch (TException ex)
                {
                    ExceptionMessageShouldNotContain(messageNotToBeExpected, ex, assertMessage);

                    throw;
                }
            });
        }

        public static void ThrowsWithExceptionMessageContains<TException>(string expectedMessage, 
            Action action, string assertMessage = null)
            where TException : Exception
        {
            Assert.IsFalse(string.IsNullOrEmpty(expectedMessage));

            Throws<TException>(() =>
            {
                try
                {
                    action();
                }
                catch (TException ex)
                {
                    ExceptionMessageContains(expectedMessage, ex, assertMessage);

                    throw;
                }
            });
        }

        public static void ThrowsWithParamName(string expectedParamName, Action action)
        {
            try
            {
                action();

                Assert.Fail("Exception expected.");
            }
            catch (ArgumentException ex)
            {
                ExceptionContainsParamName(expectedParamName, ex);
            }
        }

        public static void ThrowsWithParamName<TArgumentException>(string expectedParamName, Action action)
            where TArgumentException : ArgumentException
        {
            try
            {
                action();

                Assert.Fail("Exception expected.");
            }
            catch (TArgumentException ex)
            {
                AssertThat.IsInstanceOfType(typeof(TArgumentException), ex);

                ExceptionContainsParamName(expectedParamName, ex);
            }
        }

        public static IEnumerable<Exception> GetExceptionChain(this Exception exception)
        {
            while (exception != null)
            {
                yield return exception;
                exception = exception.InnerException;
            }
        }

        public static string TrimInside(this string value)
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

        public static void ExceptionContainsParamName(string expectedParamName, ArgumentException exception)
        {
            string assertMessage = "Exception does not contain parameter with name: " + expectedParamName +
                ". Exception message: " + exception.Message;

#if !SILVERLIGHT
            Assert.AreEqual(expectedParamName, exception.ParamName, assertMessage);
#else
            Assert.IsTrue(exception.Message.Contains(expectedParamName), assertMessage);
#endif
        }

        public static void IsNotInstanceOfType(Type unexpectedType, object actualInstance, string message = null)
        {
            Assert.IsNotNull(actualInstance, message);

            if (unexpectedType.IsAssignableFrom(actualInstance.GetType()))
            {
                Assert.Fail(string.Format("{1} is an instance of type {0}. {2}",
                    ToFriendlyName(unexpectedType), ToFriendlyName(actualInstance.GetType()), message));
            }
        }

        public static void IsInstanceOfType(Type expectedType, object actualInstance, string message = null)
        {
            Assert.IsNotNull(actualInstance, message);

            if (!expectedType.IsAssignableFrom(actualInstance.GetType()))
            {
                Assert.Fail(string.Format("{1} is not an instance of type {0}. {2}",
                    ToFriendlyName(expectedType), ToFriendlyName(actualInstance.GetType()), message));
            }
        }

        public static void AreEqual(Type expectedType, Type actualType, string message = null)
        {
            if (expectedType != actualType)
            {
                Assert.Fail(string.Format("Expected: {0}. Actual: {1}. {2}",
                    ToFriendlyName(expectedType), ToFriendlyName(actualType), message));
            }
        }

        public static void StringContains(string expectedMessage, string actualMessage, string assertMessage)
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

        public static void ExceptionMessageContains(string expectedMessage, Exception actualException,
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
                "Actual string:\n\n" + actualMessage + "\n\n\n" +
                "Expected value to be in the string:\n\n" + expectedMessage + "\n\n" + Environment.NewLine +
                stackTrace);
        }
        
        public static void ExceptionMessageShouldNotContain(string messageNotToBeExpected, Exception actualException,
            string assertMessage = null)
        {
            Assert.IsNotNull(actualException, "actualException should not be null.");

            if (messageNotToBeExpected == null)
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

            Assert.IsTrue(actualMessage != null && !actualMessage.Contains(messageNotToBeExpected),
                assertMessage +
                " The string did contain the expected value, while it was not expected. " +
                "Actual string: \"" + actualMessage + "\". " +
                "Value to be NOT expected in the string: \"" + messageNotToBeExpected + "\"." + 
                Environment.NewLine +
                stackTrace);
        }

        public static void StringContains(string expectedMessage, string actualMessage)
        {
            StringContains(expectedMessage, actualMessage, null);
        }

        public static void SequenceEquals(IEnumerable<Type> expectedTypes, IEnumerable<Type> actualTypes)
        {
            Assert.IsNotNull(actualTypes);

            expectedTypes = expectedTypes.ToArray();
            actualTypes = actualTypes.ToArray();

            if (!expectedTypes.SequenceEqual(actualTypes))
            {
                Assert.Fail("The sequences did not match.\nExpected list: {0}.\nActual list: {1}",
                    expectedTypes.ToFriendlyNamesText(),
                    actualTypes.ToFriendlyNamesText());
            }
        }

        private static string ToFriendlyName(Type type) => type.ToFriendlyName();
    }
}