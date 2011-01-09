using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CuttingEdge.ServiceLocation.Tests.Unit
{
    [TestClass]
    public class UnregisteredTypeEventArgsTests
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Register_WithNullArgument_ThrowsException()
        {
            // Arrange
            var e = new UnregisteredTypeEventArgs(typeof(IWeapon));
            
            // Act
            e.Register(null);
        }
    }
}