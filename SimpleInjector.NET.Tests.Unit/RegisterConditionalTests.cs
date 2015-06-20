namespace SimpleInjector.Tests.Unit
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Reflection;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>Tests for testing conditional registrations.</summary>
    [TestClass]
    public class RegisterConditionalTests
    {
        [TestMethod]
        public void RegisterConditional_AllowOverridingRegistrations_NotSupported()
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

            // Assert
        }

        [TestMethod]
        public void RegisterOpenGeneric_PredicateContext_ServiceTypeIsClosedImplentation()
        {
            bool called = false;

            // Arrange
            var container = ContainerFactory.New();
            container.RegisterConditional(typeof(IOpenGenericWithPredicate<>), typeof(OpenGenericWithPredicate1<>),
                Lifestyle.Transient, c =>
                {
                    Assert.IsFalse(c.ServiceType.ContainsGenericParameter(), "ServiceType should be a closed type");

                    called = true;
                    return true;
                });

            // Act
            var result = container.GetInstance<IOpenGenericWithPredicate<int>>();

            // Assert
            Assert.IsTrue(called, "Predicate was not called");
        }

        [TestMethod]
        public void RegisterOpenGeneric_PredicateContext_ImplementationTypeIsClosedImplentation()
        {
            bool called = false;

            // Arrange
            var container = ContainerFactory.New();
            container.RegisterConditional(typeof(IOpenGenericWithPredicate<>), typeof(OpenGenericWithPredicate1<>),
                Lifestyle.Transient, c =>
                {
                    Assert.IsFalse(c.ImplementationType.ContainsGenericParameter(), "ImplementationType should be a closed type");

                    called = true;
                    return true;
                });

            // Act
            var result = container.GetInstance<IOpenGenericWithPredicate<int>>();

            // Assert
            Assert.IsTrue(called, "Predicate was not called");
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
            bool handled = false;

            // Arrange
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
            Action action = () => container.RegisterConditional(typeof(IGeneric<>), typeof(GenericClassType<>), c => true);

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<InvalidOperationException>(@"
                There is already a registration for IGeneric<T> (with implementation GenericType<T>) that
                overlaps with the registration for GenericClassType<TClass> that you are trying to make. This 
                new registration would cause ambiguity, because both registrations would be used for the same
                closed service types. Either remove one of the registrations or make them both conditional."
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
            Action action = () => container.RegisterConditional(typeof(IGeneric<>), typeof(GenericClassType<>), c => true);

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<InvalidOperationException>(@"
                There is already a registration for IGeneric<T> (with implementation GenericClassType<TClass>) 
                that overlaps with the registration for GenericClassType<TClass> that you are trying to make. 
                This new registration would cause ambiguity, because both registrations would be used for the 
                same closed service types. Either remove one of the registrations or make them both 
                conditional."
                .TrimInside(),
                action);
        }
        
        [TestMethod]
        public void RegisterConditionalGeneric_DoneTwiceConditionallyForTheExactSameImplementationType_ThrowsAnExpressiveException()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.RegisterConditional(typeof(IGeneric<>), typeof(GenericType<>), Lifestyle.Singleton, c => false);

            // Act
            Action action = () => container.RegisterConditional(typeof(IGeneric<>), typeof(GenericType<>), 
                Lifestyle.Singleton, c => false);

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<InvalidOperationException>(@"
                There is already a conditional registration for IGeneric<T> (with implementation 
                GenericType<T>) that overlaps with the registration for GenericType<T> that you are trying to 
                make. This new registration would cause ambiguity, because both registrations would be used 
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

            container.Register(typeof(IConstraintedGeneric<>), typeof(ConstraintedGeneric<>));

            // Act
            // ConstraintedGeneric<T> can be applied to every possible IConstraintedGeneric<T> and that means
            // that the following conditional registration will always overlap with the previous and is 
            // therefore invalid.
            Action action = () => container.RegisterConditional(typeof(IConstraintedGeneric<>),
                typeof(ConstraintedGeneric2<>),
                c => true);

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<InvalidOperationException>(@"
                There is already a registration for IConstraintedGeneric<T> (with implementation 
                ConstraintedGeneric<T>) that overlaps with the registration for ConstraintedGeneric2<T> that 
                you are trying to make. This new registration would cause ambiguity, because both 
                registrations would be used for the same closed service types. Either remove one of the 
                registrations or make them both conditional."
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
                c => c.Consumer.ServiceType == typeof(ServiceWithDependency<ILogger>));

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

            container.RegisterConditional(typeof(IGeneric<>), typeof(GenericType<>), Lifestyle.Singleton, c => false);

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
            container.RegisterConditional(typeof(ILogger), typeof(NullLogger), Lifestyle.Singleton, c => false);

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
                c => c.Consumer.ServiceType == typeof(ServiceWithDependency<IPlugin>));

            container.RegisterConditional<IPlugin, PluginImpl2>(Lifestyle.Singleton, c => !c.Handled);

            container.RegisterDecorator<IPlugin, PluginDecorator>(Lifestyle.Singleton);
            
            // Act
            var decorator1 = container.GetInstance<ServiceWithDependency<IPlugin>>().Dependency;
            var decorator2 = container.GetInstance<ServiceWithDependency<IPlugin>>().Dependency;

            var anotherDecorator1 = container.GetInstance<AnotherServiceWithDependency<IPlugin>>().Dependency;
            var anotherDecorator2 = container.GetInstance<AnotherServiceWithDependency<IPlugin>>().Dependency;

            // Assert
            AssertThat.IsInstanceOfType(typeof(PluginDecorator), decorator1, "Service was expected to be decorated");
            AssertThat.IsInstanceOfType(typeof(PluginDecorator), anotherDecorator1, "Service was expected to be decorated");

            Assert.AreNotSame(decorator1, anotherDecorator1, @"
                Each conditional registration should get their own decorator, because such decorator can only
                point at one particular dependency.");

            Assert.AreSame(decorator1, decorator2, "Decorator was expected to be singleton");
            Assert.AreSame(anotherDecorator1, anotherDecorator2, "Decorator was expected to be singleton");

            AssertThat.IsInstanceOfType(typeof(PluginImpl), ((PluginDecorator)decorator1).Decoratee, "Wrong type");
            AssertThat.IsInstanceOfType(typeof(PluginImpl2), ((PluginDecorator)anotherDecorator1).Decoratee, "Wrong type");
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
                If your goal is to make one registration a fall back in case another registration is not
                applicable, make the fall back registration last and check the Handled property in the
                predicate."
                .TrimInside(),
                action);
        }

        [TestMethod]
        public void GetInstance_GenericConditionalRegistrationWithFallbackBehavior_ReturnsTheClosedRegistration()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Register<IGeneric<int>, IntGenericType>();

            // Conditional fall back.
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

            // Conditional fall back before the closded registration.
            container.RegisterConditional(typeof(IGeneric<>), typeof(GenericType<>), c => !c.Handled);

            container.Register<IGeneric<int>, IntGenericType>();

            // Act
            Action action = () => container.GetInstance<IGeneric<int>>();

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<ActivationException>(
                "Multiple applicable registrations found for IGeneric<Int32>",
                action);

            AssertThat.ThrowsWithExceptionMessageContains<ActivationException>(
                "make the fall back registration last and check the Handled property in the predicate",
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
                If your goal is to make one registration a fall back in case another registration is not
                applicable, make the fall back registration last and check the Handled property in the
                predicate."
               .TrimInside(),
               action);
        }
    }
}