namespace SimpleInjector.Tests.Unit
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SimpleInjector.Advanced;

    [TestClass]
    public class ExpressionBuildingEventTests
    {
        public interface IValidator<T>
        {
            void Validate(T instance);
        }

        public interface ILogger
        {
            void Write(string message);
        }

        // NOTE: This test is the example code of the XML documentation of the Container.ExpressionBuilding event.
        [TestMethod]
        public void TestExpressionBuilding()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.RegisterSingle<ILogger, ConsoleLogger>();
            container.Register<IValidator<Order>, OrderValidator>();
            container.Register<IValidator<Customer>, CustomerValidator>();

            // Intercept the creation of IValidator<T> instances and wrap them in a MonitoringValidator<T>:
            container.ExpressionBuilding += (sender, e) =>
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
            AssertThat.IsInstanceOfType(typeof(MonitoringValidator<Order>), orderValidator);
            AssertThat.IsInstanceOfType(typeof(MonitoringValidator<Customer>), customerValidator);
        }

        // This test verifies the core difference between ExpressionBuilding and ExpressionBuilt
        [TestMethod]
        public void GetInstance_OnInstanceRegisteredAsSingleton_ExpressionBuildingGetsFiredWithNewExpression()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.RegisterSingle<IUserRepository, SqlUserRepository>();

            Expression actualExpression = null;

            container.ExpressionBuilding += (sender, e) =>
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
            Assert.AreEqual("new SqlUserRepository()", actualExpression.ToString());
        }

        [TestMethod]
        public void ExpressionBuilding_OnInstanceWithInitializer_GetsExpressionWhereInitializerIsNotAppliedYet()
        {
            // Arrange
            Expression actualBuildingExpression = null;

            var container = ContainerFactory.New();

            container.Register<IUserRepository, SqlUserRepository>();

            container.RegisterInitializer<SqlUserRepository>(repository => { });

            container.ExpressionBuilding += (sender, e) =>
            {
                Assert.AreEqual(e.RegisteredServiceType, typeof(IUserRepository), "Test setup fail.");
                actualBuildingExpression = e.Expression;
            };

            // Act
            container.GetInstance<IUserRepository>();

            // Assert
            AssertThat.IsInstanceOfType(typeof(NewExpression), actualBuildingExpression, "The initializer is expected to be applied AFTER the ExpressionBuilding event ran. " +
                "This makes it much easier to alter the given expression.");
        }

        [TestMethod]
        public void GetInstance_ExpressionReplacedOnRootType_ReturnsTheExpectedTypeAndLifeStyle()
        {
            // Arrange
            var container = ContainerFactory.New();

            // Register a transient instance
            container.Register<IUserRepository, SqlUserRepository>();

            container.ExpressionBuilding += (sender, e) =>
            {
                if (e.RegisteredServiceType == typeof(IUserRepository))
                {
                    // Replace the expression with a singleton
                    e.Expression = Expression.Constant(new SqlUserRepository());
                }
            };

            // Act
            var actual1 = container.GetInstance<IUserRepository>();
            var actual2 = container.GetInstance<IUserRepository>();

            // Assert
            AssertThat.IsInstanceOfType(typeof(SqlUserRepository), actual1);
            Assert.IsTrue(object.ReferenceEquals(actual1, actual2),
                "We registered an ConstantExpression. We would the registration to be a singleton.");
        }

        [TestMethod]
        public void GetInstance_ExpressionBuildingEventChangesTheTypeOfTheExpression_ThrowsExpressiveExceptionWhenApplyingInitializer()
        {
            // Arrange
            var container = ContainerFactory.New();

            // Register a transient instance
            container.Register<IUserRepository, SqlUserRepository>();

            container.RegisterInitializer<object>(instance => { });

            container.ExpressionBuilding += (sender, e) =>
            {
                if (e.RegisteredServiceType == typeof(IUserRepository))
                {
                    // Replace the expression with a different type (this is incorrect behavior).
                    e.Expression = Expression.Constant(new InMemoryUserRepository());
                }
            };

            try
            {
                // Act
                container.GetInstance<IUserRepository>();

                // Assert
                Assert.Fail("Exception expected.");
            }
            catch (ActivationException ex)
            {
                AssertThat.ExceptionMessageContains(
                    "The initializer(s) for type SqlUserRepository could not be applied.", ex);
            }
        }

        [TestMethod]
        public void GetInstance_ExpressionReplacedOnNonRootType_ReturnsTheExpectedTypeAndLifeStyle()
        {
            // Arrange
            var container = ContainerFactory.New();

            // Register a transient instance
            container.Register<IUserRepository, SqlUserRepository>();

            container.ExpressionBuilding += (sender, e) =>
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
        public void GetInstance_RegisteredTransient_CallsEventOnceForGivenServiceType()
        {
            // Arrange
            int expectedCallCount = 1;
            int actualCallCount = 0;

            var container = ContainerFactory.New();

            container.Register<IUserRepository, SqlUserRepository>();

            container.ExpressionBuilding += (sender, e) =>
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
        public void GetInstance_RegisteredTransientAndInitializerOnServiceType_CallsEventOnceForGivenServiceType()
        {
            // Arrange
            int expectedCallCount = 1;
            int actualCallCount = 0;

            var container = ContainerFactory.New();

            container.Register<IUserRepository, SqlUserRepository>();

            container.RegisterInitializer<IUserRepository>(instance => { });

            container.ExpressionBuilding += (sender, e) =>
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
        public void GetInstance_RegisteredTransientAndInitializerOnImplementation_CallsEventOnceForGivenServiceType()
        {
            // Arrange
            int expectedCallCount = 1;
            int actualCallCount = 0;

            var container = ContainerFactory.New();

            container.Register<IUserRepository, SqlUserRepository>();

            container.RegisterInitializer<SqlUserRepository>(instance => { });

            container.ExpressionBuilding += (sender, e) =>
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
        public void GetInstance_RegisteredTransient_EventArgsContainsAnExpression()
        {
            // Arrange
            Expression actualExpression = null;

            var container = ContainerFactory.New();

            container.Register<IUserRepository, SqlUserRepository>();

            container.ExpressionBuilding += (sender, e) =>
            {
                actualExpression = e.Expression;
            };

            // Act
            container.GetInstance<IUserRepository>();

            // Assert
            Assert.IsNotNull(actualExpression);
        }

        [TestMethod]
        public void GetInstance_CalledMultipleTimesOnTypeRegisteredTransient_CallsEventOnceForGivenServiceType()
        {
            // Arrange
            int expectedCallCount = 1;
            int actualCallCount = 0;

            var container = ContainerFactory.New();

            container.Register<IUserRepository, SqlUserRepository>();

            container.ExpressionBuilding += (sender, e) =>
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
        public void GetInstance_RegisteredSingleton_CallsEventOnceForGivenServiceType()
        {
            // Arrange
            int expectedCallCount = 1;
            int actualCallCount = 0;

            var container = ContainerFactory.New();

            container.RegisterSingle<IUserRepository, SqlUserRepository>();

            container.ExpressionBuilding += (sender, e) =>
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
        public void GetInstance_RegisteredFunc_CallsEventOnceForGivenServiceType()
        {
            // Arrange
            int expectedCallCount = 1;
            int actualCallCount = 0;

            var container = ContainerFactory.New();

            container.RegisterSingle<IUserRepository>(() => new SqlUserRepository());

            container.ExpressionBuilding += (sender, e) =>
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
        public void GetInstance_RegisteredFunc_CallsEventWithExpectedExpression()
        {
            // Arrange
            Expression actualExpression = null;

            Func<IUserRepository> registeredFactory = () => new SqlUserRepository();

            var container = ContainerFactory.New();

            container.RegisterSingle<IUserRepository>(registeredFactory);

            container.ExpressionBuilding += (sender, e) =>
            {
                actualExpression = e.Expression;
            };

            // Act
            container.GetInstance<IUserRepository>();

            // Assert
            var constantValues = 
                Visitor.GetAllExpressions(actualExpression)
                .OfType<ConstantExpression>()
                .Select(expr => expr.Value);
            
            Assert.IsTrue(constantValues.Contains(registeredFactory),
                "The expression that is generated for a Func<T> registration should contain a " +
                "ConstantExpression with a reference of the registered factory delegate. This way " +
                "ExpressionBuilding registrations can replace the original registered delegate with " +
                "something different.");
        }
        
        [TestMethod]
        public void GetInstance_UnregisteredConcreteType_CallsEventOnceForGivenServiceType()
        {
            // Arrange
            int expectedCallCount = 1;
            int actualCallCount = 0;

            var container = ContainerFactory.New();

            container.ExpressionBuilding += (sender, e) =>
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
        public void GetInstance_CalledOnMultipleTypesThatDependOnAnInterceptedType_CallsEventOnceForGivenServiceType()
        {
            // Arrange
            int expectedCallCount = 1;
            int actualCallCount = 0;

            var container = ContainerFactory.New();

            container.Register<IUserRepository, SqlUserRepository>();

            container.ExpressionBuilding += (sender, e) =>
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

            container.ExpressionBuilding += (sender, e) =>
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
        public void AddExpressionBuilding_AfterContainerHasBeenLocked_ThrowsAnException()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.RegisterSingle<IUserRepository>(new SqlUserRepository());

            // The first use of the container locks the container.
            container.GetInstance<IUserRepository>();

            // Act
            container.ExpressionBuilding += (s, e) => { };
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException),
            "Removal of an event after the container is locked is illegal.")]
        public void RemoveExpressionBuilding_AfterContainerHasBeenLocked_ThrowsAnException()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.RegisterSingle<IUserRepository>(new SqlUserRepository());

            // The first use of the container locks the container.
            container.GetInstance<IUserRepository>();

            // Act
            container.ResolveUnregisteredType -= (s, e) => { };
        }

        [TestMethod]
        public void RemoveExpressionBuilding_BeforeContainerHasBeenLocked_Succeeds()
        {
            // Arrange
            bool handlerCalled = false;

            var container = ContainerFactory.New();

            EventHandler<ExpressionBuildingEventArgs> handler = (sender, e) =>
            {
                handlerCalled = true;
            };

            container.ExpressionBuilding += handler;

            container.RegisterSingle<IUserRepository>(new SqlUserRepository());

            // Act
            container.ExpressionBuilding -= handler;

            container.GetInstance<IUserRepository>();

            // Assert
            Assert.IsFalse(handlerCalled, "The delegate was not removed correctly.");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ExpressionBuildingEventArgsExpressionProperty_SetWithNullReference_ThrowsArgumentNullException()
        {
            // Arrange
            var eventArgs = new ExpressionBuildingEventArgs(
                typeof(IPlugin),
                typeof(PluginImpl), 
                Expression.Constant(new PluginImpl()),
                Lifestyle.Transient);

            // Act
            eventArgs.Expression = null;
        }

        [TestMethod]
        public void GetInstance_ExpressionBuildingWithInvalidExpression_ThrowsAnDescriptiveException()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Register<IUserRepository, SqlUserRepository>();

            container.ExpressionBuilding += (s, e) =>
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
        public void GetInstance_ExpressionBuildingChangedTheRegisterSingleRegistrationToReturnNull_ThrowsExpectedException()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.RegisterSingle<IUserRepository, SqlUserRepository>();

            container.ExpressionBuilding += (s, e) =>
            {
                e.Expression = Expression.Constant(null, typeof(SqlUserRepository));
            };

            try
            {
                // Act
                container.GetInstance(typeof(IUserRepository));

                // Assert
                Assert.Fail("Exception expected.");
            }
            catch (ActivationException ex)
            {
                AssertThat.ExceptionMessageContains(
                    "The registered delegate for type IUserRepository returned null.", ex);
            }
        }
        
        [TestMethod]
        public void GetInstance_ExpressionBuildingChangedExpressionInAnIncompatibleWay_ThrowsExpectedException()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.RegisterSingle<IUserRepository, SqlUserRepository>();

            container.ExpressionBuilding += (s, e) =>
            {
                e.Expression = Expression.Constant("some string", typeof(string));
            };

            try
            {
                // Act
                container.GetInstance(typeof(IUserRepository));

                // Assert
                Assert.Fail("Exception expected.");
            }
            catch (ActivationException ex)
            {
                AssertThat.ExceptionMessageContains(
                    "Error occurred while trying to build a delegate for type IUserRepository using " + 
                    "the expression", ex);
            }
        }

        [TestMethod]
        public void GetInstance_ExpressionBuildingAddingKnownRelationship_GetRelationshipsContainsThatItem()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.RegisterSingle<IUserRepository, SqlUserRepository>();

            container.ExpressionBuilding += (s, e) =>
            {
                if (e.RegisteredServiceType == typeof(IUserRepository))
                {
                    e.KnownRelationships.Add(new KnownRelationship(typeof(object), Lifestyle.Transient,
                        container.GetRegistration(typeof(Container))));
                }
            };

            var registration = container.GetRegistration(typeof(IUserRepository));

            // Act
            registration.GetInstance();

            // Assert
            var relationship = registration.GetRelationships().Single();

            Assert.AreEqual(typeof(object), relationship.ImplementationType);
            Assert.AreEqual(Lifestyle.Transient, relationship.Lifestyle);
            Assert.AreEqual(container.GetRegistration(typeof(Container)), relationship.Dependency);
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

        private sealed class Visitor : ExpressionVisitor
        {
            private readonly Action<Expression> visit;

            public Visitor(Action<Expression> visit)
            {
                this.visit = visit;
            }

            public static IEnumerable<Expression> GetAllExpressions(Expression expression)
            {
                var expressions = new List<Expression>();

                var visitor = new Visitor(expr => { expressions.Add(expr); });

                visitor.Visit(expression);

                return expressions;
            }

            public override Expression Visit(Expression node)
            {
                this.visit(node);
                return base.Visit(node);
            }
        }
    }
}