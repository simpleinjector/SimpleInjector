namespace SimpleInjector.PartialTrustConsole
{
    using System;
    using System.IO;
    using System.Reflection;
    using System.Runtime.Remoting;

    /// <summary>
    /// Allows running tests for a given test assembly in a partial trust sandbox.
    /// Allows verifying whether Simple Injector runs correctly in partial trust.
    /// </summary>
    public static class Program
    {
        /// <summary>Main method.</summary>
        /// <param name="args">The args.</param>
        public static void Main(string[] args)
        {
            string assemblyFile = args[0];
            bool verboseOutput = args.Length > 1;

            if (verboseOutput)
            {
                Console.WriteLine("Output: verbose");
            }
            else
            {
                Console.WriteLine("Output: silent");
            }

            var domain = AppDomain.CreateDomain("Sandbox", null);

            foreach (string dll in Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, "*.dll"))
            {
                domain.Load(AssemblyName.GetAssemblyName(dll));
            }

            ObjectHandle handle = Activator.CreateInstanceFrom(domain,
                assemblyFile: typeof(TestRunner).Assembly.Location,
                typeName: typeof(TestRunner).FullName);

            var runner = (TestRunner)handle.Unwrap();

            try
            {
                runner.Run(assemblyFile, verboseOutput);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}