namespace SimpleInjector.Tests.Unit.Advanced
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using SimpleInjector.Advanced;
    using SimpleInjector.Extensions;
    using SimpleInjector.Tests.Unit.Extensions;

    [TestClass]
    public class CustomLifestyleSelectionBehaviorTests
    {
        public interface ILog
        {
        }

        [TestMethod]
        public void RegisterConcreteGeneric_WhenCalled_RequestsTheLifestyleBehaviorForALifestyleWithTheExpectedArguments()
        {
            // Arrange
            var behavior = new FakeLifestyleSelectionBehavior();

            var container = new Container(new ContainerOptions { LifestyleSelectionBehavior = behavior });

            // Act
            container.Register<RealTimeProvider>();

            // Assert
            Assert.AreSame(typeof(RealTimeProvider), behavior.SuppliedArguments.ServiceType);
            Assert.AreSame(typeof(RealTimeProvider), behavior.SuppliedArguments.ImplementationType);
        }

        [TestMethod]
        public void RegisterGeneric_WhenCalled_RequestsTheLifestyleBehaviorForALifestyleWithTheExpectedArguments()
        {
            // Arrange
            var behavior = new FakeLifestyleSelectionBehavior();

            var container = new Container(new ContainerOptions { LifestyleSelectionBehavior = behavior });

            // Act
            container.Register<ITimeProvider, RealTimeProvider>();

            // Assert
            Assert.AreSame(typeof(ITimeProvider), behavior.SuppliedArguments.ServiceType);
            Assert.AreSame(typeof(RealTimeProvider), behavior.SuppliedArguments.ImplementationType);
        }

        [TestMethod]
        public void RegisterDelegateGeneric_WhenCalled_RequestsTheLifestyleBehaviorForALifestyleWithTheExpectedArguments()
        {
            // Arrange
            var behavior = new FakeLifestyleSelectionBehavior();

            var container = new Container(new ContainerOptions { LifestyleSelectionBehavior = behavior });

            // Act
            container.Register<ITimeProvider>(() => new RealTimeProvider());

            // Assert
            Assert.AreSame(typeof(ITimeProvider), behavior.SuppliedArguments.ServiceType);
            Assert.AreSame(typeof(ITimeProvider), behavior.SuppliedArguments.ImplementationType);
        }

        [TestMethod]
        public void RegisterConcreteNonGeneric_WhenCalled_RequestsTheLifestyleBehaviorForALifestyleWithTheExpectedArguments()
        {
            // Arrange
            var behavior = new FakeLifestyleSelectionBehavior();

            var container = new Container(new ContainerOptions { LifestyleSelectionBehavior = behavior });

            // Act
            container.Register(typeof(RealTimeProvider));

            // Assert
            Assert.AreSame(typeof(RealTimeProvider), behavior.SuppliedArguments.ServiceType);
            Assert.AreSame(typeof(RealTimeProvider), behavior.SuppliedArguments.ImplementationType);
        }

        [TestMethod]
        public void RegisterNonGeneric_WhenCalled_RequestsTheLifestyleBehaviorForALifestyleWithTheExpectedArguments()
        {
            // Arrange
            var behavior = new FakeLifestyleSelectionBehavior();

            var container = new Container(new ContainerOptions { LifestyleSelectionBehavior = behavior });

            // Act
            container.Register(typeof(ITimeProvider), typeof(RealTimeProvider));

            // Assert
            Assert.AreSame(typeof(ITimeProvider), behavior.SuppliedArguments.ServiceType);
            Assert.AreSame(typeof(RealTimeProvider), behavior.SuppliedArguments.ImplementationType);
        }

        [TestMethod]
        public void RegisterDelegateNonGeneric_WhenCalled_RequestsTheLifestyleBehaviorForALifestyleWithTheExpectedArguments()
        {
            // Arrange
            var behavior = new FakeLifestyleSelectionBehavior();

            var container = new Container(new ContainerOptions { LifestyleSelectionBehavior = behavior });

            // Act
            container.Register(typeof(ITimeProvider), () => new RealTimeProvider());

            // Assert
            Assert.AreSame(typeof(ITimeProvider), behavior.SuppliedArguments.ServiceType);
            Assert.AreSame(typeof(ITimeProvider), behavior.SuppliedArguments.ImplementationType);
        }

        [TestMethod]
        public void GetInstance_OnUnregisteredConcreteTypeWithOverriddenLifestyleSelectionBehavior_ReturnsInstanceWithExpectedLifestyle()
        {
            // Arrange
            var container = new Container();

            container.Options.LifestyleSelectionBehavior =
                new CustomLifestyleSelectionBehavior(Lifestyle.Singleton);

            // Act
            var instance1 = container.GetInstance<RealTimeProvider>();
            var instance2 = container.GetInstance<RealTimeProvider>();

            // Assert
            Assert.AreSame(instance1, instance2, "Unregistered instance was expected to be singleton.");
        }

        [TestMethod]
        public void GetInstanceNonGeneric_OnUnregisteredConcreteTypeWithOverriddenLifestyleSelectionBehavior_ReturnsInstanceWithExpectedLifestyle()
        {
            // Arrange
            var container = new Container();

            container.Options.LifestyleSelectionBehavior =
                new CustomLifestyleSelectionBehavior(Lifestyle.Singleton);

            // Act
            var instance1 = container.GetInstance(typeof(RealTimeProvider));
            var instance2 = container.GetInstance(typeof(RealTimeProvider));

            // Assert
            Assert.AreSame(instance1, instance2, "Unregistered instance was expected to be singleton.");
        }

        [TestMethod]
        public void RegisterAllGeneric_WithOverriddenLifestyleSelectionBehavior_ReturnsInstancesBasedOnTheirExpectedLifestyle()
        {
            // Arrange
            var container = new Container();

            container.Options.LifestyleSelectionBehavior = new CustomLifestyleSelectionBehavior(
                    type => type == typeof(SqlLogger) ? Lifestyle.Singleton : Lifestyle.Transient);

            // Act
            container.RegisterAll<ILog>(typeof(SqlLogger), typeof(FileLogger));

            // Assert
            var loggers = container.GetAllInstances<ILog>();

            Assert.AreSame(loggers.First(), loggers.First(), "SqlLogger should be singleton.");
            Assert.AreNotSame(loggers.Last(), loggers.Last(), "FileLogger should be transient.");
        }

        [TestMethod]
        public void RegisterAllNonGeneric_WithOverriddenLifestyleSelectionBehavior_ReturnsInstancesBasedOnTheirExpectedLifestyle()
        {
            // Arrange
            var container = new Container();

            container.Options.LifestyleSelectionBehavior = new CustomLifestyleSelectionBehavior(
                    type => type == typeof(SqlLogger) ? Lifestyle.Singleton : Lifestyle.Transient);

            // Act
            container.RegisterAll(typeof(ILog), new[] { typeof(SqlLogger), typeof(FileLogger) });

            // Assert
            var loggers = container.GetAllInstances<ILog>();

            Assert.AreSame(loggers.First(), loggers.First(), "SqlLogger should be singleton.");
            Assert.AreNotSame(loggers.Last(), loggers.Last(), "FileLogger should be transient.");
        }

        [TestMethod]
        public void RegisterConcreteWithoutLifestyle_WithCustomLifestyleSelectionBehavior_RegistersInstanceByThatLifestyle()
        {
            RegisterWithoutLifestyle_CustomerLifestyleSelectionBehavior_RegistersInstanceByThatLifestyle(
                typeof(RealTimeProvider),
                container => container.Register<RealTimeProvider>());
        }

        [TestMethod]
        public void RegisterDelegateWithoutLifestyle_WithCustomLifestyleSelectionBehavior_RegistersInstanceByThatLifestyle()
        {
            RegisterWithoutLifestyle_CustomerLifestyleSelectionBehavior_RegistersInstanceByThatLifestyle(
                typeof(ITimeProvider),
                container => container.Register<ITimeProvider>(() => new RealTimeProvider()));
        }

        [TestMethod]
        public void RegisterServiceToImplWithoutLifestyle_WithCustomLifestyleSelectionBehavior_RegistersInstanceByThatLifestyle()
        {
            RegisterWithoutLifestyle_CustomerLifestyleSelectionBehavior_RegistersInstanceByThatLifestyle(
                typeof(ITimeProvider),
                container => container.Register<ITimeProvider, RealTimeProvider>());
        }

        [TestMethod]
        public void RegisterNonGenericConcreteWithoutLifestyle_WithCustomLifestyleSelectionBehavior_RegistersInstanceByThatLifestyle()
        {
            RegisterWithoutLifestyle_CustomerLifestyleSelectionBehavior_RegistersInstanceByThatLifestyle(
                typeof(RealTimeProvider),
                container => container.Register(typeof(RealTimeProvider)));
        }

        [TestMethod]
        public void RegisterNonGenericDelegateWithoutLifestyle_WithCustomLifestyleSelectionBehavior_RegistersInstanceByThatLifestyle()
        {
            RegisterWithoutLifestyle_CustomerLifestyleSelectionBehavior_RegistersInstanceByThatLifestyle(
                typeof(ITimeProvider),
                container => container.Register(typeof(ITimeProvider), () => new RealTimeProvider()));
        }

        [TestMethod]
        public void RegisterNonGenericServiceToImplWithoutLifestyle_WithCustomLifestyleSelectionBehavior_RegistersInstanceByThatLifestyle()
        {
            RegisterWithoutLifestyle_CustomerLifestyleSelectionBehavior_RegistersInstanceByThatLifestyle(
                typeof(ITimeProvider),
                container => container.Register(typeof(ITimeProvider), typeof(RealTimeProvider)));
        }

        [TestMethod]
        public void RegisterManyForOpenGenericWithoutLifestyle_WithCustomLifestyleSelectionBehavior_RegistersInstanceByThatLifestyle()
        {
            RegisterWithoutLifestyle_CustomerLifestyleSelectionBehavior_RegistersInstanceByThatLifestyle(
                typeof(IService<double, decimal>),
                container => container.RegisterManyForOpenGeneric(
                    typeof(IService<,>),
                    AccessibilityOption.PublicTypesOnly,
                    Assembly.GetExecutingAssembly()));
        }

        [TestMethod]
        public void RegisterManyForOpenGenericWithTypesWithoutLifestyle_WithCustomLifestyleSelectionBehavior_RegistersInstanceByThatLifestyle()
        {
            RegisterWithoutLifestyle_CustomerLifestyleSelectionBehavior_RegistersInstanceByThatLifestyle(
                typeof(IService<double, decimal>),
                container => container.RegisterManyForOpenGeneric(
                    typeof(IService<,>),
                    new[] 
                    { 
                        typeof(ServiceForLifestyleSelectionBehaviorTests) 
                    }));
        }

        [TestMethod]
        public void RegisterOpenGenericWithoutLifestyle_WithCustomLifestyleSelectionBehavior_RegistersInstanceByThatLifestyle()
        {
            RegisterWithoutLifestyle_CustomerLifestyleSelectionBehavior_RegistersInstanceByThatLifestyle(
                typeof(IValidate<object>),
                container => container.RegisterOpenGeneric(typeof(IValidate<>), typeof(NullValidator<>)));
        }

        [TestMethod]
        public void RegisterConcreteWithLifestyle_WithCustomLifestyleSelectionBehavior_RegistersInstanceByWithSuppliedLifestyle()
        {
            RegisterWithLifestyle_CustomerLifestyleSelectionBehavior_RegistersInstanceByThatLifestyle(
                typeof(RealTimeProvider),
                (container, lifestyle) => container.Register<RealTimeProvider>(lifestyle));
        }

        [TestMethod]
        public void RegisterDelegateWithLifestyle_WithCustomLifestyleSelectionBehavior_RegistersInstanceByWithSuppliedLifestyle()
        {
            RegisterWithLifestyle_CustomerLifestyleSelectionBehavior_RegistersInstanceByThatLifestyle(
                typeof(ITimeProvider),
                (container, lifestyle) => container.Register<ITimeProvider>(() => new RealTimeProvider(), lifestyle));
        }

        [TestMethod]
        public void RegisterServiceToImplWithLifestyle_WithCustomLifestyleSelectionBehavior_RegistersInstanceByWithSuppliedLifestyle()
        {
            RegisterWithLifestyle_CustomerLifestyleSelectionBehavior_RegistersInstanceByThatLifestyle(
                typeof(ITimeProvider),
                (container, lifestyle) => container.Register<ITimeProvider, RealTimeProvider>(lifestyle));
        }

        [TestMethod]
        public void RegisterNonGenericConcreteWitLifestyle_WithCustomLifestyleSelectionBehavior_RegistersInstanceByWithSuppliedLifestyle()
        {
            RegisterWithLifestyle_CustomerLifestyleSelectionBehavior_RegistersInstanceByThatLifestyle(
                typeof(RealTimeProvider),
                (container, lifestyle) => container.Register(typeof(RealTimeProvider), typeof(RealTimeProvider), lifestyle));
        }

        [TestMethod]
        public void RegisterNonGenericDelegateWithLifestyle_WithCustomLifestyleSelectionBehavior_RegistersInstanceByWithSuppliedLifestyle()
        {
            RegisterWithLifestyle_CustomerLifestyleSelectionBehavior_RegistersInstanceByThatLifestyle(
                typeof(ITimeProvider),
                (container, lifestyle) => container.Register(typeof(ITimeProvider), () => new RealTimeProvider(), lifestyle));
        }

        [TestMethod]
        public void RegisterNonGenericServiceToImplWithLifestyle_WithCustomLifestyleSelectionBehavior_RegistersInstanceByWithSuppliedLifestyle()
        {
            RegisterWithLifestyle_CustomerLifestyleSelectionBehavior_RegistersInstanceByThatLifestyle(
                typeof(ITimeProvider),
                (container, lifestyle) => container.Register(typeof(ITimeProvider), typeof(RealTimeProvider), lifestyle));
        }

        [TestMethod]
        public void RegisterManyForOpenGenericWithLifestyle_WithCustomLifestyleSelectionBehavior_RegistersInstanceByWithSuppliedLifestyle()
        {
            RegisterWithLifestyle_CustomerLifestyleSelectionBehavior_RegistersInstanceByThatLifestyle(
                typeof(IService<double, decimal>),
                (container, lifestyle) => container.RegisterManyForOpenGeneric(
                    typeof(IService<,>),
                    AccessibilityOption.PublicTypesOnly,
                    lifestyle,
                    Assembly.GetExecutingAssembly()));
        }

        [TestMethod]
        public void RegisterManyForOpenGenericWithTypesWithLifestyle_WithCustomLifestyleSelectionBehavior_RegistersInstanceByWithSuppliedLifestyle()
        {
            RegisterWithLifestyle_CustomerLifestyleSelectionBehavior_RegistersInstanceByThatLifestyle(
                typeof(IService<double, decimal>),
                (container, lifestyle) => container.RegisterManyForOpenGeneric(
                    typeof(IService<,>),
                    lifestyle,
                    new[] 
                    {
                        typeof(ServiceForLifestyleSelectionBehaviorTests) 
                    }));
        }

        [TestMethod]
        public void RegisterOpenGenericWithLifestyle_WithCustomLifestyleSelectionBehavior_RegistersInstanceByWithSuppliedLifestyle()
        {
            RegisterWithLifestyle_CustomerLifestyleSelectionBehavior_RegistersInstanceByThatLifestyle(
                typeof(IValidate<object>),
                (container, lifestyle) => container.RegisterOpenGeneric(typeof(IValidate<>), typeof(NullValidator<>), lifestyle));
        }

        [TestMethod]
        public void RegisterDecorator_WithCustomLifestyleSelectionBehavior_RegistersTheDecoratorWithThatCustomLifestyle()
        {
            // Arrange
            var container = new Container();
            container.Options.LifestyleSelectionBehavior =
                new CustomLifestyleSelectionBehavior(Lifestyle.Singleton);

            container.Register<ICommandHandler<RealCommand>, NullCommandHandler<RealCommand>>();
            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(TransactionHandlerDecorator<>));

            // Act
            var decorator1 = container.GetInstance<ICommandHandler<RealCommand>>();
            var decorator2 = container.GetInstance<ICommandHandler<RealCommand>>();

            // Assert
            AssertThat.IsInstanceOfType(typeof(TransactionHandlerDecorator<RealCommand>), decorator1);
            Assert.AreSame(decorator1, decorator2, "Decorator was expected to be a singleton.");
        }

        [TestMethod]
        public void RegisterDecoratorWithPredicate_WithCustomLifestyleSelectionBehavior_RegistersTheDecoratorWithThatCustomLifestyle()
        {
            // Arrange
            var container = new Container();
            container.Options.LifestyleSelectionBehavior =
                new CustomLifestyleSelectionBehavior(Lifestyle.Singleton);

            container.Register<ICommandHandler<RealCommand>, NullCommandHandler<RealCommand>>();
            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(TransactionHandlerDecorator<>),
                context => true);

            // Act
            var decorator1 = container.GetInstance<ICommandHandler<RealCommand>>();
            var decorator2 = container.GetInstance<ICommandHandler<RealCommand>>();

            // Assert
            AssertThat.IsInstanceOfType(typeof(TransactionHandlerDecorator<RealCommand>), decorator1);
            Assert.AreSame(decorator1, decorator2, "Decorator was expected to be a singleton.");
        }

        [TestMethod]
        public void RegisterConcrete_CustomLifestyleSelectionBehaviorThatReturnsANullReference_ThrowsDescriptiveException()
        {
            // Arrange
            Lifestyle lifestyle = null;

            var container = new Container();
            container.Options.LifestyleSelectionBehavior = new CustomLifestyleSelectionBehavior(lifestyle);

            // Act
            Action action = () => container.Register<RealTimeProvider>();

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<ActivationException>(@"
                The CustomLifestyleSelectionBehaviorTests+CustomLifestyleSelectionBehavior that was registered 
                through Container.Options.LifestyleSelectionBehavior returned a null reference after its 
                SelectLifestyle(Type, Type) method was supplied with values 'RealTimeProvider' for serviceType 
                and 'RealTimeProvider' for implementationType. ILifestyleSelectionBehavior.SelectLifestyle 
                implementations should never return null.
                ".TrimInside(),
                action);
        }

        [TestMethod]
        public void RegisterDelegate_CustomLifestyleSelectionBehaviorThatReturnsANullReference_ThrowsDescriptiveException()
        {
            // Arrange
            Lifestyle lifestyle = null;

            var container = new Container();
            container.Options.LifestyleSelectionBehavior = new CustomLifestyleSelectionBehavior(lifestyle);

            // Act
            Action action = () => container.Register<ITimeProvider>(() => new RealTimeProvider());

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<ActivationException>(
                "Container.Options.LifestyleSelectionBehavior returned a null reference",
                action);
        }

        private static void RegisterWithoutLifestyle_CustomerLifestyleSelectionBehavior_RegistersInstanceByThatLifestyle(
            Type serviceType,
            Action<Container> registrationWithoutLifestyle)
        {
            // Arrange
            var expectedLifestyle = Lifestyle.Singleton;

            var container = new Container();

            container.Options.LifestyleSelectionBehavior = new CustomLifestyleSelectionBehavior(expectedLifestyle);

            registrationWithoutLifestyle(container);

            // Act
            var registration = container.GetRegistration(serviceType);

            Assert.IsNotNull(registration, "Registration not found for type " + serviceType.ToFriendlyName());

            Lifestyle actualLifestyle = registration.Lifestyle;

            // Assert
            Assert.AreSame(expectedLifestyle, actualLifestyle,
                string.Format("{0} was expected, but {1} was registered.",
                    expectedLifestyle.Name, actualLifestyle.Name));
        }

        private static void RegisterWithLifestyle_CustomerLifestyleSelectionBehavior_RegistersInstanceByThatLifestyle(
            Type serviceType,
            Action<Container, Lifestyle> registrationWithLifestyle)
        {
            // Arrange
            var expectedLifestyle = Lifestyle.CreateCustom("Expected", creator => creator);
            var notExpectedLifestyle = Lifestyle.CreateCustom("Not Expected", creator => creator);

            var container = new Container();

            container.Options.LifestyleSelectionBehavior = new CustomLifestyleSelectionBehavior(notExpectedLifestyle);

            registrationWithLifestyle(container, expectedLifestyle);

            // Act
            var registration = container.GetRegistration(serviceType);

            Assert.IsNotNull(registration, "Registration not found for type " + serviceType.ToFriendlyName());

            Lifestyle actualLifestyle = registration.Lifestyle;

            // Assert
            Assert.AreSame(expectedLifestyle, actualLifestyle,
                string.Format("{0} was expected, but {1} was registered.",
                    expectedLifestyle.Name, actualLifestyle.Name));
        }

        public class CustomLifestyleSelectionBehavior : ILifestyleSelectionBehavior
        {
            private readonly Func<Type, Lifestyle> lifestyleSelector;

            public CustomLifestyleSelectionBehavior(Lifestyle lifestyle)
            {
                this.lifestyleSelector = type => lifestyle;
            }

            public CustomLifestyleSelectionBehavior(Func<Type, Lifestyle> lifestyleSelector)
            {
                this.lifestyleSelector = lifestyleSelector;
            }

            public Lifestyle SelectLifestyle(Type serviceType, Type implementationType)
            {
                return this.lifestyleSelector(implementationType);
            }
        }

        public class FakeLifestyleSelectionBehavior : ILifestyleSelectionBehavior
        {
            private List<LifestyleArguments> suppliedArgumentCollection = new List<LifestyleArguments>();

            public LifestyleArguments SuppliedArguments
            {
                get { return this.suppliedArgumentCollection.Single(); }
            }

            public Lifestyle SelectLifestyle(Type serviceType, Type implementationType)
            {
                if (implementationType != typeof(Container))
                {
                    this.suppliedArgumentCollection.Add(new LifestyleArguments
                    {
                        ServiceType = serviceType,
                        ImplementationType = implementationType,
                    });
                }

                return Lifestyle.Transient;
            }

            public class LifestyleArguments
            {
                public Type ServiceType { get; set; }

                public Type ImplementationType { get; set; }
            }
        }

        public class SqlLogger : ILog
        {
        }

        public class FileLogger : ILog
        {
        }

        public class ServiceForLifestyleSelectionBehaviorTests : IService<double, decimal>
        {
        }
    }
}