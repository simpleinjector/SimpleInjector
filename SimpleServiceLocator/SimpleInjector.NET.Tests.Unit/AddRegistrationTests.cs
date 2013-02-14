namespace SimpleInjector.Tests.Unit
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class AddRegistrationTests
    {
        public interface IService1 
        {
        }
        
        public interface IService2 
        {
        }

        [TestMethod]
        public void GetInstanceOnTwoKeys_SameSingletonRegistrationForTwoKeys_ReturnsThatSameInstance()
        {
            // Arrange
            var container = new Container();

            var registration = Lifestyle.Singleton.CreateRegistration<Implementation, Implementation>(container);

            container.AddRegistration(typeof(IService1), registration);
            container.AddRegistration(typeof(IService2), registration);

            // Act
            var instance1 = container.GetInstance<IService1>();
            var instance2 = container.GetInstance<IService2>();

            // Assert
            Assert.IsTrue(object.ReferenceEquals(instance1, instance2));
        }

        public class Implementation : IService1, IService2 
        { 
        }
    }
}