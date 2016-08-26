namespace SimpleInjector.Tests.Unit
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SimpleInjector.Advanced;

    [TestClass]
    public class ExpressionBuiltEventTests
    {
        public interface IValidator<T>
        {
            void Validate(T instance);
        }

        public interface ILogger
        {
            void Write(string message);
        }

        // NOTE: This test is the example code of the XML documentation of the Container.ExpressionBuilt event.
        [TestMethod]
        public void TestExpressionBuilt()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Register<ILogger, ConsoleLogger>(Lifestyle.Singleton);
            container.Register<IValidator<Order>, OrderValidator>();
            container.Register<IValidator<Customer>, CustomerValidator>();

            // Intercept the creation of IValidator<T> instances and wrap them in a MonitoringValidator<T>:
            container.ExpressionBuilt += (sender, e) =>
            {
                if (e.RegisteredServiceType.IsGenericType() &&
                    e.RegisteredServiceType.GetGenericTypeDefinition() == typeof(IValidator<>))
                {
                    var decoratorType = typeof(MonitoringValidator<>)
                        .MakeGenericType(e.RegisteredServiceType.GetGenericArguments());

                    // Wrap the IValidator<T> in a MonitoringValidator<T>.
                    e.Expression = Expression.New(decoratorType.GetConstructors()[0], new Expression[]
                    {
                        e.Expression,
                        container.GetRegistration(typeof(ILogger)).BuildExpression(),
                    });
                }
            };

            // Act
            var orderValidator = container.GetInstance<IValidator<Order>>();
            var customerValidator = container.GetInstance<IValidator<Customer>>();

            // Assert
            AssertThat.IsInstanceOfType(typeof(MonitoringValidator<Order>), orderValidator);
            AssertThat.IsInstanceOfType(typeof(MonitoringValidator<Customer>), customerValidator);
        }

        // This test verifies the core difference between ExpressionBuilding and ExpressionBuilt
        [TestMethod]
        public void GetInstance_OnInstanceRegisteredAsSingleton_ExpressionBuiltGetsFiredWithConstantExpression()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Register<IUserRepository, SqlUserRepository>(Lifestyle.Singleton);

            Expression actualExpression = null;

            container.ExpressionBuilt += (sender, e) =>
            {
                if (e.RegisteredServiceType == typeof(IUserRepository))
                {
                    actualExpression = e.Expression;
                }
            };

            // Act
            container.GetInstance<IUserRepository>();

            // Assert
            Assert.IsNotNull(actualExpression);
            AssertThat.IsInstanceOfType(typeof(ConstantExpression), actualExpression);
        }

        [TestMethod]
        public void ExpressionBuilt_OnInstanceWithInitializer_GetsExpressionWhereInitializerIsApplied()
        {
            // Arrange
            Expression actualBuiltExpression = null;

            var container = ContainerFactory.New();

            container.Register<IUserRepository, SqlUserRepository>();

            container.RegisterInitializer<SqlUserRepository>(repository => { });

            container.ExpressionBuilt += (sender, e) =>
            {
                Assert.AreEqual(e.RegisteredServiceType, typeof(IUserRepository), "Test setup fail.");
                actualBuiltExpression = e.Expression;
            };

            // Act
            container.GetInstance<IUserRepository>();

            // Assert
            AssertThat.IsNotInstanceOfType(typeof(NewExpression), actualBuiltExpression,
                "The initializer is expected to be applied BEFORE the ExpressionBuilt event ran.");
        }

        [TestMethod]
        public void GetInstance_ExpressionReplacedOnRootType_ReturnsTheExpectedTypeAndLifeStyle()
        {
            // Arrange
            var container = ContainerFactory.New();

            // Register a transient instance
            container.Register<IUserRepository, SqlUserRepository>();

            container.ExpressionBuilt += (sender, e) =>
            {
                if (e.RegisteredServiceType == typeof(IUserRepository))
                {
                    // Replace the expression with a singleton
                    e.Expression = Expression.Constant(new InMemoryUserRepository());
                }
            };

            // Act
            var actual1 = container.GetInstance<IUserRepository>();
            var actual2 = container.GetInstance<IUserRepository>();

            // Assert
            AssertThat.IsInstanceOfType(typeof(InMemoryUserRepository), actual1);
            Assert.IsTrue(object.ReferenceEquals(actual1, actual2),
                "We registered an ConstantExpression. We would the registration to be a singleton.");
        }

        [TestMethod]
        public void GetInstance_ExpressionReplacedOnNonRootType_ReturnsTheExpectedTypeAndLifeStyle()
        {
            // Arrange
            var container = ContainerFactory.New();

            // Register a transient instance
            container.Register<IUserRepository, SqlUserRepository>();

            container.ExpressionBuilt += (sender, e) =>
            {
                if (e.RegisteredServiceType == typeof(IUserRepository))
                {
                    // Replace the expression with a singleton
                    e.Expression = Expression.Constant(new InMemoryUserRepository());
                }
            };

            // Act
            var actual1 = container.GetInstance<RealUserService>().Repository;
            var actual2 = container.GetInstance<RealUserService>().Repository;

            // Assert
            AssertThat.IsInstanceOfType(typeof(InMemoryUserRepository), actual1);
            Assert.IsTrue(object.ReferenceEquals(actual1, actual2),
                "We registered an ConstantExpression. We would the registration to be a singleton.");
        }

        [TestMethod]
        public void GetInstance_RegisteredTransientWithInterceptor_CallsEventOnce()
        {
            // Arrange
            int expectedCallCount = 1;
            int actualCallCount = 0;

            var container = ContainerFactory.New();

            container.Register<IUserRepository, SqlUserRepository>();

            container.ExpressionBuilt += (sender, e) =>
            {
                if (e.RegisteredServiceType == typeof(IUserRepository))
                {
                    actualCallCount++;
                }
            };

            // Act
            container.GetInstance<IUserRepository>();

            // Assert
            Assert.AreEqual(expectedCallCount, actualCallCount);
        }

        [TestMethod]
        public void GetInstance_RegisteredTransientWithInterceptorAndInitializerOnServiceType_CallsEventOnce()
        {
            // Arrange
            int expectedCallCount = 1;
            int actualCallCount = 0;

            var container = ContainerFactory.New();

            container.Register<IUserRepository, SqlUserRepository>();

            container.RegisterInitializer<IUserRepository>(instance => { });

            container.ExpressionBuilt += (sender, e) =>
            {
                if (e.RegisteredServiceType == typeof(IUserRepository))
                {
                    actualCallCount++;
                }
            };

            // Act
            container.GetInstance<IUserRepository>();

            // Assert
            Assert.AreEqual(expectedCallCount, actualCallCount);
        }

        [TestMethod]
        public void GetInstance_RegisteredTransientWithInterceptorAndInitializerOnImplementation_CallsEventOnce()
        {
            // Arrange
            int expectedCallCount = 1;
            int actualCallCount = 0;

            var container = ContainerFactory.New();

            container.Register<IUserRepository, SqlUserRepository>();

            container.RegisterInitializer<SqlUserRepository>(instance => { });

            container.ExpressionBuilt += (sender, e) =>
            {
                if (e.RegisteredServiceType == typeof(IUserRepository))
                {
                    actualCallCount++;
                }
            };

            // Act
            container.GetInstance<IUserRepository>();

            // Assert
            Assert.AreEqual(expectedCallCount, actualCallCount);
        }

        [TestMethod]
        public void GetInstance_RegisteredTransientWithInterceptor_EventArgsContainsAnExpression()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Register<IUserRepository, SqlUserRepository>();

            container.ExpressionBuilt += (sender, e) =>
            {
                // Assert
                Assert.IsNotNull(e.Expression);
            };

            // Act
            container.GetInstance<IUserRepository>();
        }

        [TestMethod]
        public void GetInstance_CalledMultipleTimesOnTypeRegisteredTransientWithInterceptor_CallsEventOnce()
        {
            // Arrange
            int expectedCallCount = 1;
            int actualCallCount = 0;

            var container = ContainerFactory.New();

            container.Register<IUserRepository, SqlUserRepository>();

            container.ExpressionBuilt += (sender, e) =>
            {
                if (e.RegisteredServiceType == typeof(IUserRepository))
                {
                    actualCallCount++;
                }
            };

            // Act
            container.GetInstance<IUserRepository>();
            container.GetInstance<IUserRepository>();

            // Assert
            Assert.AreEqual(expectedCallCount, actualCallCount);
        }

        [TestMethod]
        public void GetInstance_RegisteredSingletonWithInterceptor_CallsEventOnce()
        {
            // Arrange
            int expectedCallCount = 1;
            int actualCallCount = 0;

            var container = ContainerFactory.New();

            container.Register<IUserRepository, SqlUserRepository>(Lifestyle.Singleton);

            container.ExpressionBuilt += (sender, e) =>
            {
                if (e.RegisteredServiceType == typeof(IUserRepository))
                {
                    actualCallCount++;
                }
            };

            // Act
            container.GetInstance<IUserRepository>();

            // Assert
            Assert.AreEqual(expectedCallCount, actualCallCount, @"
                The event is expected to be called once. The old v1 behavior was to call it twice; once
                once after the NewExpression gets built, and once when the ConstantExpression gets built. " +
                "In v2 the first call will be handled by another event: ExpressionBuilding.");
        }

        [TestMethod]
        public void GetInstance_RegisteredFuncWithInterceptor_CallsEventOnce()
        {
            // Arrange
            int expectedCallCount = 1;
            int actualCallCount = 0;

            var container = ContainerFactory.New();

            container.Register<IUserRepository>(() => new SqlUserRepository(), Lifestyle.Singleton);

            container.ExpressionBuilt += (sender, e) =>
            {
                if (e.RegisteredServiceType == typeof(IUserRepository))
                {
                    actualCallCount++;
                }
            };

            // Act
            container.GetInstance<IUserRepository>();

            // Assert
            Assert.AreEqual(expectedCallCount, actualCallCount);
        }

        [TestMethod]
        public void GetInstance_UnregisteredConcreteTypeWithInterceptor_CallsEventOnce()
        {
            // Arrange
            int expectedCallCount = 1;
            int actualCallCount = 0;

            var container = ContainerFactory.New();

            container.ExpressionBuilt += (sender, e) =>
            {
                if (e.RegisteredServiceType == typeof(SqlUserRepository))
                {
                    actualCallCount++;
                }
            };

            // Act
            container.GetInstance<SqlUserRepository>();

            // Assert
            Assert.AreEqual(expectedCallCount, actualCallCount);
        }

        [TestMethod]
        public void GetInstance_CalledOnMultipleTypesThatDependOnAInterceptedType_CallsEventOnce()
        {
            // Arrange
            int expectedCallCount = 1;
            int actualCallCount = 0;

            var container = ContainerFactory.New();

            container.Register<IUserRepository, SqlUserRepository>();

            container.ExpressionBuilt += (sender, e) =>
            {
                if (e.RegisteredServiceType == typeof(IUserRepository))
                {
                    actualCallCount++;
                }
            };

            // Act
            // Both types depend on IUserRepository.
            container.GetInstance<RealUserService>();
            container.GetInstance<FakeUserService>();

            // Assert
            Assert.AreEqual(expectedCallCount, actualCallCount,
                "The event is expected to called just once for the IUserRepository for performance reasons.");
        }

        [TestMethod]
        public void GetInstance_CalledOnInterceptedTypeAndTypeThatDependsOnTheInterceptedType_CallsEventOnce()
        {
            // Arrange
            int expectedCallCount = 1;
            int actualCallCount = 0;

            var container = ContainerFactory.New();

            container.Register<IUserRepository, SqlUserRepository>();

            container.ExpressionBuilt += (sender, e) =>
            {
                if (e.RegisteredServiceType == typeof(IUserRepository))
                {
                    actualCallCount++;
                }
            };

            // Act
            container.GetInstance<IUserRepository>();

            // FakeUserService depends on IUserRepository.
            container.GetInstance<FakeUserService>();

            // Assert
            Assert.AreEqual(expectedCallCount, actualCallCount,
                "The event is expected to called just once for the IUserRepository for performance reasons.");
        }

        [TestMethod]
        public void AddExpressionBuilt_AfterContainerHasBeenLocked_ThrowsAnException()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.RegisterSingleton<IUserRepository>(new SqlUserRepository());

            // The first use of the container locks the container.
            container.GetInstance<IUserRepository>();

            // Act
            Action action = () => container.ExpressionBuilt += (s, e) => { };

            // Assert
            AssertThat.Throws<InvalidOperationException>(action,
                "Registration of an event after the container is locked is illegal.");
        }

        [TestMethod]
        public void RemoveExpressionBuilt_AfterContainerHasBeenLocked_ThrowsAnException()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.RegisterSingleton<IUserRepository>(new SqlUserRepository());

            // The first use of the container locks the container.
            container.GetInstance<IUserRepository>();

            // Act
            Action action = () => container.ResolveUnregisteredType -= (s, e) => { };

            // Assert
            AssertThat.Throws<InvalidOperationException>(action,
                "Registration of an event after the container is locked is illegal.");
        }

        [TestMethod]
        public void RemoveExpressionBuilt_BeforeContainerHasBeenLocked_Succeeds()
        {
            // Arrange
            bool handlerCalled = false;

            var container = ContainerFactory.New();

            EventHandler<ExpressionBuiltEventArgs> handler = (sender, e) =>
            {
                handlerCalled = true;
            };

            container.ExpressionBuilt += handler;

            container.RegisterSingleton<IUserRepository>(new SqlUserRepository());

            // Act
            container.ExpressionBuilt -= handler;

            container.GetInstance<IUserRepository>();

            // Assert
            Assert.IsFalse(handlerCalled, "The delegate was not removed correctly.");
        }

        [TestMethod]
        public void ExpressionBuiltEventArgsExpressionProperty_SetWithNullReference_ThrowsArgumentNullException()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Register<IPlugin, PluginImpl>();

            // Act
            container.ExpressionBuilt += (s, e) =>
            {
                e.Expression = null;
            };

            Action action = () =>
            {
                try
                {
                    container.GetInstance<IPlugin>();

                    // Assert
                    Assert.Fail("Exception expected.");
                }
                catch (Exception ex)
                {
                    throw ex.GetExceptionChain().Last();
                }
            };
            
            // Assert
            AssertThat.Throws<ArgumentNullException>(action);
        }

        [TestMethod]
        public void GetInstance_ExpressionBuiltWithInvalidExpression_ThrowsAnDescriptiveException()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Register<IUserRepository, SqlUserRepository>();

            container.ExpressionBuilt += (s, e) =>
            {
                var invalidExpression = Expression.GreaterThan(Expression.Constant(1), Expression.Constant(1));

                e.Expression = invalidExpression;
            };

            try
            {
                // Act
                container.GetInstance<IUserRepository>();

                // Assert
                Assert.Fail("Exception was expected.");
            }
            catch (Exception ex)
            {
                AssertThat.StringContains(
                    "Error occurred while trying to build a delegate for type IUserRepository",
                    ex.Message, "Exception message was not descriptive.");
            }
        }

        [TestMethod]
        public void KnownRelationships_AddingNullValue_ThrowsExpectedException()
        {
            // Arrange
            KnownRelationship invalidRelationship = null;

            var container = ContainerFactory.New();

            container.ExpressionBuilt += (s, e) =>
            {
                // Assert
                AssertThat.Throws<ArgumentNullException>(() => e.KnownRelationships.Add(invalidRelationship));
            };

            // Act
            container.GetInstance<SqlUserRepository>();
        }

        [TestMethod]
        public void KnownRelationships_InsertingNullValue_ThrowsExpectedException()
        {
            // Arrange
            KnownRelationship invalidRelationship = null;

            var container = ContainerFactory.New();

            container.Register<IUserRepository, SqlUserRepository>();
            container.Register<FakeUserService>();

            container.ExpressionBuilt += (s, e) =>
            {
                if (e.RegisteredServiceType == typeof(FakeUserService))
                {
                    Assert.AreEqual(1, e.KnownRelationships.Count, "Test setup failed.");

                    // Assert
                    AssertThat.Throws<ArgumentNullException>(
                        () => e.KnownRelationships.Insert(0, invalidRelationship));
                }               
            };

            // Act
            container.GetInstance<FakeUserService>();
        }

        public class Order
        {
        }

        public class Customer
        {
        }

        public class OrderValidator : IValidator<Order>
        {
            public void Validate(Order instance)
            {
            }
        }

        public class CustomerValidator : IValidator<Customer>
        {
            public void Validate(Customer instance)
            {
            }
        }

        public class ConsoleLogger : ILogger
        {
            public void Write(string message)
            {
                Console.WriteLine(message);
            }
        }

        // Implementation of the decorator pattern.
        public class MonitoringValidator<T> : IValidator<T>
        {
            private readonly IValidator<T> validator;
            private readonly ILogger logger;

            public MonitoringValidator(IValidator<T> validator, ILogger logger)
            {
                this.validator = validator;
                this.logger = logger;
            }

            public void Validate(T instance)
            {
                this.logger.Write("Validating " + typeof(T).Name);
                this.validator.Validate(instance);
                this.logger.Write("Validated " + typeof(T).Name);
            }
        }
    }
}