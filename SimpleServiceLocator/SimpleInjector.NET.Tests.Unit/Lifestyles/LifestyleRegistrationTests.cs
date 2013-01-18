namespace SimpleInjector.Tests.Unit.Lifestyles
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Text;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SimpleInjector.Lifestyles;

    [TestClass]
    public class LifestyleRegistrationTests
    {
        [TestMethod]
        public void BuildExpression_ReturningNull_ContainerWillThrowAnExpressiveExceptionMessage()
        {
            // Arrange
            var container = new Container();

            var invalidLifestyle = new FakeLifestyle();
            
            var invalidRegistration = new ExpressionRegistration(invalidLifestyle, container)
            {
                ExpressionToReturn = null
            };

            invalidLifestyle.RegistrationToReturn = invalidRegistration;

            container.Register<ITimeProvider, RealTimeProvider>(invalidLifestyle);

            try
            {
                // Act
                container.GetInstance<ITimeProvider>();

                // Assert
                Assert.Fail("Exception expected.");
            }
            catch (ActivationException ex)
            {
                AssertThat.ExceptionMessageContains(
                    "The ExpressionRegistration for the FakeLifestyle returned a null reference " + 
                    "from its BuildExpression method.", ex);
            }
        }
    }

    internal sealed class FakeLifestyle : Lifestyle
    {
        public FakeLifestyle() : base("Fake")
        {
        }

        public Registration RegistrationToReturn { get; set; }

        protected override int Length
        {
            // Wha evaaahhh
            get { return 1; }
        }

        public override Registration CreateRegistration<TService, TImplementation>(
            Container container)
        {
            return this.RegistrationToReturn;
        }

        public override Registration CreateRegistration<TService>(Func<TService> instanceCreator, 
            Container container)
        {
            return this.RegistrationToReturn;
        }
    }

    internal sealed class ExpressionRegistration : Registration
    {
        public ExpressionRegistration(Lifestyle lifestyle, Container container) 
            : base(lifestyle, container)
        {
        }

        public Expression ExpressionToReturn { get; set; }

        public override Expression BuildExpression()
        {
            return this.ExpressionToReturn;
        }
    }
}