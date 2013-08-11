namespace SimpleInjector.Tests.Unit.Lifestyles
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Text;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SimpleInjector.Diagnostics;

    [TestClass]
    public class CustomLifestyleTests
    {
        private static readonly Lifestyle CustomLifestyle = 
            Lifestyle.CreateCustom("Custom", creator => () => creator());

        [TestMethod]
        public void GetInstance_OnRegistrationWithCustomLifestyle_Succeeds()
        {
            // Arrange
            var container = new Container();

            container.Register<IUserRepository, InMemoryUserRepository>(CustomLifestyle);

            // Act
            container.GetInstance<IUserRepository>();
        }

        [TestMethod]
        public void GetInstance_OnRegistrationWithDelegatedCustomLifestyle_Succeeds()
        {
            // Arrange
            var container = new Container();

            container.Register<IUserRepository>(() => new InMemoryUserRepository(), CustomLifestyle);

            // Act
            container.GetInstance<IUserRepository>();
        }
    }
}
