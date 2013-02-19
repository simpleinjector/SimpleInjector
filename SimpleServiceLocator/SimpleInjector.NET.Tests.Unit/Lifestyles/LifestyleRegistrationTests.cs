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
            var container = ContainerFactory.New();

            var invalidLifestyle = new FakeLifestyle();
            
            var invalidRegistration = new FakeRegistration(invalidLifestyle, container, typeof(RealTimeProvider))
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
                    "The FakeRegistration for the FakeLifestyle returned a null reference " + 
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

        protected override Registration CreateRegistrationCore<TService, TImplementation>(
            Container container)
        {
            return this.RegistrationToReturn;
        }

        protected override Registration CreateRegistrationCore<TService>(Func<TService> instanceCreator, 
            Container container)
        {
            return this.RegistrationToReturn;
        }
    }

    internal sealed class FakeRegistration : Registration
    {
        private readonly Type implementationType;

        public FakeRegistration(Lifestyle lifestyle, Container container, Type implementationType) 
            : base(lifestyle, container)
        {
            this.implementationType = implementationType;
        }

        public override Type ImplementationType
        {
            get { return this.implementationType; }
        }

        public Expression ExpressionToReturn { get; set; }

        public override Expression BuildExpression()
        {
            return this.ExpressionToReturn;
        }
    }
}