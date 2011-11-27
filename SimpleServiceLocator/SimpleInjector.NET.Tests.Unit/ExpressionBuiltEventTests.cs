namespace SimpleInjector.Tests.Unit
{
    using System;
    using System.Linq.Expressions;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

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
            var container = new Container();

            container.RegisterSingle<ILogger, ConsoleLogger>();
            container.Register<IValidator<Order>, OrderValidator>();
            container.Register<IValidator<Customer>, CustomerValidator>();

            // Intercept the creation of IValidator<T> instances and wrap them in a MonitoringValidator<T>:
            container.ExpressionBuilt += (sender, e) =>
            {
                if (e.RegisteredServiceType.IsGenericType &&
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
            Assert.IsInstanceOfType(orderValidator, typeof(MonitoringValidator<Order>));
            Assert.IsInstanceOfType(customerValidator, typeof(MonitoringValidator<Customer>));
        }

        [TestMethod]
        public void GetInstance_ExpressionReplacedOnRootType_ReturnsTheExpectedTypeAndLifeStyle()
        {
            // Arrange
            var container = new Container();

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
            Assert.IsInstanceOfType(actual1, typeof(InMemoryUserRepository));
            Assert.IsTrue(object.ReferenceEquals(actual1, actual2),
                "We registered an ConstantExpression. We would the registration to be a singleton.");
        }

        [TestMethod]
        public void GetInstance_ExpressionReplacedOnNonRootType_ReturnsTheExpectedTypeAndLifeStyle()
        {
            // Arrange
            var container = new Container();

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
            Assert.IsInstanceOfType(actual1, typeof(InMemoryUserRepository));
            Assert.IsTrue(object.ReferenceEquals(actual1, actual2),
                "We registered an ConstantExpression. We would the registration to be a singleton.");
        }

        [TestMethod]
        public void GetInstance_RegisteredTransientWithInterceptor_CallsEventOnce()
        {
            // Arrange
            int expectedCallCount = 1;
            int actualCallCount = 0;

            var container = new Container();

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

            var container = new Container();

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

            var container = new Container();

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
            var container = new Container();

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

            var container = new Container();

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
        public void GetInstance_RegisteredSingletonWithInterceptor_CallsEventTwice()
        {
            // Arrange
            int expectedCallCount = 2;
            int actualCallCount = 0;

            var container = new Container();

            container.RegisterSingle<IUserRepository, SqlUserRepository>();

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
            Assert.AreEqual(expectedCallCount, actualCallCount,
                "The event is expected to be called twice. Once after the NewExpression gets built, and " +
                "once when the ConstantExpression gets built.");
        }

        [TestMethod]
        public void GetInstance_RegisteredFuncWithInterceptor_CallsEventOnce()
        {
            // Arrange
            int expectedCallCount = 1;
            int actualCallCount = 0;

            var container = new Container();

            container.RegisterSingle<IUserRepository>(() => new SqlUserRepository());

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

            var container = new Container();

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

            var container = new Container();

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

            var container = new Container();

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
        [ExpectedException(typeof(InvalidOperationException),
            "Registration of an event after the container is locked is illegal.")]
        public void AddExpressionBuilt_AfterContainerHasBeenLocked_ThrowsAnException()
        {
            // Arrange
            var container = new Container();

            container.RegisterSingle<IUserRepository>(new SqlUserRepository());

            // The first use of the container locks the container.
            container.GetInstance<IUserRepository>();

            // Act
            container.ExpressionBuilt += (s, e) => { };
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException),
            "Removal of an event after the container is locked is illegal.")]
        public void RemoveExpressionBuilt_AfterContainerHasBeenLocked_ThrowsAnException()
        {
            // Arrange
            var container = new Container();

            container.RegisterSingle<IUserRepository>(new SqlUserRepository());

            // The first use of the container locks the container.
            container.GetInstance<IUserRepository>();

            // Act
            container.ResolveUnregisteredType -= (s, e) => { };
        }

        [TestMethod]
        public void RemoveExpressionBuilt_BeforeContainerHasBeenLocked_Succeeds()
        {
            // Arrange
            bool handlerCalled = false;

            var container = new Container();

            EventHandler<ExpressionBuiltEventArgs> handler = (sender, e) =>
            {
                handlerCalled = true;
            };

            container.ExpressionBuilt += handler;

            container.RegisterSingle<IUserRepository>(new SqlUserRepository());
            
            // Act
            container.ExpressionBuilt -= handler;

            container.GetInstance<IUserRepository>();

            // Assert
            Assert.IsFalse(handlerCalled, "The delegate was not removed correctly.");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ExpressionBuiltEventArgsExpressionProperty_SetWithNullReference_ThrowsArgumentNullException()
        {
            // Arrange
            var eventArgs = 
                new ExpressionBuiltEventArgs(typeof(IPlugin), Expression.Constant(new PluginImpl()));

            // Act
            eventArgs.Expression = null;
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