namespace SimpleInjector.Tests.Unit
{
    using System;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class GetTypesToRegisterTests
    {
        [ContractClass(typeof(ContractClassForILog))]
        public interface ILog
        {
            void Log(string message);
        }

        [TestMethod]
        public void GetTypesToRegister_WithContainerArgument_ReturnsNoDecorators()
        {
            // Arrange
            var container = new Container();

            // Act
            var types =
                container.GetTypesToRegister(typeof(IService<,>), new[] { typeof(IService<,>).Assembly });

            // Assert
            Assert.IsFalse(types.Any(type => type == typeof(ServiceDecorator)), 
                "The decorator should not have been included.");
        }

        [TestMethod]
        public void GetTypesToRegister_AssemblyContainingContractClass_DoesNotReturnTheContractClass()
        {
            // Arrange
            var container = new Container();

            // Act
            var types = container.GetTypesToRegister(typeof(ILog), new[] { typeof(ILog).Assembly });

            // Assert
            Assert.IsFalse(types.Contains(typeof(ContractClassForILog)),
                "The Code Contracts contract class was expected to be filtered out");
        }

        public class ConsoleLogger : ILog
        {
            public void Log(string message)
            {
            }
        }

        public class DebugLogger : ILog
        {
            public void Log(string message)
            {
            }
        }

        // The assembly must be build with the "CONTRACTS_FULL" compiler directive.
        [ContractClassFor(typeof(ILog))]
        public class ContractClassForILog : ILog
        {
            public void Log(string message)
            {
                Contract.Requires<ArgumentNullException>(message != null);
                Contract.Requires<ArgumentException>(message != string.Empty);
            }
        }
    }
}