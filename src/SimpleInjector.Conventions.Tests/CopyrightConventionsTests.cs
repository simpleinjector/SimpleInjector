namespace SimpleInjector.Conventions.Tests
{
    using System;
    using System.IO;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    [TestCategory("Conventions")]
    public class CopyrightConventionsTests
    {
        private const string ExpectedCopyrightHeaderLine1 =
            "// Copyright (c) Simple Injector Contributors. All rights reserved.";

        private const string ExpectedCopyrightHeaderLine2 =
            "// Licensed under the MIT License. See LICENSE file in the project root for license information.";

        [TestMethod]
        public void AllCSharpCodeFiles_Always_HaveTheCorrectCopyrightHeader()
        {
            var csharpFilesUnderConvention =
                from projectPath in ConventionValues.ProjectsUnderConvention
                from csharpFile in projectPath.GetFiles("*.cs")
                where !StringComparer.OrdinalIgnoreCase.Equals(csharpFile.Name, "AssemblyInfo.cs")
                select csharpFile;

            var csharpFilesWithIncorrectCopyrightHeader =
                from csharpFile in csharpFilesUnderConvention
                let lines = File.ReadLines(csharpFile.FullName).Take(4).ToArray()
                let line1 = lines.FirstOrDefault()
                let line2 = lines.Skip(1).FirstOrDefault()
                let line3 = lines.Skip(2).FirstOrDefault()
                let isHeaderCorrect =
                    line1 == ExpectedCopyrightHeaderLine1 && line2 == ExpectedCopyrightHeaderLine2
                let containsSpacing = string.IsNullOrWhiteSpace(line3)
                where !isHeaderCorrect || !containsSpacing
                select new { csharpFile, isHeaderCorrect, containsSpacing };

            if (csharpFilesWithIncorrectCopyrightHeader.Any())
            {
                var repoRoot = ConventionValues.GetRepositoryRoot().FullName;

                var violations =
                    from violation in csharpFilesWithIncorrectCopyrightHeader
                    let explanation = !violation.isHeaderCorrect
                        ? "The header does not consist of the two expected lines of text"
                        : !violation.containsSpacing
                            ? "There is no space between the copyright header and the rest of the file"
                            : "[Error in query]"
                    let fileName = violation.csharpFile.FullName.Replace(repoRoot, string.Empty)
                    select $"{fileName}: {explanation}";

                Assert.Fail(
                    "The following files do not follow the copyright convention:" + Environment.NewLine +
                    string.Join(Environment.NewLine, violations));
            }
        }
    }
}