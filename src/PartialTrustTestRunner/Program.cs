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
        public static void Main(string[] args)
        {
            string assemblyFile = args[0];

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
                runner.Run(assemblyFile);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}