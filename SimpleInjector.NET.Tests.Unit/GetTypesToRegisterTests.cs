namespace SimpleInjector.Tests.Unit
{
    using System;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class GetTypesToRegisterTests
    {
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
    }
}