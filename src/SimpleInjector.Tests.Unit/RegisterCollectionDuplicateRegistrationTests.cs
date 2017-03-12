namespace SimpleInjector.Tests.Unit
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// This set of tests control whether the container correctly prevents duplicate registrations of 
    /// collections and correctly replaces a collection in case overriding registrations is allowed.
    /// </summary>
    [TestClass]
    public class RegisterCollectionDuplicateRegistrationTests
    {
        [TestMethod]
        public void RegisterCollectionTServiceUncontrolled_CalledTwiceWithSameClosedGenericType_Throws()
        {
            Assert_CalledTwiceForSameType_ThrowsExpectedMessage(
                "Collection of items for type IEventHandler<Int32> has already been registered",
                c => c.RegisterCollection<IEventHandler<int>>(Enumerable.Empty<IEventHandler<int>>()));
        }

        [TestMethod]
        public void RegisterCollectionTServiceUncontrolled_CalledTwiceWithSameClosedGenericTypeAllowOverridingRegistrations_ResolvedExpectedType()
        {
            IEnumerable<IEventHandler<int>> handlers1 = new[] { new StructConstraintEventHandler<int>() };
            IEnumerable<IEventHandler<int>> handlers2 = new[] { new GenericEventHandler<int>() };

            Assert_CalledTwiceForSameType_ResolvesExpectedSequence<IEventHandler<int>>(c =>
                {
                    c.RegisterCollection<IEventHandler<int>>(handlers1);
                    c.Options.AllowOverridingRegistrations = true;
                    c.RegisterCollection<IEventHandler<int>>(handlers2);
                },
                expectedTypes: typeof(GenericEventHandler<int>));
        }

        [TestMethod]
        public void RegisterCollectionTServiceUncontrolled_CalledTwiceWithSameNonGenericType_Throws()
        {
            Assert_CalledTwiceForSameType_ThrowsExpectedMessage(
                "Collection of items for type ILogger has already been registered",
                c => c.RegisterCollection<ILogger>(Enumerable.Empty<ILogger>()));
        }

        [TestMethod]
        public void RegisterCollectionTServiceUncontrolled_CalledTwiceWithSameNonGenericTypeAllowOverridingRegistrations_ResolvedExpectedType()
        {
            Assert_CalledTwiceForSameType_ResolvesExpectedSequence<ILogger>(c =>
                {
                    c.RegisterCollection<ILogger>((IEnumerable<ILogger>)new[] { new NullLogger() });
                    c.Options.AllowOverridingRegistrations = true;
                    c.RegisterCollection<ILogger>((IEnumerable<ILogger>)new[] { new ConsoleLogger() });
                },
                expectedTypes: typeof(ConsoleLogger));
        }

        [TestMethod]
        public void RegisterCollectionTServiceSingletons_CalledTwiceWithSameClosedGenericType_Throws()
        {
            Assert_CalledTwiceForSameType_ThrowsExpectedMessage(
                "Collection of items for type IEventHandler<Int32> has already been registered",
                c => c.RegisterCollection<IEventHandler<int>>(new[] { new StructConstraintEventHandler<int>() }));
        }

        [TestMethod]
        public void RegisterCollectionTServiceSingletons_CalledTwiceWithSameClosedGenericTypeAllowOverridingRegistrations_ResolvedExpectedType()
        {
            Assert_CalledTwiceForSameType_ResolvesExpectedSequence<IEventHandler<int>>(c =>
                {
                    c.RegisterCollection<IEventHandler<int>>(new[] { new StructConstraintEventHandler<int>() });
                    c.Options.AllowOverridingRegistrations = true;
                    c.RegisterCollection<IEventHandler<int>>(new[] { new GenericEventHandler<int>() });
                },
                expectedTypes: typeof(GenericEventHandler<int>));
        }

        [TestMethod]
        public void RegisterCollectionTServiceSingletons_CalledTwiceWithSameNonGenericType_Throws()
        {
            Assert_CalledTwiceForSameType_ThrowsExpectedMessage(
                "Collection of items for type ILogger has already been registered",
                c => c.RegisterCollection<ILogger>(new[] { new NullLogger() }));
        }

        [TestMethod]
        public void RegisterCollectionTServiceSingletons_CalledTwiceWithSameNonGenericTypeAllowOverridingRegistrations_ResolvedExpectedType()
        {
            Assert_CalledTwiceForSameType_ResolvesExpectedSequence<ILogger>(c =>
                {
                    c.RegisterCollection<ILogger>(new[] { new NullLogger() });
                    c.Options.AllowOverridingRegistrations = true;
                    c.RegisterCollection<ILogger>(new[] { new ConsoleLogger() });
                },
                expectedTypes: typeof(ConsoleLogger));
        }

        [TestMethod]
        public void RegisterCollectionTServiceTypes_CalledTwiceWithSameClosedGenericType_Throws()
        {
            Assert_CalledTwiceForSameType_ThrowsExpectedMessage(
                "Collection of items for type IEventHandler<Int32> has already been registered",
                c => c.RegisterCollection<IEventHandler<int>>(new[] { typeof(StructConstraintEventHandler<int>) }));
        }

        [TestMethod]
        public void RegisterCollectionTServiceTypes_CalledTwiceWithSameClosedGenericTypeAllowOverridingRegistrations_ResolvedExpectedType()
        {
            Assert_CalledTwiceForSameType_ResolvesExpectedSequence<IEventHandler<int>>(c =>
                {
                    c.RegisterCollection<IEventHandler<int>>(new[] { typeof(StructConstraintEventHandler<int>) });
                    c.Options.AllowOverridingRegistrations = true;
                    c.RegisterCollection<IEventHandler<int>>(new[] { typeof(GenericEventHandler<int>) });
                },
                expectedTypes: typeof(GenericEventHandler<int>));
        }

        [TestMethod]
        public void RegisterCollectionTServiceTypes_CalledTwiceWithSameNonGenericType_Throws()
        {
            Assert_CalledTwiceForSameType_ThrowsExpectedMessage(
                "Collection of items for type ILogger has already been registered",
                c => c.RegisterCollection<ILogger>(new[] { typeof(NullLogger) }));
        }

        [TestMethod]
        public void RegisterCollectionTServiceTypes_CalledTwiceWithSameNonGenericTypeAllowOverridingRegistrations_ResolvedExpectedType()
        {
            Assert_CalledTwiceForSameType_ResolvesExpectedSequence<ILogger>(c =>
                {
                    c.RegisterCollection<ILogger>(new[] { typeof(NullLogger) });
                    c.Options.AllowOverridingRegistrations = true;
                    c.RegisterCollection<ILogger>(new[] { typeof(ConsoleLogger) });
                },
                expectedTypes: typeof(ConsoleLogger));
        }

        [TestMethod]
        public void RegisterCollectionTypes_CalledTwiceWithSameClosedGenericType_Throws()
        {
            Assert_CalledTwiceForSameType_ThrowsExpectedMessage(
                "Collection of items for type IEventHandler<Int32> has already been registered",
                c => c.RegisterCollection(typeof(IEventHandler<int>), new[] { new StructConstraintEventHandler<int>() }));
        }
        
        [TestMethod]
        public void RegisterCollectionTypes_CalledTwiceWithSameClosedGenericTypeAllowOverridingRegistrations_ResolvedExpectedType()
        {
            Assert_CalledTwiceForSameType_ResolvesExpectedSequence<IEventHandler<int>>(c =>
                {
                    c.RegisterCollection(typeof(IEventHandler<int>), new[] { typeof(StructConstraintEventHandler<int>) });
                    c.Options.AllowOverridingRegistrations = true;
                    c.RegisterCollection(typeof(IEventHandler<int>), new[] { typeof(GenericEventHandler<int>) });
                },
                expectedTypes: typeof(GenericEventHandler<int>));
        }

        [TestMethod]
        public void RegisterCollectionTypes_CalledTwiceWithSameNonGenericType_Throws()
        {
            Assert_CalledTwiceForSameType_ThrowsExpectedMessage(
                "Collection of items for type ILogger has already been registered",
                c => c.RegisterCollection(typeof(ILogger), new[] { typeof(NullLogger) }));
        }

        [TestMethod]
        public void RegisterCollectionTypes_CalledTwiceWithSameNonGenericTypeAllowOverridingRegistrations_ResolvedExpectedType()
        {
            Assert_CalledTwiceForSameType_ResolvesExpectedSequence<ILogger>(c =>
                {
                    c.RegisterCollection(typeof(ILogger), new[] { typeof(NullLogger) });
                    c.Options.AllowOverridingRegistrations = true;
                    c.RegisterCollection(typeof(ILogger), new[] { typeof(ConsoleLogger) });
                },
                expectedTypes: typeof(ConsoleLogger));
        }
        
        [TestMethod]
        public void RegisterCollectionRegistrations_CalledTwiceWithSameClosedGenericType_Throws()
        {
            Assert_CalledTwiceForSameType_ThrowsExpectedMessage(
                "Collection of items for type IEventHandler<Int32> has already been registered",
                c => c.RegisterCollection(typeof(IEventHandler<int>), new[] 
                { 
                    Lifestyle.Transient.CreateRegistration<StructConstraintEventHandler<int>>(c)
                }));
        }

        [TestMethod]
        public void RegisterCollectionRegistrations_CalledTwiceWithSameClosedGenericTypeAllowOverridingRegistrations_ResolvedExpectedType()
        {
            Assert_CalledTwiceForSameType_ResolvesExpectedSequence<IEventHandler<int>>(c =>
                {
                    c.RegisterCollection(typeof(IEventHandler<int>), new[] 
                    { 
                        Lifestyle.Transient.CreateRegistration<StructConstraintEventHandler<int>>(c)
                    });

                    c.Options.AllowOverridingRegistrations = true;

                    c.RegisterCollection(typeof(IEventHandler<int>), new[] 
                    { 
                        Lifestyle.Transient.CreateRegistration<GenericEventHandler<int>>(c)
                    });
                },
                expectedTypes: typeof(GenericEventHandler<int>));
        }

        [TestMethod]
        public void RegisterCollectionRegistrations_CalledTwiceWithSameNonGenericType_Throws()
        {
            Assert_CalledTwiceForSameType_ThrowsExpectedMessage(
                "Collection of items for type ILogger has already been registered",
                c => c.RegisterCollection(typeof(ILogger), new[] 
                { 
                    Lifestyle.Transient.CreateRegistration<NullLogger>(c) 
                }));
        }

        [TestMethod]
        public void RegisterCollectionRegistrations_CalledTwiceWithSameNonGenericTypeAllowOverridingRegistrations_ResolvedExpectedType()
        {
            Assert_CalledTwiceForSameType_ResolvesExpectedSequence<ILogger>(c =>
                {
                    c.RegisterCollection(typeof(ILogger), new[] 
                    { 
                        Lifestyle.Transient.CreateRegistration<NullLogger>(c) 
                    });

                    c.Options.AllowOverridingRegistrations = true;

                    c.RegisterCollection(typeof(ILogger), new[] 
                    { 
                        Lifestyle.Transient.CreateRegistration<ConsoleLogger>(c) 
                    });
                },
                expectedTypes: typeof(ConsoleLogger));
        }
        
        [TestMethod]
        public void RegisterCollectionUncontrolled_CalledTwiceWithSameClosedGenericType_Throws()
        {
            Assert_CalledTwiceForSameType_ThrowsExpectedMessage(
                "Collection of items for type IEventHandler<Int32> has already been registered",
                c => c.RegisterCollection(typeof(IEventHandler<int>), Enumerable.Empty<IEventHandler<int>>()));
        }

        [TestMethod]
        public void RegisterCollectionUncontrolled_CalledTwiceWithSameClosedGenericTypeAllowOverridingRegistrations_ResolvedExpectedType()
        {
            Assert_CalledTwiceForSameType_ResolvesExpectedSequence<IEventHandler<int>>(c =>
                {
                    c.RegisterCollection(typeof(IEventHandler<int>), new[] { new StructConstraintEventHandler<int>() });
                    c.Options.AllowOverridingRegistrations = true;
                    c.RegisterCollection(typeof(IEventHandler<int>), new[] { new GenericEventHandler<int>() });
                },
                expectedTypes: typeof(GenericEventHandler<int>));
        }

        [TestMethod]
        public void RegisterCollectionUncontrolled_CalledTwiceWithSameNonGenericType_Throws()
        {
            Assert_CalledTwiceForSameType_ThrowsExpectedMessage(
                "Collection of items for type ILogger has already been registered",
                c => c.RegisterCollection(typeof(ILogger), Enumerable.Empty<ILogger>()));
        }

        [TestMethod]
        public void RegisterCollectionUncontrolled_CalledTwiceWithSameNonGenericTypeAllowOverridingRegistrations_ResolvedExpectedType()
        {
            Assert_CalledTwiceForSameType_ResolvesExpectedSequence<ILogger>(c =>
                {
                    c.RegisterCollection(typeof(ILogger), (IEnumerable)new[] { new NullLogger() });
                    c.Options.AllowOverridingRegistrations = true;
                    c.RegisterCollection(typeof(ILogger), (IEnumerable)new[] { new ConsoleLogger() });
                },
                expectedTypes: typeof(ConsoleLogger));
        }

        [TestMethod]
        public void RegisterCollection_ForAnOpenGenericCollectionAfterACallOfAClosedGenericVersion_Fails()
        {
            // Arrange
            var container = new Container();

            container.RegisterCollection(typeof(IEventHandler<AuditableEvent>), new[]
            {
                typeof(AuditableEventEventHandler)
            });

            // Act 
            Action action = () => container.RegisterCollection(typeof(IEventHandler<>), new[] { typeof(StructEventHandler) });

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<InvalidOperationException>(@"
                Mixing calls to RegisterCollection for the same open generic service type is not supported. Consider
                making one single call to RegisterCollection(typeof(IEventHandler<>), types)."
                .TrimInside(),
                action);
        }

        [TestMethod]
        public void RegisterCollection_ForClosedGenericCollectionAfterACallOfThatOpenGenericVersion_Fails()
        {
            // Arrange
            var container = new Container();

            container.RegisterCollection(typeof(IEventHandler<>), new[] { typeof(StructEventHandler) });

            // Act
            Action action = () => container.RegisterCollection(typeof(IEventHandler<AuditableEvent>), new[]
            {
                typeof(AuditableEventEventHandler)
            });

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<InvalidOperationException>(@"
                Mixing calls to RegisterCollection for the same open generic service type is not supported. Consider
                making one single call to RegisterCollection(typeof(IEventHandler<>), types)."
                .TrimInside(),
                action);
        }

        private static void Assert_CalledTwiceForSameType_ThrowsExpectedMessage(string message,
            Action<Container> registrationDelegate)
        {
            // Arrange
            var container = new Container();

            Action action = () => registrationDelegate(container);

            // Act
            action();

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<InvalidOperationException>(message, action);
        }

        private static void Assert_CalledTwiceForSameType_ResolvesExpectedSequence<T>(
            Action<Container> registration,
            params Type[] expectedTypes)
            where T : class
        {
            // Arrange
            var container = new Container();

            // Act
            registration(container);

            var instances = container.GetAllInstances<T>();

            var actualTypes = instances.Select(GetType).ToArray();

            // Assert
            AssertThat.SequenceEquals(expectedTypes, actualTypes);
        }

        private static string ToActualTypeNames<T>(IEnumerable<T> instances) => 
            "Actual: " + instances.Select(GetType).ToFriendlyNamesText();

        private static Type GetType<T>(T instance) => instance.GetType();
    }
}