namespace SimpleInjector.Tests.Unit
{
    using System;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class GetTypesToRegisterTests
    {
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
    }
}