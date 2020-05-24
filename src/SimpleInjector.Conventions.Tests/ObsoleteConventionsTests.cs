namespace SimpleInjector.Conventions.Tests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using Microsoft.CodeAnalysis;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    [TestCategory("Conventions")]
    public class ObsoleteConventionsTests
    {
        private const string WillBeRemovedInVersionMessage = " Will be removed in version ";
        private const string TreatedAsErrorFromVersionMessage = " Will be treated as an error from version ";

        private static readonly IEnumerable<Tuple<MemberInfo, ObsoleteAttribute>> ObsoletedTypesAndTypeMembers;

        static ObsoleteConventionsTests()
        {
            var exportedSimpleInjectorTypes = (
                from assembly in ConventionValues.AssembliesUnderConvention
                from type in assembly.GetExportedTypes()
                orderby type.FullName
                select type)
                .ToArray();

            var obsoletedTypes =
                from type in exportedSimpleInjectorTypes
                where type.GetCustomAttributes<ObsoleteAttribute>().Any()
                let attribute = type.GetCustomAttributes<ObsoleteAttribute>().Single()
                select Tuple.Create((MemberInfo)type, attribute);

            var members = (
                from type in exportedSimpleInjectorTypes
                from member in type.GetMembers(BindingFlags.DeclaredOnly
                    | BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static)
                where member.Module.Assembly == type.Assembly
                select member)
                .Distinct();

            var obsoletedTypeMembers =
                from member in members
                where member.GetCustomAttributes<ObsoleteAttribute>().Any()
                let attribute = member.GetCustomAttributes<ObsoleteAttribute>().Single()
                select Tuple.Create(member, attribute);

            ObsoletedTypesAndTypeMembers = (
                from member in obsoletedTypes.Concat(obsoletedTypeMembers)
                let assembly = member.Item1 is Type t ? t.Assembly : member.Item1.DeclaringType.Assembly
                let fileName = Path.GetFileName(assembly.Location)
                orderby fileName != "SimpleInjector.dll", assembly.FullName
                select member)
                .ToList().AsReadOnly();
        }

        [TestMethod]
        public void AllPublicallyExposedMembers_MarkedWithObsoleteAttribute_MessageThatMatchesTheConvention()
        {
            string[] shouldNotContainPhrases = new[]
            {
                "Do not call this",
                "is obsolete",
                "is deprecated"
            };

            bool MessageMatchesObsoleteConvention(ObsoleteAttribute a) =>
                Contains(a.Message, WillBeRemovedInVersionMessage)
                    && !shouldNotContainPhrases.Any(phrase => Contains(a.Message, phrase))
                    && (a.IsError
                        ? !Contains(a.Message, TreatedAsErrorFromVersionMessage)
                        : Contains(a.Message, TreatedAsErrorFromVersionMessage));

            var invalidMembers =
                from member in ObsoletedTypesAndTypeMembers
                where !MessageMatchesObsoleteConvention(member.Item2)
                select member;

            if (invalidMembers.Any())
            {
                var shouldNotContain = string.Join(", ", shouldNotContainPhrases.Select(t => $"\"{t}\""));
                var nl = Environment.NewLine;

                Assert.Fail(
                    "There are code elements that are marked with the [Obsolete] attribute, but do " +
                    "not adhere to the specified convention for the [Obsolete] message. " +
                    $"The convention is as follows:{nl}" +
                    $"- The message should always contain the following phrase: {WillBeRemovedInVersionMessage}.{nl}" +
                    $"- The message should only contain the following phrase when error is false: {TreatedAsErrorFromVersionMessage}.{nl}" +
                    $"- The message should NOT contain the following phrases: {shouldNotContain}.{nl}{nl}" +
                    $"The following code elements do not adhere to the convention:{nl}" +
                    string.Join(nl, invalidMembers.Select(m => $"- {GetMemberName(m.Item1)}")));
            }
        }

        [TestMethod]
        public void NoPublicallyExposedMembers_MarkedAsCompileWarning_ExistWithAVersionNumberHigherThanTheirCurrentAssemblyVersion()
        {
            var warningMembers =
                from member in ObsoletedTypesAndTypeMembers
                where !member.Item2.IsError
                select member;

            var memberVersions =
                from member in warningMembers
                let message = member.Item2.Message
                where Contains(message, TreatedAsErrorFromVersionMessage)
                let version = GetVersionSuffix(message, TreatedAsErrorFromVersionMessage)
                select new { version, member };

            var membersThatShouldHaveBeenMarkedAsError =
                from memberVersion in memberVersions
                let member = memberVersion.member.Item1
                let assemblyVersion = GetAssemblyVersion(member)
                where memberVersion.version <= assemblyVersion
                select new { memberVersion.version, memberVersion.member, assemblyVersion };

            if (membersThatShouldHaveBeenMarkedAsError.Any())
            {
                var nl = Environment.NewLine;

                Assert.Fail(
                    "There are code elements that are marked with the [Obsolete] attribute and state to " +
                    $"be marked as error from a certain version, but those members are not marked as error:{nl}" +

                    string.Join(nl, membersThatShouldHaveBeenMarkedAsError
                        .Select(m => $"- {GetMemberName(m.member.Item1)}. " +
                        $"Assembly version {m.assemblyVersion}. " +
                        $"Treated as error from version: {m.version}.")));
            }
        }

        [TestMethod]
        public void NoPublicallyExposedMembers_MarkedAsCompileError_ExistWithAVersionNumberHigherThanTheirCurrentAssemblyVersion()
        {
            var errorMembers =
                from member in ObsoletedTypesAndTypeMembers
                where member.Item2.IsError
                select member;

            var memberVersions =
                from member in errorMembers
                let message = member.Item2.Message
                where Contains(message, WillBeRemovedInVersionMessage)
                let version = GetVersionSuffix(message, WillBeRemovedInVersionMessage)
                select new { version, member };

            var membersThatShouldHaveBeenRemoved =
                from memberVersion in memberVersions
                let member = memberVersion.member.Item1
                let assemblyVersion = GetAssemblyVersion(member)
                where memberVersion.version <= assemblyVersion
                select new { memberVersion.version, memberVersion.member, assemblyVersion };

            if (membersThatShouldHaveBeenRemoved.Any())
            {
                var nl = Environment.NewLine;

                Assert.Fail(
                    "There are code elements that are marked with the [Obsolete] attribute and state to " +
                    $"be removed from a specific version, but those members still exist:{nl}" +

                    string.Join(nl, membersThatShouldHaveBeenRemoved
                        .Select(m => $"- {GetMemberName(m.member.Item1)}. " +
                        $"Assembly version {m.assemblyVersion}. " +
                        $"Removed version: {m.version}.")));
            }
        }

        private static Version GetVersionSuffix(string message, string prefix)
        {
            int indexOfRemoveMessage = message.IndexOf(prefix, StringComparison.OrdinalIgnoreCase);

            var lastPart = message.Substring(startIndex: indexOfRemoveMessage + prefix.Length);
            var versionText = lastPart.IndexOf(" ") == -1
                ? lastPart.Substring(0, lastPart.Length - 1) // Everything until the closing dot
                : lastPart.Substring(0, lastPart.IndexOf(" ") - 1); // Everything before the first space

            return Version.Parse(versionText);
        }

        private static Version GetAssemblyVersion(MemberInfo member)
        {
            var assembly = member is Type type ? type.Assembly : member.DeclaringType.Assembly;
            return assembly.GetName().Version;
        }

        private static bool Contains(string message, string search) =>
            message.IndexOf(search, StringComparison.OrdinalIgnoreCase) > -1;

        private static string GetMemberName(MemberInfo member) =>
            member is Type t ? "type " + t.FullName + Assembly(t) :
            member is MethodInfo m ? "method " + GetMethodName(m) + Assembly(m.DeclaringType) :
            member is PropertyInfo p ? "property " + GetPropertyName(p) + Assembly(p.DeclaringType) :
            member is ConstructorInfo c ? "ctor " + GetConstructorName(c) + Assembly(c.DeclaringType) :
            $"member {member.DeclaringType.FullName}.{member.Name}" + Assembly(member.DeclaringType);

        private static string GetMethodName(MethodInfo method) =>
            $"{method.DeclaringType.FullName}.{method.Name}(" +
                string.Join(", ",
                    method.GetParameters()
                        .Select(p => p.ParameterType.ToFriendlyName())) + ")";

        private static string GetPropertyName(PropertyInfo property) =>
            $"{property.DeclaringType.FullName}.{property.Name}";

        private static string GetConstructorName(ConstructorInfo ctor) =>
            $"{ctor.DeclaringType.FullName}(" +
                string.Join(", ",
                    ctor.GetParameters()
                        .Select(p => p.ParameterType.ToFriendlyName())) + ")";

        private static string Assembly(Type type) => $" ({type.Assembly.GetName().Name})";
    }
}