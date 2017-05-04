namespace SimpleInjector.PartialTrustConsole
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    public class TestRunner : MarshalByRefObject
    {
        public void Run(string testAssemblyName)
        {
            var testClasses =
                from type in Assembly.Load(AssemblyName.GetAssemblyName(testAssemblyName)).GetExportedTypes()
                where type.GetCustomAttribute(typeof(TestClassAttribute)) != null
                select type;

            int tests = 0;
            var failedTests = new List<MethodInfo>();

            foreach (var testClass in testClasses)
            {
                var results = RunTestMethods(testClass);
                tests += results.Item1;
                failedTests.AddRange(results.Item2);
            }

            Console.WriteLine();
            Console.WriteLine("Tests {0}. Failed {1}. Succeeded {2}.", tests, failedTests.Count, tests - failedTests.Count);

            if (failedTests.Any())
            {
                PrintFailedTests(failedTests);
            }
        }

        private static void PrintFailedTests(List<MethodInfo> failedTests)
        {
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("List of failed tests:");

            var old = Console.ForegroundColor;

            try
            {
                Console.ForegroundColor = ConsoleColor.Red;

                foreach (var failedTest in failedTests)
                {
                    Console.WriteLine("{0}.{1}", failedTest.DeclaringType.Name, failedTest.Name);
                }
            }
            finally
            {
                Console.ForegroundColor = old;
            }
        }

        private Tuple<int, List<MethodInfo>> RunTestMethods(Type testClass)
        {
            var testMethods =
                from method in testClass.GetMethods()
                where method.GetCustomAttribute(typeof(TestMethodAttribute)) != null
                select method;

            var failedTests = new List<MethodInfo>();

            foreach (var testMethod in testMethods)
            {
                if (!RunTestMethod(testMethod))
                {
                    failedTests.Add(testMethod);
                }
            }

            return Tuple.Create(testMethods.Count(), failedTests);
        }

        private bool RunTestMethod(MethodInfo testMethod)
        {
            try
            {
                var testClass = Activator.CreateInstance(testMethod.DeclaringType);

                testMethod.Invoke(testClass, new object[0]);

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(testMethod.DeclaringType.FullName + "." + testMethod.Name + " failed: ");
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex);
                Console.WriteLine();

                return false;
            }
        }
    }
}