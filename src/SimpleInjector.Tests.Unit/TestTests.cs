namespace SimpleInjector.Tests.Unit
{
    using System.Linq;
    using System.Reflection;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class TestTests
    {
        [TestMethod]
        public void VerifyIfAllTestMethodsHaveAValidName()
        {
            var invalidlyNamedTestMethods =
                from type in this.GetType().Assembly.GetTypes()
                from m in type.GetMethods()
                where m.GetCustomAttributes<TestMethodAttribute>(true).Any() && (
                    m.Name.Contains("MethodUnderTest")
                    || m.Name.Contains("_Scenario")
                    || m.Name.Contains("_Behavior"))
                select m;
            
            var method = invalidlyNamedTestMethods.FirstOrDefault();

            // Act
            Assert.IsNull(method,
                $"There is a test method, named {method?.Name} in class {method?.DeclaringType?.Name} that is not a good test name.");
        }
    }
}