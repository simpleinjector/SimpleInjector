namespace SimpleInjector.CodeSamples.Tests.Unit
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using SimpleInjector.Extensions;

    /// <summary>
    /// Variance Scenario 2: Single registration, multiple resolve.
    /// Per service, a single type is registered, but multiple instances are resolved or used behind
    /// the covers.
    /// </summary>
    [TestClass]
    public class VarianceExtensions_AllowToResolveVariantCollectionsTests
    {
        // Each test gets its own test class instance and therefore its own new container and logger.
        private readonly Container container = new Container();
        private readonly ListLogger logger = new ListLogger();

        public VarianceExtensions_AllowToResolveVariantCollectionsTests()
        {
            // Container configuration.
            this.container.Register<IEventHandler<CustomerMovedEvent>, CustomerMovedEventHandler>();
            this.container.Register<IEventHandler<CustomerMovedAbroadEvent>, CustomerMovedAbroadEventHandler>();

            this.container.RegisterSingle<IEventRaiser, EventRaiserImpl>();

            // The ILogger is used by the unit tests to test the configuration.
            this.container.RegisterSingle<ILogger>(this.logger);
        }

        public interface IEventHandler<in TEvent>
        {
            void Handle(TEvent @event);
        }

        public interface IEventRaiser
        {
            void Raise(object @event);
        }

        [TestMethod]
        public void Raise_CustomerMovedAbroadEvent_AllowToResolveVariantCollectionsCalled_ExecutesTwoHandlers()
        {
            // Arrange
            this.container.AllowToResolveVariantCollections();

            // NOTE: EventRaiserImpl<T> takes a dependency on IEnumerable<IEventHandler<T>>.
            var raiser = this.container.GetInstance<IEventRaiser>();

            // Act
            raiser.Raise(new CustomerMovedAbroadEvent());

            // Assert
            Assert.AreEqual(2, this.logger.Count(), "Two handlers were expected.", this.logger.ToString());
            Assert.IsTrue(this.logger.Contains("CustomerMovedAbroadEventHandler handled CustomerMovedAbroadEvent"), this.logger.ToString());
            Assert.IsTrue(this.logger.Contains("CustomerMovedEventHandler handled CustomerMovedAbroadEvent"), this.logger.ToString());
        }

        [TestMethod]
        public void Raise_CustomerMovedAbroadEvent_NoAllowToResolveVariantCollectionsCalled_ExecutesNoHandlers()
        {
            // Arrange
            // NOTE: No AllowToResolveVariantCollections this time.
            // NOTE: EventRaiserImpl<T> takes a dependency on IEnumerable<IEventHandler<T>>.
            var raiser = this.container.GetInstance<IEventRaiser>();

            // Act
            raiser.Raise(new CustomerMovedAbroadEvent());

            // Assert
            Assert.AreEqual(0, this.logger.Count(), "No handler was expected: " + this.logger.ToString());
        }

        [TestMethod]
        public void Raise_CustomerMovedEvent_AllowToResolveVariantCollectionsCalled_ExecutesOneHandlers()
        {
            // Arrange
            this.container.AllowToResolveVariantCollections();

            // NOTE: EventRaiserImpl<T> takes a dependency on IEnumerable<IEventHandler<T>>.
            var raiser = this.container.GetInstance<IEventRaiser>();

            // Act
            raiser.Raise(new CustomerMovedEvent());

            // Assert
            Assert.AreEqual(1, this.logger.Count(), "One handler were expected: " + this.logger.ToString());
            Assert.IsTrue(this.logger.Contains("CustomerMovedEventHandler handled CustomerMovedEvent"), this.logger.ToString());
        }

        [TestMethod]
        public void Raise_CustomerMovedEvent_NoAllowToResolveVariantCollectionsCalled_ExecutesNoHandlers()
        {
            // Arrange
            // NOTE: No AllowToResolveVariantCollections this time.
            // NOTE: EventRaiserImpl<T> takes a dependency on IEnumerable<IEventHandler<T>>.
            var raiser = this.container.GetInstance<IEventRaiser>();

            // Act
            raiser.Raise(new CustomerMovedEvent());

            // Assert
            Assert.AreEqual(0, this.logger.Count(), "No handlers were expected: " + this.logger.ToString());
        }

        public class CustomerMovedEvent
        {
        }

        public class CustomerMovedAbroadEvent : CustomerMovedEvent
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

        public sealed class EventRaiserImpl : IEventRaiser
        {
            private readonly Container container;

            public EventRaiserImpl(Container container)
            {
                this.container = container;
            }

            public void Raise(object @event)
            {
                Type processorType = typeof(GenericEventRaiser<>).MakeGenericType(@event.GetType());

                var processor = (IEventRaiser)this.container.GetInstance(processorType);

                processor.Raise(@event);
            }

            public sealed class GenericEventRaiser<TEvent> : IEventRaiser
            {
                private readonly IEnumerable<IEventHandler<TEvent>> handlers;

                public GenericEventRaiser(IEnumerable<IEventHandler<TEvent>> handlers)
                {
                    this.handlers = handlers;
                }

                public void Raise(object @event)
                {
                    var e = (TEvent)@event;
                    this.handlers.ToList().ForEach(h => h.Handle(e));
                }
            }
        }
    }
}