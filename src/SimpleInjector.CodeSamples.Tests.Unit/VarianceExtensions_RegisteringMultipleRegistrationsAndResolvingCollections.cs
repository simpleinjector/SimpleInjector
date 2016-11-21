namespace SimpleInjector.CodeSamples.Tests.Unit
{
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Variance Scenario 3: Multiple registrations, multiple resolve.
    /// Per service, multiple types are registered and multiple types are resolved or used behind
    /// the covers.
    /// </summary>
    [TestClass]
    public class VarianceExtensions_RegisteringMultipleRegistrationsAndResolvingCollections
    {
        public interface IEventHandler<in TEvent>
        {
            void Handle(TEvent e);
        }

        [TestMethod]
        public void Handle_CustomerMovedEvent_ExecutesExpectedEventHandlers()
        {
            // Arrange
            var logger = new ListLogger();
            var container = CreateContainer(logger);

            var handler = container.GetInstance<IEventHandler<CustomerMovedEvent>>();

            // Act
            handler.Handle(new CustomerMovedEvent());

            // Assert
            Assert.AreEqual(2, logger.Count, logger.ToString());
            Assert.IsTrue(logger.Contains("CustomerMovedEventHandler handled CustomerMovedEvent"), logger.ToString());
            Assert.IsTrue(logger.Contains("NotifyStaffWhenCustomerMovedEventHandler handled CustomerMovedEvent"), logger.ToString());
        }

        [TestMethod]
        public void Handle_SpecialCustomerMovedEvent_ExecutesExpectedEventHandlers()
        {
            // Arrange
            var logger = new ListLogger();
            var container = CreateContainer(logger);

            var handler = container.GetInstance<IEventHandler<SpecialCustomerMovedEvent>>();

            handler.Handle(new SpecialCustomerMovedEvent());

            // Assert
            Assert.AreEqual(2, logger.Count);
            Assert.IsTrue(logger.Contains("CustomerMovedEventHandler handled SpecialCustomerMovedEvent"), logger.ToString());
            Assert.IsTrue(logger.Contains("NotifyStaffWhenCustomerMovedEventHandler handled SpecialCustomerMovedEvent"), logger.ToString());
        }

        [TestMethod]
        public void Handle_CustomerMovedAbroadEvent_ExecutesExpectedEventHandlers()
        {
            // Arrange
            var logger = new ListLogger();
            var container = CreateContainer(logger);

            var handler = container.GetInstance<IEventHandler<CustomerMovedAbroadEvent>>();

            // Act
            handler.Handle(new CustomerMovedAbroadEvent());

            // Assert
            Assert.AreEqual(3, logger.Count, logger.ToString());

            Assert.IsTrue(logger.Contains("CustomerMovedEventHandler handled CustomerMovedAbroadEvent"), logger.ToString());
            Assert.IsTrue(logger.Contains("NotifyStaffWhenCustomerMovedEventHandler handled CustomerMovedAbroadEvent"), logger.ToString());
            Assert.IsTrue(logger.Contains("CustomerMovedAbroadEventHandler handled CustomerMovedAbroadEvent"), logger.ToString());
        }

        [TestMethod]
        public void MultipleDispatchEventHandler_Always_UsesTransientHandlers()
        {
            // Arrange
            var container = CreateContainer();

            var handler = container.GetInstance<MultipleDispatchEventHandler<CustomerMovedEvent>>();

            // Assert
            Assert.AreEqual(2, handler.Handlers.Count());
            Assert.AreEqual(0, handler.Handlers.Intersect(handler.Handlers).Count(),
                "The wrapped handlers are expected to be registered as transient, but they are singletons.");
        }

        private static Container CreateContainer(ILogger logger = null)
        {
            // Container configuration.
            var container = new Container();

            container.RegisterCollection(typeof(IEventHandler<>), typeof(IEventHandler<>).Assembly);

            container.Register(typeof(IEventHandler<>), typeof(MultipleDispatchEventHandler<>), Lifestyle.Singleton);

            // The ILogger is used by the unit tests to test the configuration.
            container.RegisterSingleton<ILogger>(logger ?? new ListLogger());

            return container;
        }

        public class CustomerMovedEvent
        {
        }

        public class CustomerMovedAbroadEvent : CustomerMovedEvent
        {
        }

        public class SpecialCustomerMovedEvent : CustomerMovedEvent
        {
        }

        public class CustomerMovedEventHandler : IEventHandler<CustomerMovedEvent>
        {
            private readonly ILogger logger;

            public CustomerMovedEventHandler(ILogger logger)
            {
                this.logger = logger;
            }

            public void Handle(CustomerMovedEvent e)
            {
                this.logger.Log(this.GetType().Name + " handled " + e.GetType().Name);
            }
        }

        public class NotifyStaffWhenCustomerMovedEventHandler : IEventHandler<CustomerMovedEvent>
        {
            private readonly ILogger logger;

            public NotifyStaffWhenCustomerMovedEventHandler(ILogger logger)
            {
                this.logger = logger;
            }

            public void Handle(CustomerMovedEvent e)
            {
                this.logger.Log(this.GetType().Name + " handled " + e.GetType().Name);
            }
        }

        public class CustomerMovedAbroadEventHandler : IEventHandler<CustomerMovedAbroadEvent>
        {
            private readonly ILogger logger;

            public CustomerMovedAbroadEventHandler(ILogger logger)
            {
                this.logger = logger;
            }

            public void Handle(CustomerMovedAbroadEvent e)
            {
                this.logger.Log(this.GetType().Name + " handled " + e.GetType().Name);
            }
        }

        public sealed class MultipleDispatchEventHandler<TEvent> : IEventHandler<TEvent>
        {
            public readonly IEnumerable<IEventHandler<TEvent>> Handlers;

            public MultipleDispatchEventHandler(IEnumerable<IEventHandler<TEvent>> handlers)
            {
                this.Handlers = handlers;
            }

            void IEventHandler<TEvent>.Handle(TEvent e)
            {
                foreach (var handler in this.Handlers)
                {
                    handler.Handle(e);
                }
            }
        }
    }
}