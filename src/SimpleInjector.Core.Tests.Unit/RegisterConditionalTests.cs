namespace SimpleInjector.Tests.Unit
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SimpleInjector.Advanced;

    /// <summary>Tests for testing conditional registrations.</summary>
    [TestClass]
    public class RegisterConditionalTests
    {
        [TestMethod]
        public void RegisterConditionalNonGeneric_AllowOverridingRegistrations_NotSupported()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Options.AllowOverridingRegistrations = true;

            // Act
            Action action = () => container.RegisterConditional(typeof(ILogger), typeof(NullLogger),
                Lifestyle.Singleton, c => true);

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<NotSupportedException>(
                "The making of conditional registrations is not supported when AllowOverridingRegistrations " +
                "is set, because it is impossible for the container to detect whether the registration " +
                "should replace a different registration or not.",
                action);
        }

        [TestMethod]
        public void RegisterConditionalOpenGeneric_AllowOverridingRegistrations_NotSupported()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Options.AllowOverridingRegistrations = true;

            // Act
            Action action = () => container.RegisterConditional(typeof(IGeneric<>), typeof(GenericType<>),
                Lifestyle.Singleton, c => true);

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<NotSupportedException>(
                "The making of conditional registrations is not supported when AllowOverridingRegistrations " +
                "is set, because it is impossible for the container to detect whether the registration " +
                "should replace a different registration or not.",
                action);
        }

        [TestMethod]
        public void RegisterConditionalOnClosedGeneric_AllowOverridingRegistrations_NotSupported()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Options.AllowOverridingRegistrations = true;

            // Act
            Action action = () => container.RegisterConditional(typeof(IGeneric<int>), typeof(GenericType<int>),
                Lifestyle.Singleton, c => true);

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<NotSupportedException>(
                "The making of conditional registrations is not supported when AllowOverridingRegistrations " +
                "is set, because it is impossible for the container to detect whether the registration " +
                "should replace a different registration or not.",
                action);
        }

        [TestMethod]
        public void RegisterOpenGeneric_TwoEquivalentImplementationsOfTheSameInterface_Verifies()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.RegisterConditional(typeof(IOpenGenericWithPredicate<>), typeof(OpenGenericWithPredicate1<>),
                Lifestyle.Transient, c => c.ServiceType.GetGenericArguments().Single().GetType() == typeof(int));

            container.RegisterConditional(typeof(IOpenGenericWithPredicate<>), typeof(OpenGenericWithPredicate2<>),
                Lifestyle.Transient, c => c.ServiceType.GetGenericArguments().Single().GetType() == typeof(long));

            // Act
            container.Verify();
        }

        [TestMethod]
        public void RegisterOpenGeneric_PredicateContext_ServiceTypeIsClosedImplentation()
        {
            // Arrange
            Type actualServiceType = null;

            var container = ContainerFactory.New();
            container.RegisterConditional(typeof(IOpenGenericWithPredicate<>), typeof(OpenGenericWithPredicate1<>),
                Lifestyle.Transient, c =>
                {
                    actualServiceType = c.ServiceType;
                    return true;
                });

            // Act
            var result = container.GetInstance<IOpenGenericWithPredicate<int>>();

            // Assert
            Assert.IsNotNull(actualServiceType, "Predicate was not called");
            Assert.IsFalse(actualServiceType.ContainsGenericParameter(), "ServiceType should be a closed type");
        }

        [TestMethod]
        public void RegisterOpenGeneric_PredicateContext_ImplementationTypeIsClosedImplentation()
        {
            // Arrange
            Type actualImplementationType = null;

            var container = ContainerFactory.New();
            container.RegisterConditional(typeof(IOpenGenericWithPredicate<>), typeof(OpenGenericWithPredicate1<>),
                Lifestyle.Transient, c =>
                {
                    actualImplementationType = c.ImplementationType;
                    return true;
                });

            // Act
            var result = container.GetInstance<IOpenGenericWithPredicate<int>>();

            // Assert
            Assert.IsNotNull(actualImplementationType, "Predicate was not called");
            Assert.IsFalse(actualImplementationType.ContainsGenericParameter(), "ImplementationType should be a closed type");
        }

        [TestMethod]
        public void RegisterOpenGeneric_TwoEquivalentImplementationsOfTheSameInterfaceWithValidPredicate_AppliesPredicate1()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.RegisterConditional(typeof(IOpenGenericWithPredicate<>), typeof(OpenGenericWithPredicate1<>),
                Lifestyle.Transient, c => c.ImplementationType.GetGenericArguments().Single() == typeof(int));

            container.RegisterConditional(typeof(IOpenGenericWithPredicate<>), typeof(OpenGenericWithPredicate2<>),
                Lifestyle.Transient, c => c.ImplementationType.GetGenericArguments().Single() == typeof(long));

            // Act
            var result = container.GetInstance<IOpenGenericWithPredicate<int>>();

            // Assert
            Assert.IsNotNull(result);
            AssertThat.IsInstanceOfType(typeof(OpenGenericWithPredicate1<int>), result);
        }

        [TestMethod]
        public void RegisterOpenGeneric_TwoEquivalentImplementationsOfTheSameInterfaceWithValidPredicate_AppliesPredicate2()
        {
            // Arrange
            var container = ContainerFactory.New();
            container.RegisterConditional(typeof(IOpenGenericWithPredicate<>), typeof(OpenGenericWithPredicate1<>),
                Lifestyle.Transient, c => c.ImplementationType.GetGenericArguments().Single() == typeof(int));

            container.RegisterConditional(typeof(IOpenGenericWithPredicate<>), typeof(OpenGenericWithPredicate2<>),
                Lifestyle.Transient, c => c.ImplementationType.GetGenericArguments().Single() == typeof(long));

            // Act
            var result = container.GetInstance<IOpenGenericWithPredicate<long>>();

            // Assert
            Assert.IsNotNull(result);
            AssertThat.IsInstanceOfType(typeof(OpenGenericWithPredicate2<long>), result);
        }

        [TestMethod]
        public void RegisterOpenGeneric_TwoEquivalentImplementationsOfTheSameInterfaceWithOverlappingPredicate_ThrowsExceptionWhenResolving()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.RegisterConditional(typeof(IOpenGenericWithPredicate<>), typeof(OpenGenericWithPredicate1<>),
                Lifestyle.Transient, c => true);

            // Since both registrations are conditional for a different implementation, it's impossible to check
            // this here, so this call to RegisterConditional must succeed and we need to check when resolving.
            container.RegisterConditional(typeof(IOpenGenericWithPredicate<>), typeof(OpenGenericWithPredicate2<>),
                Lifestyle.Transient, c => true);

            // Act
            Action action = () => container.GetInstance<IOpenGenericWithPredicate<long>>();

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<ActivationException>(
                "Multiple applicable registrations found for IOpenGenericWithPredicate<Int64>",
                action,
                "GetInstance should fail because the framework should detect that more than one " +
                "implementation of the requested service.");
        }

        [TestMethod]
        public void RegisterOpenGeneric_TwoEquivalentImplementationsOfTheSameInterfaceWithOverlappingPredicate_ThrowsException2()
        {
            // Arrange
            var container = ContainerFactory.New();
            container.RegisterConditional(typeof(IOpenGenericWithPredicate<>), typeof(OpenGenericWithPredicate1<>),
                Lifestyle.Transient, c => c.ImplementationType.GetGenericArguments().Single() == typeof(int));

            container.RegisterConditional(typeof(IOpenGenericWithPredicate<>), typeof(OpenGenericWithPredicate2<>),
                Lifestyle.Transient, c => c.ImplementationType.GetGenericArguments().Single().Namespace.StartsWith("System"));

            // Act
            var result1 = container.GetInstance<IOpenGenericWithPredicate<long>>();
            Action action = () =>
                container.GetInstance<IOpenGenericWithPredicate<int>>();

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<ActivationException>(
                "Multiple applicable registrations found for IOpenGenericWithPredicate<Int32>",
                action,
                "GetInstance should fail because the framework should detect that more than one " +
                "implementation of the requested service.");
        }

        [TestMethod]
        public void RegisterOpenGeneric_TwoEquivalentImplementationsWithValidPredicate_UpdateHandledProperty()
        {
            // Arrange
            bool handled = false;

            var container = ContainerFactory.New();
            container.RegisterConditional(typeof(IOpenGenericWithPredicate<>), typeof(OpenGenericWithPredicate1<>),
                Lifestyle.Transient, c =>
                {
                    if (c.Handled)
                    {
                        throw new InvalidOperationException("The test assumes handled is false at this time.");
                    }

                    return c.ImplementationType.GetGenericArguments().Single() == typeof(int);
                });

            container.RegisterConditional(typeof(IOpenGenericWithPredicate<>), typeof(OpenGenericWithPredicate2<>),
                Lifestyle.Transient, c =>
                {
                    // this is the test - we are checking that c.handled changed between
                    // the registered Predicates for OpenGenericWithPredicate1<> and OpenGenericWithPredicate2<>
                    handled = c.Handled;
                    return c.ImplementationType.GetGenericArguments().Single() == typeof(long);
                });

            // Act
            handled = false;
            var result = container.GetInstance<IOpenGenericWithPredicate<int>>();

            // Assert
            Assert.IsTrue(handled);
        }

        [TestMethod]
        public void RegisterUnconditional_AfterAConditionalRegistrationForTheSameServcieTypeHasBeenMade_ThrowsAnExpressiveException()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.RegisterConditional(typeof(ILogger), typeof(NullLogger), Lifestyle.Singleton, c => true);

            // Act
            Action action = () => container.Register<ILogger, ConsoleLogger>();

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<InvalidOperationException>(
                "Type ILogger has already been registered as conditional registration.",
                action);
        }

        [TestMethod]
        public void RegisterConditional_AfterAnUnconditionalRegistrationForTheSameServiceTypeHasBeenMade_ThrowsAnExpressiveException()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Register<ILogger, ConsoleLogger>();

            // Act
            Action action = () => container.RegisterConditional(typeof(ILogger), typeof(NullLogger), c => true);

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<InvalidOperationException>(
                "Type ILogger has already been registered as unconditional registration. For non-generic " +
                "types, conditional and unconditional registrations can't be mixed.",
                action);
        }

        [TestMethod]
        public void RegisterClosedUnconditional_AfterAClosedConditionalRegistrationForTheSameServcieTypeHasBeenMade_ThrowsAnExpressiveException()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.RegisterConditional(typeof(IGeneric<int>), typeof(IntGenericType), c => true);

            // Act
            Action action = () => container.Register<IGeneric<int>, GenericType<int>>();

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<InvalidOperationException>(@"
                There is already a conditional registration for IGeneric<Int32> (with implementation 
                IntGenericType) that overlaps with the registration for GenericType<Int32> that 
                you are trying to make. This new registration would cause ambiguity, because both 
                registrations would be used for the same closed service types. Either remove one of the 
                registrations or make them both conditional."
                .TrimInside(),
                action);
        }

        [TestMethod]
        public void RegisterClosedConditional_AfterAClosedUnconditionalRegistrationForTheSameServiceTypeHasBeenMade_ThrowsAnExpressiveException()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Register<IGeneric<int>, GenericType<int>>();

            // Act
            Action action = () => container.RegisterConditional(typeof(IGeneric<int>), typeof(IntGenericType), 
                c => true);

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<InvalidOperationException>(@"
                There is already a registration for IGeneric<Int32> (with implementation 
                GenericType<Int32>) that overlaps with the conditional registration for IntGenericType that 
                you are trying to make. This new registration would cause ambiguity, because both 
                registrations would be used for the same closed service types. Either remove one of the 
                registrations or make them both conditional."
                .TrimInside(),
                action);
        }

        [TestMethod]
        public void RegisterClosedUnconditional_AfterAClosedConditionalRegistrationForTheDifferentClosedVersionHasBeenMade_Succeeds()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.RegisterConditional(typeof(IGeneric<int>), typeof(IntGenericType), c => true);

            // Act
            container.Register<IGeneric<float>, GenericType<float>>();
        }

        [TestMethod]
        public void RegisterGeneric_ForUnconstraintedTypeAfterAConditionalRegistrationForTheSameServiceTypeHasBeenMade_ThrowsAnExpressiveException()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.RegisterConditional(typeof(IGeneric<>), typeof(GenericClassType<>), Lifestyle.Singleton, c => true);

            // Act
            // DefaultGenericType<T> applies to every T, so it will overlap with the previous registration.
            Action action = () => container.Register(typeof(IGeneric<>), typeof(DefaultGenericType<>));

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<InvalidOperationException>(@"
                There is already a conditional registration for IGeneric<T> (with implementation 
                GenericClassType<TClass>) that overlaps with the registration for DefaultGenericType<T> that 
                you are trying to make. This new registration would cause ambiguity, because both 
                registrations would be used for the same closed service types. Either remove one of the 
                registrations or make them both conditional."
                .TrimInside(),
                action);
        }

        [TestMethod]
        public void RegisterConditionalGeneric_AfterAnUnconditionalUnconstraintRegistrationForTheSameTypeHasBeenMade_ThrowsAnExpressiveException()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Register(typeof(IGeneric<>), typeof(GenericType<>));

            // Act
            Action action = () => container.RegisterConditional(typeof(IGeneric<>), typeof(GenericClassType<>), 
                c => true);

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<InvalidOperationException>(@"
                There is already a registration for IGeneric<T> (with implementation GenericType<T>) that
                overlaps with the conditional registration for GenericClassType<TClass> that you are trying 
                to make. This new registration would cause ambiguity, because both registrations would be 
                used for the same closed service types. Either remove one of the registrations or make them 
                both conditional."
                .TrimInside(),
                action);
        }

        [TestMethod]
        public void RegisterConditionalGeneric_ForConstraintTypeAfterAnUnconditionalConstraintRegistrationForTheSameImplementationTypeHasBeenMade_ThrowsAnExpressiveException()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Register(typeof(IGeneric<>), typeof(GenericClassType<>));

            // Act
            // Although we skip checks for types with type constraints, these two registrations use the same
            // implementation type and this will always cause overlap.
            Action action = () => container.RegisterConditional(typeof(IGeneric<>), typeof(GenericClassType<>), 
                c => true);

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<InvalidOperationException>(@"
                There is already a registration for IGeneric<T> (with implementation GenericClassType<TClass>) 
                that overlaps with the conditional registration for GenericClassType<TClass> that you are 
                trying to make. This new registration would cause ambiguity, because both registrations would
                be used for the same closed service types. Either remove one of the registrations or make 
                them both conditional."
                .TrimInside(),
                action);
        }

        [TestMethod]
        public void RegisterConditionalGeneric_DoneTwiceConditionallyForTheExactSameImplementationType_ThrowsAnExpressiveException()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.RegisterConditional(typeof(IGeneric<>), typeof(GenericType<>), Lifestyle.Singleton, 
                c => false);

            // Act
            Action action = () => container.RegisterConditional(typeof(IGeneric<>), typeof(GenericType<>),
                Lifestyle.Singleton, c => false);

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<InvalidOperationException>(@"
                There is already a conditional registration for IGeneric<T> (with implementation 
                GenericType<T>) that overlaps with the conditional registration for GenericType<T> that you 
                are trying to make. This new registration would cause ambiguity, because both registrations 
                would be used for the same closed service types. You can merge both registrations into a 
                single conditional registration and combine both predicates into one single predicate."
                .TrimInside(),
                action);
        }

        [TestMethod]
        public void RegisterConditional_DoneTwiceConditionallyForTheExactSameImplementationType_ThrowsAnExpressiveException()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.RegisterConditional<ILogger, NullLogger>(Lifestyle.Singleton, c => false);

            // Act
            Action action = () => container.RegisterConditional<ILogger, NullLogger>(Lifestyle.Singleton, c => false);

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<InvalidOperationException>(@"
                There is already a conditional registration for ILogger (with implementation 
                NullLogger) that overlaps with the conditional registration for NullLogger that you are trying
                to make. This new registration would cause ambiguity, because both registrations would be used 
                for the same closed service types. You can merge both registrations into a single conditional 
                registration and combine both predicates into one single predicate."
                .TrimInside(),
                action);
        }

        [TestMethod]
        public void RegisterConditionalGeneric_AfterAnUnconditionalRegistrationForTypeWithTypeConstraints_Succeeds()
        {
            // Arrange
            var container = ContainerFactory.New();

            // GenericClassType<T> contains a type constraint, and it can therefore not be applied to every
            // possible IGeneric<T>. This makes the following conditional registration valid.
            container.Register(typeof(IGeneric<>), typeof(GenericClassType<>));

            // Act
            container.RegisterConditional(typeof(IGeneric<>), typeof(GenericType<>), c => true);
        }

        [TestMethod]
        public void RegisterConditionalGeneric_AfterAnUnconditionalRegistrationWhereImplementationCanBeAppliedToEveryServiceType_ThrowsAnExpressiveException()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Register(typeof(IClassConstraintedGeneric<>), typeof(ClassConstraintedGeneric<>));

            // Act
            // ConstraintedGeneric<T> can be applied to every possible IConstraintedGeneric<T> and that means
            // that the following conditional registration will always overlap with the previous and is 
            // therefore invalid.
            Action action = () => container.RegisterConditional(typeof(IClassConstraintedGeneric<>),
                typeof(ClassConstraintedGeneric2<>),
                c => true);

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<InvalidOperationException>(@"
                There is already a registration for IClassConstraintedGeneric<T> (with implementation 
                ClassConstraintedGeneric<T>) that overlaps with the conditional registration for 
                ClassConstraintedGeneric2<T> that you are trying to make. This new registration would cause 
                ambiguity, because both registrations would be used for the same closed service types. Either 
                remove one of the registrations or make them both conditional."
                .TrimInside(),
                action);
        }

        [TestMethod]
        public void RegisterConditional_RegisteredSingletonWithPredicateTrue_InjectsSameInstanceInAllConsumers()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.RegisterConditional(typeof(ILogger), typeof(NullLogger), Lifestyle.Singleton, c => true);

            // Act
            var service1 = container.GetInstance<ServiceWithDependency<ILogger>>();
            var service2 = container.GetInstance<AnotherServiceWithDependency<ILogger>>();

            // Assert
            Assert.AreSame(service1.Dependency, service2.Dependency);
        }

        [TestMethod]
        public void RegisterConditional_RegisteredGenericSingletonWithPredicateTrue_InjectsSameInstanceInAllConsumers()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.RegisterConditional(typeof(IGeneric<>), typeof(GenericType<>), Lifestyle.Singleton, c => true);

            // Act
            var service1 = container.GetInstance<ServiceWithDependency<IGeneric<int>>>();
            var service2 = container.GetInstance<AnotherServiceWithDependency<IGeneric<int>>>();

            // Assert
            Assert.AreSame(service1.Dependency, service2.Dependency);
        }

        [TestMethod]
        public void RegisterConditional_TwoConditionalRegistrationsWithOneFallback_InjectsTheExpectedInstancesIntoTheConsumers()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.RegisterConditional(typeof(ILogger), typeof(NullLogger), Lifestyle.Singleton,
                c => c.Consumer.ImplementationType == typeof(ServiceWithDependency<ILogger>));

            // Fallback registration
            container.RegisterConditional(typeof(ILogger), typeof(ConsoleLogger), Lifestyle.Singleton, c => !c.Handled);

            // Act
            var service1 = container.GetInstance<ServiceWithDependency<ILogger>>();
            var service2 = container.GetInstance<ServiceWithDependency<ILogger>>();

            var service3 = container.GetInstance<AnotherServiceWithDependency<ILogger>>();
            var service4 = container.GetInstance<AnotherServiceWithDependency<ILogger>>();

            // Assert
            Assert.AreSame(service1.Dependency, service2.Dependency);
            AssertThat.IsInstanceOfType(typeof(NullLogger), service1.Dependency);

            Assert.AreSame(service3.Dependency, service4.Dependency);
            AssertThat.IsInstanceOfType(typeof(ConsoleLogger), service3.Dependency);
        }

        [TestMethod]
        public void GetInstance_ConsumerDependingOnConditionalRegistrationThatDoesNotGetInjected_ThrowsExpectedExceptions()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.RegisterConditional(typeof(ILogger), typeof(NullLogger), Lifestyle.Singleton, c => false);

            // Act
            Action action = () => container.GetInstance<ServiceWithDependency<ILogger>>();

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<ActivationException>(@"
                The constructor of type ServiceWithDependency<ILogger> contains the parameter with name 
                'dependency' and type ILogger that is not registered. Please ensure ILogger is registered, or 
                change the constructor of ServiceWithDependency<ILogger>. 1 conditional registration for ILogger 
                exists, but its supplied predicate didn't return true when provided with the contextual 
                information for ServiceWithDependency<ILogger>."
                .TrimInside(),
                action);
        }

        [TestMethod]
        public void GetInstance_ConsumerDependingOnConditionalGenericRegistrationThatDoesNotGetInjected_ThrowsExpectedExceptions()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Register(typeof(IGeneric<>), typeof(GenericClassType<>));
            container.RegisterConditional(typeof(IGeneric<>), typeof(GenericType<>), Lifestyle.Singleton, c => false);
            container.RegisterConditional(typeof(IGeneric<>), typeof(GenericClassType2<>), Lifestyle.Singleton, c => false);

            // Act
            Action action = () => container.GetInstance<ServiceWithDependency<IGeneric<int>>>();

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<ActivationException>(@"
                The constructor of type ServiceWithDependency<IGeneric<Int32>> contains the parameter with 
                name 'dependency' and type IGeneric<Int32> that is not registered."
                .TrimInside(),
                action);

            AssertThat.ThrowsWithExceptionMessageContains<ActivationException>(@"
                1 conditional registration for IGeneric<T> exists that is applicable to IGeneric<Int32>, 
                but its supplied predicate didn't return true when provided with the contextual information 
                for ServiceWithDependency<IGeneric<Int32>>."
                .TrimInside(),
                action);
        }

        [TestMethod]
        public void GetInstance_ConsumerDependingOnConditionalGenericRegistrationThatDoesNotGetInjected2_ThrowsExpectedExceptions()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.RegisterConditional(typeof(IGeneric<>), typeof(GenericType<>), Lifestyle.Singleton, c => false);
            container.RegisterConditional(typeof(IGeneric<>), typeof(GenericClassType<>), Lifestyle.Singleton, c => false);

            // Act
            Action action = () => container.GetInstance<ServiceWithDependency<IGeneric<string>>>();

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<ActivationException>(@"
                2 conditional registrations for IGeneric<T> exist that are applicable to IGeneric<String>, but
                none of the supplied predicates returned true when provided with the contextual information for 
                ServiceWithDependency<IGeneric<String>>."
                .TrimInside(),
                action);
        }

        [TestMethod]
        public void GetInstance_ConsumerDependingOnConditionalRegistrationsThatDoNotGetInjected_ThrowsExpectedExceptions()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.RegisterConditional(typeof(ILogger), typeof(NullLogger), Lifestyle.Singleton, c => false);
            container.RegisterConditional(typeof(ILogger), typeof(ConsoleLogger), Lifestyle.Singleton, c => false);

            // Act
            Action action = () => container.GetInstance<ServiceWithDependency<ILogger>>();

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<ActivationException>(@"
                2 conditional registrations for ILogger exist, but none of the supplied predicates returned 
                true when provided with the contextual information for ServiceWithDependency<ILogger>."
                .TrimInside(),
                action);
        }

        [TestMethod]
        public void GetInstance_SingletonDecorator_GetsItsOwnSingletonPerRegistration()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.RegisterConditional<IPlugin, PluginImpl>(Lifestyle.Singleton,
                c => c.Consumer.ImplementationType == typeof(ServiceWithDependency<IPlugin>));

            container.RegisterConditional<IPlugin, PluginImpl2>(Lifestyle.Singleton, c => !c.Handled);

            container.RegisterDecorator<IPlugin, PluginDecorator>(Lifestyle.Singleton);

            // Act
            var decorator1 = container.GetInstance<ServiceWithDependency<IPlugin>>().Dependency;
            var decorator2 = container.GetInstance<ServiceWithDependency<IPlugin>>().Dependency;

            var anotherDecorator1 = container.GetInstance<AnotherServiceWithDependency<IPlugin>>().Dependency;
            var anotherDecorator2 = container.GetInstance<AnotherServiceWithDependency<IPlugin>>().Dependency;

            // Assert
            AssertThat.IsInstanceOfType(typeof(PluginDecorator), decorator1, 
                "Service was expected to be decorated");
            AssertThat.IsInstanceOfType(typeof(PluginDecorator), anotherDecorator1, 
                "Service was expected to be decorated");

            Assert.AreNotSame(decorator1, anotherDecorator1, @"
                Each conditional registration should get its own decorator, because such decorator can only
                point at one particular dependency.");

            Assert.AreSame(decorator1, decorator2, "Decorator was expected to be singleton");
            Assert.AreSame(anotherDecorator1, anotherDecorator2, "Decorator was expected to be singleton");

            AssertThat.IsInstanceOfType(typeof(PluginImpl), ((PluginDecorator)decorator1).Decoratee);
            AssertThat.IsInstanceOfType(typeof(PluginImpl2), ((PluginDecorator)anotherDecorator1).Decoratee);
        }

        [TestMethod]
        public void GetInstance_GenericConditionalRegistrationThatOverlapsWithClosedRegistration_ThrowsExpectedException()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Register<IGeneric<int>, IntGenericType>();

            // Conditional that overlaps with previous registration.
            container.RegisterConditional(typeof(IGeneric<>), typeof(GenericType<>), c => true);

            // Act
            Action action = () => container.GetInstance<IGeneric<int>>();

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<ActivationException>(@"
                Multiple applicable registrations found for IGeneric<Int32>. The applicable registrations are
                (1) the unconditional closed generic registration for IGeneric<Int32> using IntGenericType and
                (2) the conditional open generic registration for IGeneric<T> using GenericType<T>.
                If your goal is to make one registration a fallback in case another registration is not
                applicable, make the fallback registration last using RegisterConditional and make sure
                the supplied predicate returns false in case the Handled property is true."
                .TrimInside(),
                action);
        }

        [TestMethod]
        public void GetInstance_GenericConditionalRegistrationWithFallbackBehavior_ReturnsTheClosedRegistration()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Register<IGeneric<int>, IntGenericType>();

            // Conditional fallback.
            container.RegisterConditional(typeof(IGeneric<>), typeof(GenericType<>), c => !c.Handled);

            // Act
            var instance = container.GetInstance<IGeneric<int>>();

            // Assert
            AssertThat.IsInstanceOfType(typeof(IntGenericType), instance);
        }

        [TestMethod]
        public void GetInstance_GenericConditionalRegistrationWithFallbackBehaviorRegisteredBeforeClosed_Throws()
        {
            // Arrange
            var container = ContainerFactory.New();

            // Conditional fallback before the closed registration.
            container.RegisterConditional(typeof(IGeneric<>), typeof(GenericType<>), c => !c.Handled);

            container.Register<IGeneric<int>, IntGenericType>();

            // Act
            Action action = () => container.GetInstance<IGeneric<int>>();

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<ActivationException>(
                "Multiple applicable registrations found for IGeneric<Int32>",
                action);

            AssertThat.ThrowsWithExceptionMessageContains<ActivationException>(
                "make the fallback registration last using RegisterConditional and make sure",
                action);
        }

        [TestMethod]
        public void GetInstance_GenericConditionalRegistrationThatNonoverlappingClosedConditionalRegistration_Succeeds()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.RegisterConditional<IGeneric<int>, IntGenericType>(c => false);

            // Conditional generic registration that doesn't overlap, because the previous conditional uses c => false
            container.RegisterConditional(typeof(IGeneric<>), typeof(GenericType<>), c => true);

            // Act
            container.GetInstance<IGeneric<int>>();
        }

        [TestMethod]
        public void GetInstance_NonRootTypeGenericConditionalRegistrationThatNonoverlappingClosedConditionalRegistration_Succeeds()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.RegisterConditional<IGeneric<int>, IntGenericType>(c => false);

            // Conditional generic registration that doesn't overlap, because the previous conditional uses c => false
            container.RegisterConditional(typeof(IGeneric<>), typeof(GenericType<>), c => true);

            // Act
            container.GetInstance<ServiceWithDependency<IGeneric<int>>>();
        }

        [TestMethod]
        public void Verify_WithConditionalRegistration_VerifiesTheConditionalRegistrationAsWell()
        {
            // Arrange
            var container = ContainerFactory.New();

            // ILogger is not registered
            container.RegisterConditional<IPlugin, PluginWithDependency<ILogger>>(c => false);

            // Act
            Action action = () => container.Verify();

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<InvalidOperationException>(@"
                The constructor of type PluginWithDependency<ILogger> contains the parameter with name 
                'dependency' and type ILogger that is not registered."
                .TrimInside(),
                action);
        }

        [TestMethod]
        public void GetInstance_MultipleApplicableConditionalNonGenericRegistrations_ThrowsExpectedException()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.RegisterConditional(typeof(ILogger), typeof(NullLogger), Lifestyle.Singleton, c => true);
            container.RegisterConditional(typeof(ILogger), typeof(ConsoleLogger), Lifestyle.Singleton, c => true);

            // Act
            Action action = () => container.GetInstance<ServiceWithDependency<ILogger>>();

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<ActivationException>(@"
                Multiple applicable registrations found for ILogger. The applicable registrations are
                (1) the conditional registration for ILogger using NullLogger and
                (2) the conditional registration for ILogger using ConsoleLogger.
                If your goal is to make one registration a fallback in case another registration is not
                applicable, make the fallback registration last using RegisterConditional and make sure
                the supplied predicate returns false in case the Handled property is true."
               .TrimInside(),
               action);
        }

        [TestMethod]
        public void RegisterConditionalFactory_AllowOverridingRegistrations_NotSupported()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Options.AllowOverridingRegistrations = true;

            // Act
            Action action = () => container.RegisterConditional(typeof(IGeneric<>),
                c => typeof(GenericType<>),
                Lifestyle.Singleton,
                c => true);

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<NotSupportedException>(
                "not supported when AllowOverridingRegistrations",
                action);
        }

        [TestMethod]
        public void GetInstance_ResolvingDifferentConsumersDependingOnConditionalTypeWithFactory_EachComponentGetsItsSpecificDependency()
        {
            // Arrange
            var container = new Container();

            container.RegisterConditional(
                typeof(ILogger),
                c => typeof(Logger<>).MakeGenericType(c.Consumer.ImplementationType),
                Lifestyle.Singleton,
                c => true);

            // Act
            var a = container.GetInstance<ServiceDependingOn<ILogger>>();
            var b = container.GetInstance<ComponentDependingOn<ILogger>>();

            // Assert
            AssertThat.IsInstanceOfType(typeof(Logger<ServiceDependingOn<ILogger>>), a.Dependency);
            AssertThat.IsInstanceOfType(typeof(Logger<ComponentDependingOn<ILogger>>), b.Dependency);
        }

        [TestMethod]
        public void GetInstance_ConditionalTypeFactoryReturnsSameTypeForSingletonRegistration_DependencyIsAlwaysSameInstance()
        {
            // Arrange
            var container = new Container();

            container.RegisterConditional(
                typeof(ILogger),
                c => typeof(Logger<int>),
                Lifestyle.Singleton,
                c => true);

            // Act
            var a = container.GetInstance<ServiceDependingOn<ILogger>>();
            var b = container.GetInstance<ComponentDependingOn<ILogger>>();

            // Assert
            Assert.AreSame(a.Dependency, b.Dependency);
        }

        [TestMethod]
        public void GetInstance_ConditionalSingletonWithMutlipleClosedMappingAtSameImplementationThroughFactory_InjectsSameInstanceForDifferentClosedVersions()
        {
            // Arrange
            var container = new Container();

            // This is a bit nasty: IntAndFloatGeneric implements IGeneric<int> and IGeneric<float>.
            container.RegisterConditional(
                typeof(IGeneric<>),
                c => typeof(IntAndFloatGeneric),
                Lifestyle.Singleton,
                c => true);

            // Act
            var a = container.GetInstance<ServiceDependingOn<IGeneric<int>>>();
            var b = container.GetInstance<ComponentDependingOn<IGeneric<float>>>();

            // Assert
            Assert.AreSame(a.Dependency, b.Dependency,
                "Since both dependencies are IntAndFloatGeneric<object> and it is registered as singleton " +
                ", there should only be one single instance.");
        }

        [TestMethod]
        public void RegisterConditionalFactory_PredicateContext_ServiceTypeIsClosedImplentation()
        {
            // Arrange
            Type actualServiceType = null;

            var container = ContainerFactory.New();
            container.RegisterConditional(
                typeof(IOpenGenericWithPredicate<>),
                c => typeof(OpenGenericWithPredicate1<>),
                Lifestyle.Transient, c =>
                {
                    actualServiceType = c.ServiceType;
                    return true;
                });

            // Act
            var result = container.GetInstance<IOpenGenericWithPredicate<int>>();

            // Assert
            Assert.IsNotNull(actualServiceType, "Predicate was not called");
            Assert.IsFalse(actualServiceType.ContainsGenericParameter(), "ServiceType should be a closed type");
        }

        [TestMethod]
        public void RegisterConditionalFactory_FactoryReturningOpenGenericType_WorksCorrectly()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.RegisterConditional(typeof(IOpenGenericWithPredicate<>),
                c => typeof(OpenGenericWithPredicate1<>),
                Lifestyle.Transient,
                c => true);

            // Act
            var result = container.GetInstance<IOpenGenericWithPredicate<int>>();

            // Assert
            AssertThat.IsInstanceOfType(typeof(OpenGenericWithPredicate1<int>), result);
        }

        [TestMethod]
        public void RegisterConditionalFactory_PredicateCallingImplementationType_CallsFactoryToConstructClosedImplementationType()
        {
            // Arrange
            Type actualImplementationType = null;

            var container = ContainerFactory.New();
            container.RegisterConditional(typeof(IOpenGenericWithPredicate<>),
                c => typeof(OpenGenericWithPredicate1<>),
                Lifestyle.Transient, c =>
                {
                    actualImplementationType = c.ImplementationType;
                    return true;
                });

            // Act
            var result = container.GetInstance<IOpenGenericWithPredicate<int>>();

            // Assert
            Assert.AreEqual(actualImplementationType, typeof(OpenGenericWithPredicate1<int>));
        }

        [TestMethod]
        public void RegisterConditionalFactory_FactoryReturningAnOpenGenericTypeThatCanNotBeAppliedToRequestedService_ImplementationTypeIsNull()
        {
            // Arrange
            Type actualImplementationType = typeof(object); // we just assign something invalid here.

            var container = ContainerFactory.New();
            container.RegisterConditional(typeof(IOpenGenericWithPredicate<>),
                c => typeof(OpenGenericWithPredicateWithClassConstraint<>),
                Lifestyle.Transient, c =>
                {
                    actualImplementationType = c.ImplementationType;
                    return true;
                });

            container.RegisterConditional(typeof(IOpenGenericWithPredicate<>),
                typeof(OpenGenericWithPredicate1<>),
                Lifestyle.Transient,
                c => true);

            // Act
            var result = container.GetInstance<IOpenGenericWithPredicate<int>>();

            // Assert
            Assert.IsNull(actualImplementationType, @"
                Since the returned OpenGenericWithPredicateWithClassConstraint<T> can't be applied to
                IOpenGenericWithPredicate<int> (due to its type constraints), the ImplementationType 
                property is expected to return null.");
        }

        [TestMethod]
        public void RegisterConditionalFactoryGeneric_PredicateReturningFalse_FactoryDoesNotGetCalled()
        {
            bool predicateCalled = false;

            // Arrange
            var container = ContainerFactory.New();
            container.RegisterConditional(typeof(IOpenGenericWithPredicate<>),
                c =>
                {
                    Assert.Fail("Factory is not expected to be called when the predicate returns false." +
                        "This allows the user to check generic type constrains in the predicate and " +
                        "correctly build a type in the factory without having to return some kind of dummy " +
                        "type.");
                    return null;
                },
                Lifestyle.Transient,
                c =>
                {
                    predicateCalled = true;
                    return false;
                });

            // Register a second conditional that allows to be selected, because the first returns false.
            container.RegisterConditional(typeof(IOpenGenericWithPredicate<>),
                typeof(OpenGenericWithPredicate1<>),
                Lifestyle.Singleton,
                c => true);

            // Act
            var result = container.GetInstance<IOpenGenericWithPredicate<int>>();

            // Assert
            Assert.IsTrue(predicateCalled);
        }

        [TestMethod]
        public void RegisterConditionalFactory_PredicateReturningFalse_FactoryDoesNotGetCalled()
        {
            bool predicateCalled = false;

            // Arrange
            var container = ContainerFactory.New();
            container.RegisterConditional(typeof(ILogger),
                c =>
                {
                    Assert.Fail("Factory is not expected to be called when the predicate returns false." +
                        "This allows the user to check generic type constrains in the predicate and " +
                        "correctly build a type in the factory without having to return some kind of dummy " +
                        "type.");
                    return null;
                },
                Lifestyle.Transient,
                c =>
                {
                    predicateCalled = true;
                    return false;
                });

            // Register a second conditional that allows to be selected, because the first returns false.
            container.RegisterConditional<ILogger, NullLogger>(c => true);

            // Act
            var result = container.GetInstance<ServiceDependingOn<ILogger>>();

            // Assert
            Assert.IsTrue(predicateCalled);
        }

        [TestMethod]
        public void RegisterConditionalFactory_TwoEquivalentImplementationsOfTheSameInterfaceWithOverlappingPredicate_ThrowsException2()
        {
            // Arrange
            var container = ContainerFactory.New();
            container.RegisterConditional(typeof(IOpenGenericWithPredicate<>),
                c => typeof(OpenGenericWithPredicate1<>),
                Lifestyle.Transient,
                c => c.ImplementationType.GetGenericArguments().Single() == typeof(int));

            container.RegisterConditional(typeof(IOpenGenericWithPredicate<>),
                c => typeof(OpenGenericWithPredicate2<>),
                Lifestyle.Transient,
                c => c.ImplementationType.GetGenericArguments().Single().Namespace.StartsWith("System"));

            // Act
            container.GetInstance<IOpenGenericWithPredicate<long>>();
            Action action = () => container.GetInstance<IOpenGenericWithPredicate<int>>();

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<ActivationException>(
                "Multiple applicable registrations found for IOpenGenericWithPredicate<Int32>",
                action,
                "GetInstance should fail because the framework should detect that more than one " +
                "implementation of the requested service.");
        }

        [TestMethod]
        public void GetInstance_RegisterTypeFactoryReturningAPartialOpenGenericType_WorksLikeACharm()
        {
            // Arrange
            var container = new Container();

            // Here we make a partial open-generic type by filling in the TUnresolved.
            container.RegisterConditional(typeof(IOpenGenericWithPredicate<>),
                c => typeof(OpenGenericWithUnresolvableArgument<,>)
                    .MakePartialOpenGenericType(
                        secondArgument: typeof(double)),
                Lifestyle.Transient,
                c => true);

            // Act
            var service = container.GetInstance<ServiceDependingOn<IOpenGenericWithPredicate<string>>>();

            var result = service.Dependency;

            // Assert
            AssertThat.IsInstanceOfType(typeof(OpenGenericWithUnresolvableArgument<string, double>), result);
        }

        [TestMethod]
        public void GetInstance_WithClosedGenericServiceAndFactoryReturningIncompatibleClosedImplementation_FailsWithExpectedException()
        {
            // Arrange
            var container = new Container();

            container.RegisterConditional(typeof(IOpenGenericWithPredicate<>),
                c => typeof(OpenGenericWithPredicate1<string>),
                Lifestyle.Transient,
                c => true);

            // Act
            Action action = () => container.GetInstance<IOpenGenericWithPredicate<object>>();

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<ActivationException>(@"
                The registered type factory returned type OpenGenericWithPredicate1<String> which
                does not implement IOpenGenericWithPredicate<Object>"
                .TrimInside(),
                action);
        }

        [TestMethod]
        public void GetInstance_RegisterDecoratorWithFactoryReturningTypeWithMultiplePublicConstructors_ThrowsExceptedException()
        {
            // Arrange
            string expectedMessage = "it should have only one public constructor";

            var container = new Container();

            container.RegisterConditional(typeof(IOpenGenericWithPredicate<>),
                c => typeof(OpenGenericWithPredicateWithMultipleCtors<>),
                Lifestyle.Transient,
                c => true);

            // Act
            Action action = () => container.GetInstance<IOpenGenericWithPredicate<int>>();

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<ActivationException>(expectedMessage, action);
        }

        [TestMethod]
        public void GetInstance_RegisterTypeWithNonGenericServiceAndFactoryReturningAnOpenGenericType_ThrowsExpectedException()
        {
            // Arrange
            var container = new Container();

            container.RegisterConditional(typeof(IDisposable),
                c => typeof(DisposableOpenGenericWithPredicate<>),
                Lifestyle.Transient,
                c => true);

            // Act
            Action action = () => container.GetInstance<IDisposable>();

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<ActivationException>(
                @"The registered type factory returned open generic type DisposableOpenGenericWithPredicate<T> 
                while the registered service type IDisposable is not generic, making it impossible for a 
                closed generic type to be constructed"
                .TrimInside(),
                action);
        }

        [TestMethod]
        public void GetInstance_RegisterTypeWithFactoryReturningTypeWithUnresolvableArgument_ThrowsExceptedException()
        {
            // Arrange
            var container = new Container();

            container.RegisterConditional(typeof(IOpenGenericWithPredicate<>),
                c => typeof(OpenGenericWithUnresolvableArgument<,>),
                Lifestyle.Transient,
                c => true);

            // Act
            Action action = () => container.GetInstance<IOpenGenericWithPredicate<int>>();

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<ActivationException>(
                typeof(OpenGenericWithUnresolvableArgument<,>).ToFriendlyName() +
                " contains unresolvable type arguments.", action);
        }

        [TestMethod]
        public void RegisterWithFactory_FactoryThatReturnsNull_ThrowsExpectedExceptionWhenResolving()
        {
            // Arrange
            var container = new Container();

            container.RegisterConditional(typeof(IOpenGenericWithPredicate<>),
                c => null,
                Lifestyle.Transient,
                c => true);

            // Act
            Action action = () => container.GetInstance<IOpenGenericWithPredicate<int>>();

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<InvalidOperationException>(
                "The type factory delegate that was registered for service type " +
                "IOpenGenericWithPredicate<T> returned null.",
                action);
        }

        [TestMethod]
        public void GetInstance_ConditionalNonGenericRegistrationWithFactory_InjectsTheExpectedImplementation()
        {
            // Arrange
            var container = new Container();

            container.RegisterConditional(typeof(ILogger),
                c => typeof(Logger<>).MakeGenericType(c.Consumer.ImplementationType),
                Lifestyle.Singleton,
                c => true);

            // Act
            ILogger logger = container.GetInstance<ServiceDependingOn<ILogger>>().Dependency;

            // Assert
            AssertThat.IsInstanceOfType(typeof(Logger<ServiceDependingOn<ILogger>>), logger);
        }

        [TestMethod]
        public void GetInstance_ConditionalNonGenericRegistrationWithFactoryInjectedIntoMultipleConsumersAsSingleton_AlwaysInjectSameInstance()
        {
            // Arrange
            var container = new Container();

            container.RegisterConditional(typeof(ILogger),
                c => typeof(Logger<int>),
                Lifestyle.Singleton,
                c => true);

            // Act
            ILogger logger1 = container.GetInstance<ServiceDependingOn<ILogger>>().Dependency;
            ILogger logger2 = container.GetInstance<ComponentDependingOn<ILogger>>().Dependency;

            // Assert
            Assert.AreSame(logger1, logger2);
        }

        [TestMethod]
        public void GetInstance_ResolvingAbstractionForComponentThatDependsOnConditionalRegistration_ContextGetsSuppliedWithActualImplementation()
        {
            // Arrange
            PredicateContext context = null;

            var container = new Container();

            container.Options.PropertySelectionBehavior = new InjectAllProperties();

            container.Register<INonGenericService, ServiceWithProperty<ILogger>>();

            container.RegisterConditional<ILogger, Logger<int>>(c =>
            {
                context = c;
                return true;
            });

            // Act
            container.GetInstance<INonGenericService>();

            // Assert
            Assert.AreEqual(typeof(ServiceWithProperty<ILogger>), context.Consumer.ImplementationType);
        }

        [TestMethod]
        public void ConditionalRegistration_SupplyingOpenGenericServiceType_ThrowsExpectedException()
        {
            // Arrange
            var container = ContainerFactory.New();

            var registration = Lifestyle.Transient.CreateRegistration<GenericClassType<object>>(container);

            // Act
            Action action = () => container.RegisterConditional(typeof(IGeneric<>), registration, c => true);

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<ArgumentException>(
                "IGeneric<T> is an open generic type",
                action);
        }

        [TestMethod]
        public void GetInstance_ConditionalRegistration_Succeeds()
        {
            // Arrange
            var container = ContainerFactory.New();

            var registration = Lifestyle.Transient.CreateRegistration<NullLogger>(container);

            container.RegisterConditional(typeof(ILogger), registration, c => true);

            // Act
            container.GetInstance<ILogger>();
        }

        [TestMethod]
        public void ConditionalRegistration_RegisteringTheSameRegistrationTwice_ThrowsExpectedException()
        {
            // Arrange
            var container = ContainerFactory.New();

            var registration = Lifestyle.Transient.CreateRegistration<NullLogger>(container);

            container.RegisterConditional(typeof(ILogger), registration, c => true);

            // Act
            Action action = () => container.RegisterConditional(typeof(ILogger), registration, c => false);

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<InvalidOperationException>(
                "This new registration would cause ambiguity",
                action);
        }

        [TestMethod]
        public void ConditionalRegistration_RegisteringTwoRegistrationsForSameImplementationType_ThrowsExpectedException()
        {
            // Arrange
            var container = ContainerFactory.New();

            var registration1 = Lifestyle.Transient.CreateRegistration<NullLogger>(container);
            var registration2 = Lifestyle.Transient.CreateRegistration<NullLogger>(container);

            container.RegisterConditional(typeof(ILogger), registration1, c => true);

            // Act
            Action action = () => container.RegisterConditional(typeof(ILogger), registration2, c => false);

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<InvalidOperationException>(
                "This new registration would cause ambiguity",
                action);
        }

        [TestMethod]
        public void GetInstance_RegisteringTheTwoDifferentRegistrationsWithDelegate_Succeeds()
        {
            // Arrange
            var container = ContainerFactory.New();

            var registration1 = Lifestyle.Transient.CreateRegistration<ILogger>(() => new NullLogger(), container);
            var registration2 = Lifestyle.Transient.CreateRegistration<ILogger>(() => new NullLogger(), container);

            container.RegisterConditional(typeof(ILogger), registration1, c => false);
            container.RegisterConditional(typeof(ILogger), registration2, c => true);

            // Act
            container.GetInstance<ILogger>();
        }
        
        [TestMethod]
        public void ConditionalRegistration_ClosedGenericTypes_RegistrationsAreAppended1()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.RegisterConditional<IGeneric<int>, IntGenericType>(c => true);
            container.RegisterConditional<IGeneric<int>, GenericType<int>>(c => false);

            // Act
            var instance = container.GetInstance<IGeneric<int>>();

            // Assert
            AssertThat.IsInstanceOfType(typeof(IntGenericType), instance);
        }
        
        [TestMethod]
        public void ConditionalRegistration_ClosedGenericTypes_RegistrationsAreAppended2()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.RegisterConditional<IGeneric<int>, IntGenericType>(c => false);
            container.RegisterConditional<IGeneric<int>, GenericType<int>>(c => true);

            // Act
            var instance = container.GetInstance<IGeneric<int>>();

            // Assert
            AssertThat.IsInstanceOfType(typeof(GenericType<int>), instance);
        }

        [TestMethod]
        public void GetInstance_ResolvingRegisteredTypeWithStringDependencyRegisteredAsConditional_InjectsStringDependency()
        {
            // Arrange
            string expectedValue = "Some Value";

            var container = ContainerFactory.New();

            container.Options.DependencyInjectionBehavior =
                new VerficationlessInjectionBehavior(container.Options.DependencyInjectionBehavior);

            // Type to resolve is explicitly registered
            container.Register<ServiceDependingOn<string>>();

            RegisterConditionalConstant(container, expectedValue, c => true);

            // Act
            var service = container.GetInstance<ServiceDependingOn<string>>();

            // Assert
            Assert.AreEqual(expectedValue, service.Dependency);
        }

        [TestMethod]
        public void GetInstance_ResolvingUnregisteredTypeWithStringDependencyRegisteredAsConditional_InjectsStringDependency()
        {
            // Arrange
            string expectedValue = "Some Value";

            var container = ContainerFactory.New();

            container.Options.DependencyInjectionBehavior =
                new VerficationlessInjectionBehavior(container.Options.DependencyInjectionBehavior);

            RegisterConditionalConstant(container, expectedValue, c => true);

            // Act
            // ServiceDependingOn<T> is not registered here. This causes a different code path.
            var service = container.GetInstance<ServiceDependingOn<string>>();

            // Assert
            Assert.AreEqual(expectedValue, service.Dependency);
        }
        
        [TestMethod]
        public void GetInstance_PredicateContextForOpenGenericConsumer_ContainsTheExpectedConsumerInfo()
        {
            // Arrange
            InjectionConsumerInfo actualConsumer = null;

            var container = ContainerFactory.New();

            container.Register(typeof(IGeneric<>), typeof(GenericTypeWithLoggerDependency<>), Lifestyle.Transient);

            RegisterConditionalConstant<ILogger>(container, new NullLogger(), 
                c => { actualConsumer = c.Consumer; return true; });

            // Act
            container.GetInstance<IGeneric<int>>();

            // Assert
            Assert.IsTrue(actualConsumer.ImplementationType == typeof(GenericTypeWithLoggerDependency<int>));
        }

        [TestMethod]
        public void GetInstance_PredicateContextForGenericConsumer_ContainsTheExpectedConsumerInfo()
        {
            // Arrange
            InjectionConsumerInfo actualConsumer = null;

            var container = ContainerFactory.New();

            container.Register<IGeneric<int>, GenericTypeWithLoggerDependency<int>>(Lifestyle.Singleton);

            RegisterConditionalConstant<ILogger>(container, new NullLogger(), 
                c => { actualConsumer = c.Consumer; return true; });

            // Act
            container.GetInstance<IGeneric<int>>();

            // Assert
            AssertThat.AreEqual(typeof(GenericTypeWithLoggerDependency<int>), actualConsumer.ImplementationType);
        }

        // Tests: #346
        [TestMethod]
        public void GetInstance_ResolvingComponentWithConditionalDependency_CallsPredicateOnce()
        {
            // Arrange
            int expectedPredicateCallCount = 1;

            var container = new Container();

            container.Register<IX, XDependingOn<ILogger>>();

            int actualPredicateCallCount = 0;

            container.RegisterConditional<ILogger, Logger<int>>(c =>
            {
                actualPredicateCallCount++;
                return true;
            });

            // Act
            container.GetInstance<IX>();

            // Assert
            Assert.AreEqual(expectedPredicateCallCount, actualPredicateCallCount,
                "Under normal conditions, the predicate for a conditional registrations should run just " +
                "once per registration.");
        }

        // Tests: #346
        [TestMethod]
        public void GetInstance_ResolvingComponentWithPropertyForConditionalRegistration_CallsPredicateOnce()
        {
            // Arrange
            int expectedPredicateCallCount = 1;

            var container = new Container();

            container.Options.PropertySelectionBehavior = new InjectAllProperties();

            container.Register<INonGenericService, ServiceWithProperty<ILogger>>();

            int actualPredicateCallCount = 0;

            container.RegisterConditional<ILogger, Logger<int>>(c =>
            {
                actualPredicateCallCount++;
                return true;
            });

            // Act
            container.GetInstance<INonGenericService>();

            // Assert
            Assert.AreEqual(expectedPredicateCallCount, actualPredicateCallCount,
                "Under normal conditions, the predicate for a conditional registrations should run just " +
                "once per registration.");
        }

        // Tests: #346
        [TestMethod]
        public void GetInstance_ResolvingOpenGenericThatDependsOnConditionalRegistration_CallsPredicateOnce()
        {
            // Arrange
            int expectedPredicateCallCount = 1;

            var container = ContainerFactory.New();

            container.Register(typeof(IGeneric<>), typeof(GenericTypeWithLoggerDependency<>), Lifestyle.Transient);

            int actualPredicateCallCount = 0;

            RegisterConditionalConstant<ILogger>(container, new NullLogger(), c => 
            {
                actualPredicateCallCount++;
                return true;
            });

            // Act
            container.GetInstance<IGeneric<int>>();

            // Assert
            Assert.AreEqual(expectedPredicateCallCount, actualPredicateCallCount,
                "Under normal conditions, the predicate for a conditional registrations should run just " +
                "once per registration.");
        }

        [TestMethod]
        public void GetInstance_ConditionalRegistrationAndOpenGenericRegistrationForSameType_UseSameRegistration()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Register(typeof(Logger<>), typeof(Logger<>), Lifestyle.Singleton);

            container.RegisterConditional(typeof(ILogger),
                c => typeof(Logger<>).MakeGenericType(c.Consumer.ImplementationType),
                Lifestyle.Singleton,
                c => true);

            // Act
            var logger1 = container.GetInstance<ServiceDependingOn<ILogger>>().Dependency;
            var logger2 = container.GetInstance<Logger<ServiceDependingOn<ILogger>>>();

            // Assert
            Assert.IsTrue(ReferenceEquals(logger1, logger2),
                "Simple Injector is expected to reuse the same Registration instance for both registrations.");
        }

        private static void RegisterConditionalConstant<T>(Container container, T constant,
            Predicate<PredicateContext> predicate)
        {
            container.RegisterConditional(typeof(T),
                Lifestyle.Singleton.CreateRegistration(typeof(T), () => constant, container),
                predicate);
        }

        private sealed class InjectAllProperties : IPropertySelectionBehavior
        {
            public bool SelectProperty(Type implementationType, PropertyInfo property) => true;
        }

        private sealed class VerficationlessInjectionBehavior : IDependencyInjectionBehavior
        {
            private readonly IDependencyInjectionBehavior real;

            public VerficationlessInjectionBehavior(IDependencyInjectionBehavior real)
            {
                this.real = real;
            }

            public InstanceProducer GetInstanceProducer(InjectionConsumerInfo c, bool t) => 
                this.real.GetInstanceProducer(c, t);

            public void Verify(InjectionConsumerInfo consumer)
            {
                // Suppress verification.
            }
        }
    }
}