namespace SimpleInjector.Diagnostics.Tests.Unit
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SimpleInjector.Diagnostics;
    using SimpleInjector.Diagnostics.Debugger;
    using SimpleInjector.Tests.Unit;

    [TestClass]
    public class CustomLifestyleTests
    {
        private static readonly Lifestyle CustomLifestyle =
            Lifestyle.CreateCustom("Custom", creator => () => creator());

        [TestMethod]
        public void ContainerDebugView_CustomLifestyleDependingOnSingleton_ResultsInNoWarnings()
        {
            // Arrange
            var container = new Container();

            container.Register<IUserRepository, InMemoryUserRepository>(Lifestyle.Singleton);

            // FakeUserService -> IUserRepository
            container.Register<UserServiceBase, FakeUserService>(CustomLifestyle);

            // Act
            var items = VerifyAndGetDebuggerViewItems(container);

            // Assert
            Assert_NoWarnings(items);
        }

        [TestMethod]
        public void ContainerDebugView_CustomLifestyleDependingOnLongerThanTransient_ResultsInWarning()
        {
            // Arrange
            var container = new Container();

            var longerThanTransientLifestyle = new FakeLifestyle(length: 100);

            container.Register<IUserRepository, InMemoryUserRepository>(longerThanTransientLifestyle);
            container.Register<UserServiceBase, FakeUserService>(CustomLifestyle);

            // Act
            var items = VerifyAndGetDebuggerViewItems(container);

            // Assert
            Assert_HasLifestyleMismatch(items);
        }

        [TestMethod]
        public void ContainerDebugView_CustomLifestyleDependingOnTransient_ResultsInWarning()
        {
            // Arrange
            var container = new Container();

            container.Register<IUserRepository, InMemoryUserRepository>(Lifestyle.Transient);
            container.Register<UserServiceBase, FakeUserService>(CustomLifestyle);

            // Act
            var items = VerifyAndGetDebuggerViewItems(container);

            // Assert
            Assert_HasLifestyleMismatch(items);
        }

        [TestMethod]
        public void ContainerDebugView_TransientDependingOnCustomLifestyle_ResultsInNoWarnings()
        {
            // Arrange
            var container = new Container();

            container.Register<IUserRepository, InMemoryUserRepository>(Lifestyle.Singleton);
            container.Register<UserServiceBase, FakeUserService>(CustomLifestyle);

            // UserController -> UserServiceBase.
            container.Register<UserController, UserController>(Lifestyle.Transient);

            // Act
            var items = VerifyAndGetDebuggerViewItems(container);

            // Assert
            Assert_NoWarnings(items);
        }

        [TestMethod]
        public void ContainerDebugView_LongerThanTransientDependingOnCustomLifestyle_ResultsInWarning()
        {
            // Arrange
            var container = new Container();

            var longerThanTransientLifestyle = new FakeLifestyle(length: 100);

            container.Register<IUserRepository, InMemoryUserRepository>(Lifestyle.Singleton);

            // It should produce a warning because we don't know what the exact lifetime of the custom is,
            // so if it's not transient, there could potentially be a problem.
            container.Register<UserServiceBase, FakeUserService>(CustomLifestyle);
            container.Register<UserController, UserController>(longerThanTransientLifestyle);

            // Act
            var items = VerifyAndGetDebuggerViewItems(container);

            // Assert
            Assert_HasLifestyleMismatch(items);
        }

        [TestMethod]
        public void ContainerDebugView_SingletonDependingOnCustomLifestyle_ResultsInWarning()
        {
            // Arrange
            var container = new Container();

            container.Register<IUserRepository, InMemoryUserRepository>(Lifestyle.Singleton);

            // It is very likely that Custom lifestyle is shorter than singleton and that's why we should
            // generate a warning.
            container.Register<UserServiceBase, FakeUserService>(CustomLifestyle);
            container.Register<UserController, UserController>(Lifestyle.Singleton);

            // Act
            var items = VerifyAndGetDebuggerViewItems(container);

            // Assert
            Assert_HasLifestyleMismatch(items);
        }

        private static DebuggerViewItem[] VerifyAndGetDebuggerViewItems(Container container)
        {
            container.Options.SuppressLifestyleMismatchVerification = true;

            container.Verify(VerificationOption.VerifyOnly);

            return new ContainerDebugView(container).Items;
        }

        private static void Assert_NoWarnings(DebuggerViewItem[] items)
        {
            Assert.IsTrue(items.Any(item => item.Description == "No warnings detected."),
                "There are warnings detected.");
        }

        private static void Assert_HasLifestyleMismatch(DebuggerViewItem[] items)
        {
            Assert.IsTrue(items.Any(item => item.Name == "Lifestyle Mismatches"),
                "No lifestyle mismatch was detected.");
        }
        
        private sealed class FakeLifestyle : Lifestyle
        {
            public FakeLifestyle(int length)
                : base("Fake")
            {
                this.Length = length;
            }

            public override int Length { get; }

            protected internal override Registration CreateRegistrationCore<TConcrete>(Container c) => 
                new FakeRegistration<TConcrete>(this, c);

            protected internal override Registration CreateRegistrationCore<TService>(Func<TService> instanceCreator,
                Container container)
            {
                throw new NotImplementedException();
            }

            private sealed class FakeRegistration<TImplementation> : Registration 
                where TImplementation : class
            {
                public FakeRegistration(Lifestyle lifestyle, Container container)
                    : base(lifestyle, container)
                {
                }

                public override Type ImplementationType => typeof(TImplementation);

                public override Expression BuildExpression() => this.BuildTransientExpression();
            }
        }
    }
}
