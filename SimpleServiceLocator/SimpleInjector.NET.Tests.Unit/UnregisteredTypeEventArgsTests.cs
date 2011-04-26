using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SimpleInjector.Tests.Unit
{
    [TestClass]
    public class UnregisteredTypeEventArgsTests
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Register_WithNullArgument_ThrowsException()
        {
            // Arrange
            var e = new UnregisteredTypeEventArgs(typeof(IUserRepository));
            
            // Act
            e.Register(null);
        }
    }
}