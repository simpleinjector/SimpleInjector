namespace SimpleInjector.Conventions.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SimpleInjector.Integration.Web;

    public static class ConventionValues
    {
        public static readonly ReadOnlyCollection<Assembly> AssembliesUnderConvention;
        public static readonly ReadOnlyCollection<DirectoryInfo> ProjectsUnderConvention;

        static ConventionValues()
        {
            // Ensure all assemblies are loaded
            var types = new[]
            {
                typeof(Container), // SimpleInjector.dll
                typeof(SimpleInjectorWcfExtensions), // SI.Integration.Wcf
                typeof(SimpleInjectorHttpModule), // SI.Integration.Web
                typeof(SimpleInjectorMvcExtensions), // SI.Integration.Web.Mvc
                typeof(SimpleInjectorWebApiExtensions), // SI.Integration.WebApi
                typeof(PackageExtensions) // SI.Packaging
            };

            AssembliesUnderConvention = new ReadOnlyCollection<Assembly>((
                from assembly in AppDomain.CurrentDomain.GetAssemblies()
                let fileName = Path.GetFileName(assembly.Location)
                where fileName.Contains("SimpleInjector")
                where !fileName.Contains("Test")
                select assembly)
                .ToArray());

            // Arrange
            DirectoryInfo repoDir = GetRepositoryRoot();

            ProjectsUnderConvention = new ReadOnlyCollection<DirectoryInfo>((
                from assembly in AssembliesUnderConvention
                let projectName = Path.GetFileNameWithoutExtension(assembly.Location)
                select new DirectoryInfo(Path.Combine(repoDir.FullName, projectName)))
                .ToArray());

            VerifyProjectsExist(ProjectsUnderConvention);
        }

        public static DirectoryInfo GetRepositoryRoot()
        {
            var dir = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);

            while (dir.Name != "src")
            {
                dir = dir.Parent;
            }

            return dir;
        }

        private static void VerifyProjectsExist(IEnumerable<DirectoryInfo> projectsUnderConvention)
        {
            if (projectsUnderConvention.Any(p => !p.Exists))
            {
                Assert.Fail("Not all assemblies under convention could be mapped to a physical project. " +
                    "Following project paths are missing:" + Environment.NewLine +
                    string.Join(Environment.NewLine,
                        from project in projectsUnderConvention
                        where !project.Exists
                        select project.FullName));
            }
        }
    }
}