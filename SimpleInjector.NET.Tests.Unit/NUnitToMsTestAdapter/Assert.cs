namespace Microsoft.VisualStudio.TestTools.UnitTesting
{
    using System;

    // The classes in this folder mimic the MSTest API, while forwarding the calls to NUnit. This allows us
    // to use the same tests in the Silverlight test project, which runs on MSTest, because there is no
    // Silverlight compatible version of NUnit.
    public static class Assert
    {
        public static void AreEqual(object expected, object actual)
        {
            NUnit.Framework.Assert.AreEqual(expected, actual);
        }

        public static void AreEqual<T>(T expected, T actual, string message, params object[] parameters)
        {
            NUnit.Framework.Assert.AreEqual(expected, actual, message, parameters);
        }

        public static void AreNotEqual(object expected, object actual)
        {
            NUnit.Framework.Assert.AreNotEqual(expected, actual);
        }

        public static void AreNotEqual<T>(T expected, T actual, string message, params object[] parameters)
        {
            NUnit.Framework.Assert.AreNotEqual(expected, actual, message, parameters);
        }
        
        public static void Fail(string message)
        {
            NUnit.Framework.Assert.Fail(message);
        }

        public static void Fail(string message, params object[] args)
        {
            NUnit.Framework.Assert.Fail(message, args);
        }

        public static void IsTrue(bool condition)
        {
            NUnit.Framework.Assert.IsTrue(condition);
        }

        public static void IsTrue(bool condition, string message)
        {
            NUnit.Framework.Assert.IsTrue(condition, message);
        }

        public static void IsTrue(bool condition, string message, params object[] args)
        {
            NUnit.Framework.Assert.IsTrue(condition, message, args);
        }

        public static void IsFalse(bool condition)
        {
            NUnit.Framework.Assert.IsFalse(condition);
        }

        public static void IsFalse(bool condition, string message)
        {
            NUnit.Framework.Assert.IsFalse(condition, message);
        }

        public static void IsNull(object instance)
        {
            NUnit.Framework.Assert.IsNull(instance);
        }

        public static void IsNull(object instance, string message)
        {
            NUnit.Framework.Assert.IsNull(instance, message);
        }

        public static void IsNotNull(object instance)
        {
            NUnit.Framework.Assert.IsNotNull(instance);
        }

        public static void IsNotNull(object instance, string message)
        {
            NUnit.Framework.Assert.IsNotNull(instance, message);
        }

        public static void AreSame(object expected, object actual)
        {
            NUnit.Framework.Assert.AreSame(expected, actual);
        }

        public static void AreSame(object expected, object actual, string message)
        {
            NUnit.Framework.Assert.AreSame(expected, actual, message);
        }

        public static void AreNotSame(object expected, object actual)
        {
            NUnit.Framework.Assert.AreNotSame(expected, actual);
        }

        public static void AreNotSame(object expected, object actual, string message)
        {
            NUnit.Framework.Assert.AreNotSame(expected, actual, message);
        }
    }
}