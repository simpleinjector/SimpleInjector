namespace SimpleInjector.Tests.Unit
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class ContainerCollectionAppendToTests
    {
        [TestMethod]
        public void AppendTo_WithValidArguments_Suceeds()
        {
            // Arrange
            var container = ContainerFactory.New();

            // Act
            container.Collections.Append(typeof(object), CreateRegistration(container));
        }

        [TestMethod]
        public void AppendTo_WithNullServiceTypeArgument_ThrowsException()
        {
            // Arrange
            var container = ContainerFactory.New();

            Type invalidServiceType = null;

            // Act
            Action action =
                () => container.Collections.Append(invalidServiceType, CreateRegistration(container));

            // Assert
            AssertThat.ThrowsWithParamName<ArgumentNullException>("serviceType", action);
        }

        [TestMethod]
        public void AppendTo_WithNullRegistrationArgument_ThrowsException()
        {
            // Arrange
            var container = ContainerFactory.New();

            Registration invalidRegistration = null;

            // Act
            Action action = () => container.Collections.Append(typeof(object), invalidRegistration);

            // Assert
            AssertThat.ThrowsWithParamName<ArgumentNullException>("registration", action);
        }

        [TestMethod]
        public void AppendTo_WithRegistrationForDifferentContainer_ThrowsException()
        {
            // Arrange
            var container = ContainerFactory.New();

            var differentContainer = new Container();

            Registration invalidRegistration = CreateRegistration(differentContainer);

            // Act
            Action action = () => container.Collections.Append(typeof(object), invalidRegistration);

            // Assert
            AssertThat.ThrowsWithParamName<ArgumentException>("registration", action);
            AssertThat.ThrowsWithExceptionMessageContains<ArgumentException>(
                "The supplied Registration belongs to a different container.", action);
        }

        [TestMethod]
        public void AppendTo_ForUnregisteredCollection_ResolvesThatRegistrationWhenRequested()
        {
            // Arrange
            var container = ContainerFactory.New();

            var registration = Lifestyle.Transient.CreateRegistration<PluginImpl>(container);

            container.Collections.Append(typeof(IPlugin), registration);

            // Act
            var instance = container.GetAllInstances<IPlugin>().Single();

            // Assert
            AssertThat.IsInstanceOfType(typeof(PluginImpl), instance);
        }

        [TestMethod]
        public void AppendTo_CalledTwice_ResolvesBothRegistrationsWhenRequested()
        {
            // Arrange
            var container = ContainerFactory.New();

            var registration1 = Lifestyle.Transient.CreateRegistration<PluginImpl>(container);
            var registration2 = Lifestyle.Transient.CreateRegistration<PluginImpl2>(container);

            container.Collections.Append(typeof(IPlugin), registration1);
            container.Collections.Append(typeof(IPlugin), registration2);

            // Act
            var instances = container.GetAllInstances<IPlugin>().ToArray();

            // Assert
            AssertThat.IsInstanceOfType(typeof(PluginImpl), instances[0]);
            AssertThat.IsInstanceOfType(typeof(PluginImpl2), instances[1]);
        }

        [TestMethod]
        public void AppendTo_CalledAfterRegisterCollectionWithTypes_CombinedAllRegistrationsWhenRequested()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.RegisterCollection<IPlugin>(new[] { typeof(PluginImpl) });

            var registration = Lifestyle.Transient.CreateRegistration<PluginImpl2>(container);

            container.Collections.Append(typeof(IPlugin), registration);

            // Act
            var instances = container.GetAllInstances<IPlugin>().ToArray();

            // Assert
            AssertThat.IsInstanceOfType(typeof(PluginImpl), instances[0]);
            AssertThat.IsInstanceOfType(typeof(PluginImpl2), instances[1]);
        }

        [TestMethod]
        public void AppendTo_CalledAfterRegisterCollectionWithRegistration_CombinedAllRegistrationsWhenRequested()
        {
            // Arrange
            var container = ContainerFactory.New();

            var registration1 = Lifestyle.Transient.CreateRegistration<PluginImpl>(container);
            var registration2 = Lifestyle.Transient.CreateRegistration<PluginImpl2>(container);

            container.RegisterCollection(typeof(IPlugin), new[] { registration1 });

            container.Collections.Append(typeof(IPlugin), registration2);

            // Act
            var instances = container.GetAllInstances<IPlugin>().ToArray();

            // Assert
            AssertThat.IsInstanceOfType(typeof(PluginImpl), instances[0]);
            AssertThat.IsInstanceOfType(typeof(PluginImpl2), instances[1]);
        }

        [TestMethod]
        public void AppendTo_CalledAfterTheFirstItemIsRequested_ThrowsExpectedException()
        {
            // Arrange
            var container = ContainerFactory.New();

            var registration1 = Lifestyle.Transient.CreateRegistration<PluginImpl>(container);
            var registration2 = Lifestyle.Transient.CreateRegistration<PluginImpl2>(container);

            container.Collections.Append(typeof(IPlugin), registration1);

            var instances = container.GetAllInstances<IPlugin>().ToArray();

            // Act
            Action action = () => container.Collections.Append(typeof(IPlugin), registration2);

            // Assert
            AssertThat.Throws<InvalidOperationException>(action);
        }

        [TestMethod]
        public void AppendTo_OnContainerUncontrolledCollection_ThrowsExpressiveException()
        {
            // Arrange
            var container = ContainerFactory.New();

            IEnumerable<IPlugin> containerUncontrolledCollection = new[] { new PluginImpl() };

            container.RegisterCollection<IPlugin>(containerUncontrolledCollection);

            var registration = Lifestyle.Transient.CreateRegistration<PluginImpl>(container);

            // Act
            Action action = () => container.Collections.Append(typeof(IPlugin), registration);

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<NotSupportedException>(@"
                appending registrations to these collections is not supported. Please register the collection
                with one of the other RegisterCollection overloads if appending is required."
                .TrimInside(),
                action);
        }

        [TestMethod]
        public void GetAllInstances_RegistrationAppendedToExistingOpenGenericRegistration_ResolvesTheExtectedCollection()
        {
            // Arrange
            Type[] expectedHandlerTypes = new[]
            {
                typeof(NewConstraintEventHandler<StructEvent>),
                typeof(StructEventHandler),
            };

            var container = ContainerFactory.New();

            container.RegisterCollection(typeof(IEventHandler<>), new[] { typeof(NewConstraintEventHandler<>) });

            var registration = Lifestyle.Transient.CreateRegistration<StructEventHandler>(container);

            container.Collections.Append(typeof(IEventHandler<>), registration);

            // Act
            Type[] actualHandlerTypes = container.GetAllInstances(typeof(IEventHandler<StructEvent>))
                .Select(h => h.GetType()).ToArray();

            // Assert
            Assert.AreEqual(
                expected: expectedHandlerTypes.ToFriendlyNamesText(),
                actual: actualHandlerTypes.ToFriendlyNamesText());
        }

        [TestMethod]
        public void GetAllInstances_RegistrationPrependedToExistingOpenGenericRegistration_ResolvesTheExtectedCollection()
        {
            // Arrange
            Type[] expectedHandlerTypes = new[]
            {
                typeof(StructEventHandler),
                typeof(NewConstraintEventHandler<StructEvent>),
            };

            var container = ContainerFactory.New();

            var registration = Lifestyle.Transient.CreateRegistration<StructEventHandler>(container);

            container.Collections.Append(typeof(IEventHandler<>), registration);

            container.RegisterCollection(typeof(IEventHandler<>), new[] { typeof(NewConstraintEventHandler<>) });

            // Act
            Type[] actualHandlerTypes = container.GetAllInstances(typeof(IEventHandler<StructEvent>))
                .Select(h => h.GetType()).ToArray();

            // Assert
            Assert.AreEqual(
                expected: expectedHandlerTypes.ToFriendlyNamesText(),
                actual: actualHandlerTypes.ToFriendlyNamesText());
        }

        [TestMethod]
        public void GetAllInstances_MultipleAppendedOpenGenericTypes_ResolvesTheExpectedCollection()
        {
            // Arrange
            Type[] expectedHandlerTypes = new[]
            {
                typeof(NewConstraintEventHandler<StructEvent>),
                typeof(StructConstraintEventHandler<StructEvent>),
                typeof(AuditableEventEventHandler<StructEvent>)
            };

            var container = ContainerFactory.New();

            container.Collections.Append(typeof(IEventHandler<>), typeof(NewConstraintEventHandler<>));
            container.Collections.Append(typeof(IEventHandler<>), typeof(StructConstraintEventHandler<>));
            container.Collections.Append(typeof(IEventHandler<>), typeof(AuditableEventEventHandler<>));

            // Act
            Type[] actualHandlerTypes = container.GetAllInstances(typeof(IEventHandler<StructEvent>))
                .Select(h => h.GetType()).ToArray();

            // Assert
            Assert.AreEqual(
                expected: expectedHandlerTypes.ToFriendlyNamesText(),
                actual: actualHandlerTypes.ToFriendlyNamesText());
        }

        [TestMethod]
        public void GetAllInstances_MultipleAppendedOpenGenericTypesMixedWithClosedGenericRegisterCollection_ResolvesTheExpectedCollection()
        {
            // Arrange
            Type[] expectedHandlerTypes = new[]
            {
                typeof(NewConstraintEventHandler<StructEvent>),
                typeof(AuditableEventEventHandler<StructEvent>),
                typeof(StructConstraintEventHandler<StructEvent>),
            };

            var container = ContainerFactory.New();

            container.Collections.Append(typeof(IEventHandler<>), typeof(NewConstraintEventHandler<>));

            container.RegisterCollection(typeof(IEventHandler<StructEvent>), new[]
            {
                typeof(AuditableEventEventHandler<StructEvent>)
            });

            container.Collections.Append(typeof(IEventHandler<>), typeof(StructConstraintEventHandler<>));

            // Act
            Type[] actualHandlerTypes = container.GetAllInstances(typeof(IEventHandler<StructEvent>))
                .Select(h => h.GetType()).ToArray();

            // Assert
            Assert.AreEqual(
                expected: expectedHandlerTypes.ToFriendlyNamesText(),
                actual: actualHandlerTypes.ToFriendlyNamesText());
        }

        [TestMethod]
        public void GetAllInstances_MultipleOpenGenericTypesAppendedToPreRegistrationWithOpenGenericType_ResolvesTheExpectedCollection()
        {
            // Arrange
            Type[] expectedHandlerTypes = new[]
            {
                typeof(NewConstraintEventHandler<StructEvent>),
                typeof(StructConstraintEventHandler<StructEvent>),
                typeof(AuditableEventEventHandler<StructEvent>)
            };

            var container = ContainerFactory.New();

            container.RegisterCollection(typeof(IEventHandler<>), new[] { typeof(NewConstraintEventHandler<>) });

            container.Collections.Append(typeof(IEventHandler<>), typeof(StructConstraintEventHandler<>));
            container.Collections.Append(typeof(IEventHandler<>), typeof(AuditableEventEventHandler<>));

            // Act
            Type[] actualHandlerTypes = container.GetAllInstances(typeof(IEventHandler<StructEvent>))
                .Select(h => h.GetType()).ToArray();

            // Assert
            Assert.AreEqual(
                expected: expectedHandlerTypes.ToFriendlyNamesText(),
                actual: actualHandlerTypes.ToFriendlyNamesText());
        }

        [TestMethod]
        public void GetAllInstances_RegistrationAppendedToExistingRegistrationForSameClosedType_ResolvesTheInstanceWithExpectedLifestyle()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.RegisterCollection(typeof(IEventHandler<>), new[]
            { 
                // Here we make a closed registration; this causes an explicit registration for the
                // IEventHandlerStructEvent> collection.
                typeof(NewConstraintEventHandler<StructEvent>),
            });

            var registration = Lifestyle.Singleton
                .CreateRegistration(typeof(StructConstraintEventHandler<StructEvent>), container);

            container.Collections.Append(typeof(IEventHandler<>), registration);

            // Act
            var handler1 = container.GetAllInstances<IEventHandler<StructEvent>>().Last();
            var handler2 = container.GetAllInstances<IEventHandler<StructEvent>>().Last();

            // Assert
            AssertThat.IsInstanceOfType(typeof(StructConstraintEventHandler<StructEvent>), handler1);
            Assert.AreSame(handler1, handler2, "The instance was expected to be registered as singleton");
        }

        [TestMethod]
        public void GetAllInstances_DelegatedRegistrationAppendedToExistingRegistrationForSameClosedType_ResolvesTheInstanceWithExpectedLifestyle()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.RegisterCollection(typeof(IEventHandler<>), new[]
            {
                typeof(NewConstraintEventHandler<StructEvent>),
            });

            var registration = Lifestyle.Singleton.CreateRegistration(
                typeof(IEventHandler<StructEvent>),
                () => new StructConstraintEventHandler<StructEvent>(),
                container);

            container.Collections.Append(typeof(IEventHandler<>), registration);

            // Act
            var handler1 = container.GetAllInstances<IEventHandler<StructEvent>>().Last();
            var handler2 = container.GetAllInstances<IEventHandler<StructEvent>>().Last();

            // Assert
            AssertThat.IsInstanceOfType(typeof(StructConstraintEventHandler<StructEvent>), handler1);
            Assert.AreSame(handler1, handler2, "The instance was expected to be registered as singleton");
        }

        private static Registration CreateRegistration(Container container) =>
            Lifestyle.Transient.CreateRegistration<PluginImpl>(container);
    }
}