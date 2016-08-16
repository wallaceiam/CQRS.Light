using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CQRS.Light.Core;
using FluentAssertions;
using System.Linq;
using System.Threading.Tasks;
using CQRS.Light.Contracts;
using Moq;
using System.Collections.Generic;

namespace CQRS.Light.Core.Tests
{
    [TestClass]
    public class EventBusTests
    {
        [TestInitialize]
        public void TestInitialize()
        {
            EventBus.Instance.Reset();
            EventHandlersDatabase<string>.Instance.Clear();
        }

        [TestMethod]
        public void EventBus_ShouldBeOnlyOneInstance()
        {
            var a = EventBus.Instance;
            var b = EventBus.Instance;

            a.Should().Be(b);
        }

        [TestMethod]
        public void EventBus_FuncSubscribeShouldAddToEventHandlersDatabase()
        {
            Func<string, Task> foo = (x) => Task.FromResult<string>(x);

            EventHandlersDatabase<string>.Instance.Get().Count().Should().Be(0);

            EventBus.Instance.Subscribe<string>(foo);

            EventHandlersDatabase<string>.Instance.Get().Count().Should().Be(1);
            EventHandlersDatabase<string>.Instance.Get().Where(x => x == foo).Count().Should().Be(1);
        }

        [TestMethod]
        public void EventBus_HandlerSubscribeShouldAddToEventHandlersDatabase()
        {
            var EventHandler = new Mock<IEventHandler<string>>();
            EventHandler.Setup(x => x.HandleAsync(It.IsAny<string>())).Returns(Task.FromResult<string>(null));

            EventHandlersDatabase<string>.Instance.Get().Count().Should().Be(0);

            EventBus.Instance.Subscribe<string>(EventHandler.Object);

            EventHandlersDatabase<string>.Instance.Get().Count().Should().Be(1);
            EventHandlersDatabase<string>.Instance.Get().Where(x => x == EventHandler.Object.HandleAsync).Count().Should().Be(1);
        }

        [TestMethod]
        public void EventBus_ImplicitFuncSubscribeShouldAddToEventHandlersDatabase()
        {
            Func<string, Task> foo = (x) => Task.FromResult<string>(x);

            EventHandlersDatabase<string>.Instance.Get().Count().Should().Be(0);

            EventBus.Instance.Subscribe(foo);

            EventHandlersDatabase<string>.Instance.Get().Count().Should().Be(1);
            EventHandlersDatabase<string>.Instance.Get().Where(x => x == foo).Count().Should().Be(1);
        }

        [TestMethod]
        public void EventBus_ImplicitHandlerSubscribeShouldAddToEventHandlersDatabase()
        {
            var EventHandler = new Mock<IEventHandler<string>>();
            EventHandler.Setup(x => x.HandleAsync(It.IsAny<string>())).Returns(Task.FromResult<string>(null));

            EventHandlersDatabase<string>.Instance.Get().Count().Should().Be(0);

            EventBus.Instance.Subscribe(EventHandler.Object);

            EventHandlersDatabase<string>.Instance.Get().Count().Should().Be(1);
            EventHandlersDatabase<string>.Instance.Get().Where(x => x == EventHandler.Object.HandleAsync).Count().Should().Be(1);
        }

        [TestMethod]
        public void EventBus_ConfigureShouldAssignValues()
        {
            var eventStore = new Mock<IEventStore>();
            var eventSerializationStrat = new Mock<ISerializationStrategy>();

            EventBus.Instance.Configure(eventStore.Object, eventSerializationStrat.Object, true);

            EventBus.Instance.GetEventStore().Should().Be(eventStore.Object);
        }

        [TestMethod]
        public void EventBus_ConfigureShouldNotAllowNulls()
        {
            EventBus.Instance.Invoking(x => x.Configure(null, null, false)).ShouldThrow<ArgumentNullException>();
        }

        [TestMethod]
        public void EventBus_ConfigureShouldReallyNotAllowNulls()
        {
            var eventStore = new Mock<IEventStore>();
            EventBus.Instance.Invoking(x => x.Configure(eventStore.Object, null, false)).ShouldThrow<ArgumentNullException>();
        }

        [TestMethod]
        public void EventBus_ConfigureOverloadShouldAssignValues()
        {
            var eventStore = new Mock<IEventStore>();
            var eventSerializationStrat = new Mock<ISerializationStrategy>();

            EventBus.Instance.Configure(eventStore.Object, eventSerializationStrat.Object);

            EventBus.Instance.GetEventStore().Should().Be(eventStore.Object);
        }

        [TestMethod]
        public void EventBus_ConfigureOverloadShouldNotAllowNulls()
        {
            EventBus.Instance.Invoking(x => x.Configure(null, null)).ShouldThrow<ArgumentNullException>();
        }

        [TestMethod]
        public void EventBus_PublishShouldThrowExceptionIfConfigureHasNotBeenCalled()
        {
            var guid = Guid.NewGuid();
            var testEvent = new TestEvent();

            EventBus.Instance.Awaiting(x => x.PublishAsync<TestEvent>(typeof(TestAggregate), guid, testEvent)).ShouldThrow<InvalidOperationException>();
        }

        [TestMethod]
        public void EventBus_PublishShouldStoreAndRaiseEventsOnTheAggregate()
        {
            var guid = Guid.NewGuid();
            var testEvent = new TestEvent();
            var eventStore = new Mock<IEventStore>();
            eventStore.Setup(x => x.SaveAsync(It.IsAny<AggregateEvent>())).Returns(Task.FromResult<object>(null)).Verifiable();
            var serializationStrategy = new Mock<ISerializationStrategy>();
            serializationStrategy.Setup(x => x.Serialize(testEvent)).Returns("SerializedTestEvent").Verifiable();

            EventBus.Instance.Configure(eventStore.Object, serializationStrategy.Object, true);

            EventBus.Instance.Awaiting(x => x.PublishAsync<TestEvent>(typeof(TestAggregate), guid, testEvent)).ShouldNotThrow();

            eventStore.Verify(x => x.SaveAsync(It.IsAny<AggregateEvent>()), Times.Once);
            serializationStrategy.Verify(x => x.Serialize(testEvent), Times.Once);
        }

        [TestMethod]
        public void EventBus_PublishShouldBubbleExceptionFromEventStore()
        {
            var guid = Guid.NewGuid();
            var testEvent = new TestEvent();
            var eventStore = new Mock<IEventStore>();
            eventStore.Setup(x => x.SaveAsync(It.IsAny<AggregateEvent>())).Throws(new Exception("Oops")).Verifiable();
            var serializationStrategy = new Mock<ISerializationStrategy>();
            serializationStrategy.Setup(x => x.Serialize(testEvent)).Returns("SerializedTestEvent").Verifiable();

            EventBus.Instance.Configure(eventStore.Object, serializationStrategy.Object, true);

            EventBus.Instance.Awaiting(x => x.PublishAsync<TestEvent>(typeof(TestAggregate), guid, testEvent))
                .ShouldThrow<ApplicationException>();

            eventStore.Verify(x => x.SaveAsync(It.IsAny<AggregateEvent>()), Times.Once);
            serializationStrategy.Verify(x => x.Serialize(testEvent), Times.Once);
        }

        [TestMethod]
        public void EventBus_PublishShouldCheckLatestAggregateTimeStampIfConfigured()
        {
            var guid = Guid.NewGuid();
            var testEvent = new TestEvent();
            var eventStore = new Mock<IEventStore>();
            eventStore.Setup(x => x.SaveAsync(It.IsAny<AggregateEvent>())).Returns(Task.FromResult<object>(null)).Verifiable();
            eventStore.Setup(x => x.LatestEventTimestampAsync(guid)).Returns(Task.FromResult<DateTime>(DateTime.UtcNow.AddMinutes(1))).Verifiable();
            var serializationStrategy = new Mock<ISerializationStrategy>();
            serializationStrategy.Setup(x => x.Serialize(testEvent)).Returns("SerializedTestEvent").Verifiable();
            serializationStrategy.Setup(x => x.Serialize(guid)).Returns(guid.ToString()).Verifiable();

            EventBus.Instance.Configure(eventStore.Object, serializationStrategy.Object, true);

            EventBus.Instance.Awaiting(x => x.PublishAsync<TestEvent>(typeof(TestAggregate), guid, testEvent)).ShouldNotThrow();

            eventStore.Verify(x => x.SaveAsync(It.IsAny<AggregateEvent>()), Times.Once);
            eventStore.Verify(x => x.LatestEventTimestampAsync(guid), Times.Once);
            serializationStrategy.Verify(x => x.Serialize(testEvent), Times.Once);
            serializationStrategy.Verify(x => x.Serialize(guid), Times.Never);
        }

        [TestMethod]
        public void EventBus_PublishShouldIgnoreLatestAggregateTimeStampIfConfigured()
        {
            var guid = Guid.NewGuid();
            var testEvent = new TestEvent();
            var eventStore = new Mock<IEventStore>();
            eventStore.Setup(x => x.SaveAsync(It.IsAny<AggregateEvent>())).Returns(Task.FromResult<object>(null)).Verifiable();
            eventStore.Setup(x => x.LatestEventTimestampAsync(guid)).Returns(Task.FromResult<DateTime>(DateTime.UtcNow.AddMinutes(1))).Verifiable();
            var serializationStrategy = new Mock<ISerializationStrategy>();
            serializationStrategy.Setup(x => x.Serialize(testEvent)).Returns("SerializedTestEvent").Verifiable();
            serializationStrategy.Setup(x => x.Serialize(guid)).Returns(guid.ToString()).Verifiable();

            EventBus.Instance.Configure(eventStore.Object, serializationStrategy.Object, false);

            EventBus.Instance.Awaiting(x => x.PublishAsync<TestEvent>(typeof(TestAggregate), guid, testEvent)).ShouldNotThrow();

            eventStore.Verify(x => x.SaveAsync(It.IsAny<AggregateEvent>()), Times.Once);
            eventStore.Verify(x => x.LatestEventTimestampAsync(guid), Times.Exactly(0));
            serializationStrategy.Verify(x => x.Serialize(testEvent), Times.Once);
            serializationStrategy.Verify(x => x.Serialize(guid), Times.Never);
        }

        [TestMethod]
        public void EventBus_PublishGenericShouldThrowExceptionIfConfigureHasNotBeenCalled()
        {
            var guid = Guid.NewGuid();
            var testEvent = new TestEvent();

            EventBus.Instance.Awaiting(x => x.PublishAsync<TestAggregate, TestEvent>(guid, testEvent)).ShouldThrow<InvalidOperationException>();
        }

        [TestMethod]
        public void EventBus_PublishGenericShouldStoreAndRaiseEventsOnTheAggregate()
        {
            var guid = Guid.NewGuid();
            var testEvent = new TestEvent();
            var eventStore = new Mock<IEventStore>();
            eventStore.Setup(x => x.SaveAsync(It.IsAny<AggregateEvent>())).Returns(Task.FromResult<object>(null)).Verifiable();
            var serializationStrategy = new Mock<ISerializationStrategy>();
            serializationStrategy.Setup(x => x.Serialize(testEvent)).Returns("SerializedTestEvent").Verifiable();

            EventBus.Instance.Configure(eventStore.Object, serializationStrategy.Object, true);

            EventBus.Instance.Awaiting(x => x.PublishAsync<TestAggregate, TestEvent>(guid, testEvent)).ShouldNotThrow();

            eventStore.Verify(x => x.SaveAsync(It.IsAny<AggregateEvent>()), Times.Once);
            serializationStrategy.Verify(x => x.Serialize(testEvent), Times.Once);
        }

        [TestMethod]
        public void EventBus_PublishGenericShouldBubbleExceptionFromEventStore()
        {
            var guid = Guid.NewGuid();
            var testEvent = new TestEvent();
            var eventStore = new Mock<IEventStore>();
            eventStore.Setup(x => x.SaveAsync(It.IsAny<AggregateEvent>())).Throws(new Exception("Oops")).Verifiable();
            var serializationStrategy = new Mock<ISerializationStrategy>();
            serializationStrategy.Setup(x => x.Serialize(testEvent)).Returns("SerializedTestEvent").Verifiable();

            EventBus.Instance.Configure(eventStore.Object, serializationStrategy.Object, true);

            EventBus.Instance.Awaiting(x => x.PublishAsync<TestAggregate, TestEvent>(guid, testEvent))
                .ShouldThrow<ApplicationException>();

            eventStore.Verify(x => x.SaveAsync(It.IsAny<AggregateEvent>()), Times.Once);
            serializationStrategy.Verify(x => x.Serialize(testEvent), Times.Once);
        }

        [TestMethod]
        public void EventBus_PublishGenericShouldCheckLatestAggregateTimeStampIfConfigured()
        {
            var guid = Guid.NewGuid();
            var testEvent = new TestEvent();
            var eventStore = new Mock<IEventStore>();
            eventStore.Setup(x => x.SaveAsync(It.IsAny<AggregateEvent>())).Returns(Task.FromResult<object>(null)).Verifiable();
            eventStore.Setup(x => x.LatestEventTimestampAsync(guid)).Returns(Task.FromResult<DateTime>(DateTime.UtcNow.AddMinutes(1))).Verifiable();
            var serializationStrategy = new Mock<ISerializationStrategy>();
            serializationStrategy.Setup(x => x.Serialize(testEvent)).Returns("SerializedTestEvent").Verifiable();
            serializationStrategy.Setup(x => x.Serialize(guid)).Returns(guid.ToString()).Verifiable();

            EventBus.Instance.Configure(eventStore.Object, serializationStrategy.Object, true);

            EventBus.Instance.Awaiting(x => x.PublishAsync<TestAggregate, TestEvent>(guid, testEvent)).ShouldNotThrow();

            eventStore.Verify(x => x.SaveAsync(It.IsAny<AggregateEvent>()), Times.Once);
            eventStore.Verify(x => x.LatestEventTimestampAsync(guid), Times.Once);
            serializationStrategy.Verify(x => x.Serialize(testEvent), Times.Once);
            serializationStrategy.Verify(x => x.Serialize(guid), Times.Never);
        }

        [TestMethod]
        public void EventBus_PublishGenericShouldIgnoreLatestAggregateTimeStampIfConfigured()
        {
            var guid = Guid.NewGuid();
            var testEvent = new TestEvent();
            var eventStore = new Mock<IEventStore>();
            eventStore.Setup(x => x.SaveAsync(It.IsAny<AggregateEvent>())).Returns(Task.FromResult<object>(null)).Verifiable();
            eventStore.Setup(x => x.LatestEventTimestampAsync(guid)).Returns(Task.FromResult<DateTime>(DateTime.UtcNow.AddMinutes(1))).Verifiable();
            var serializationStrategy = new Mock<ISerializationStrategy>();
            serializationStrategy.Setup(x => x.Serialize(testEvent)).Returns("SerializedTestEvent").Verifiable();
            serializationStrategy.Setup(x => x.Serialize(guid)).Returns(guid.ToString()).Verifiable();

            EventBus.Instance.Configure(eventStore.Object, serializationStrategy.Object, false);

            EventBus.Instance.Awaiting(x => x.PublishAsync<TestAggregate, TestEvent>(guid, testEvent)).ShouldNotThrow();

            eventStore.Verify(x => x.SaveAsync(It.IsAny<AggregateEvent>()), Times.Once);
            eventStore.Verify(x => x.LatestEventTimestampAsync(guid), Times.Exactly(0));
            serializationStrategy.Verify(x => x.Serialize(testEvent), Times.Once);
            serializationStrategy.Verify(x => x.Serialize(guid), Times.Never);
        }

        [TestMethod]
        public void EventBus_RestoreReadModelAyncShouldThrowExceptionIfConfigureHasNotBeenCalled()
        {
            EventBus.Instance.Awaiting(x => x.RestoreReadModelAync()).ShouldThrow<InvalidOperationException>();
        }

        [TestMethod]
        public void EventBus_RestoreReadModelUntilAyncShouldThrowExceptionIfConfigureHasNotBeenCalled()
        {
            EventBus.Instance.Awaiting(x => x.RestoreReadModelAync(DateTime.UtcNow.AddMinutes(-1))).ShouldThrow<InvalidOperationException>();
        }

        [TestMethod]
        public void EventBus_RestoreReadModelAyncShouldIterateAndHandleAllTheEvents()
        {
            var eventStore = new Mock<IEventStore>();
            eventStore.Setup(x => x.GetAllAsync()).Returns(Task.FromResult<IEnumerable<AggregateEvent>>(
                    new List<AggregateEvent>()
                    {
                        new AggregateEvent() { AggregateIdType = "guid1", AggregateType = typeof(TestAggregate).AssemblyQualifiedName, CreatedOn = DateTime.UtcNow.AddMinutes(-3), EventType = typeof(TestEvent).AssemblyQualifiedName, Id = Guid.NewGuid(), AggregateId = Guid.NewGuid(), SerializedEvent = "event" },
                        new AggregateEvent() { AggregateIdType = "guid2", AggregateType = typeof(TestAggregate).AssemblyQualifiedName, CreatedOn = DateTime.UtcNow.AddMinutes(-2), EventType = typeof(TestEvent).AssemblyQualifiedName, Id = Guid.NewGuid(), AggregateId = Guid.NewGuid(), SerializedEvent = "event" },
                        new AggregateEvent() { AggregateIdType = "guid3", AggregateType = typeof(TestAggregate).AssemblyQualifiedName, CreatedOn = DateTime.UtcNow.AddMinutes(-1), EventType = typeof(TestEvent).AssemblyQualifiedName, Id = Guid.NewGuid(), AggregateId = Guid.NewGuid(), SerializedEvent = "event" }
                    }
                )).Verifiable();
            var serializationStrategy = new Mock<ISerializationStrategy>();
            serializationStrategy.Setup(x => x.Deserialize(It.IsAny<string>(), typeof(TestEvent))).Returns(new TestEvent()).Verifiable();

            EventBus.Instance.Configure(eventStore.Object, serializationStrategy.Object, true);

            EventBus.Instance.Awaiting(x => x.RestoreReadModelAync()).ShouldNotThrow();

            eventStore.Verify(x => x.GetAllAsync(), Times.Once);
            serializationStrategy.Verify(x => x.Deserialize(It.IsAny<string>(), typeof(TestEvent)), Times.Exactly(3));
        }

        [TestMethod]
        public void EventBus_RestoreReadModelUntilAyncShouldIterateAndHandleAllTheEvents()
        {
            var until = DateTime.UtcNow.AddMinutes(-1);
            var eventStore = new Mock<IEventStore>();
            eventStore.Setup(x => x.GetAllAsync(until)).Returns(Task.FromResult<IEnumerable<AggregateEvent>>(
                    new List<AggregateEvent>()
                    {
                        new AggregateEvent() { AggregateIdType = "guid1", AggregateType = typeof(TestAggregate).AssemblyQualifiedName, CreatedOn = DateTime.UtcNow.AddMinutes(-3), EventType = typeof(TestEvent).AssemblyQualifiedName, Id = Guid.NewGuid(), AggregateId = Guid.NewGuid(), SerializedEvent = "event" },
                        new AggregateEvent() { AggregateIdType = "guid2", AggregateType = typeof(TestAggregate).AssemblyQualifiedName, CreatedOn = DateTime.UtcNow.AddMinutes(-2), EventType = typeof(TestEvent).AssemblyQualifiedName, Id = Guid.NewGuid(), AggregateId = Guid.NewGuid(), SerializedEvent = "event" },
                        new AggregateEvent() { AggregateIdType = "guid3", AggregateType = typeof(TestAggregate).AssemblyQualifiedName, CreatedOn = DateTime.UtcNow.AddMinutes(-1), EventType = typeof(TestEvent).AssemblyQualifiedName, Id = Guid.NewGuid(), AggregateId = Guid.NewGuid(), SerializedEvent = "event" }
                    }
                )).Verifiable();
            var serializationStrategy = new Mock<ISerializationStrategy>();
            serializationStrategy.Setup(x => x.Deserialize(It.IsAny<string>(), typeof(TestEvent))).Returns(new TestEvent()).Verifiable();

            EventBus.Instance.Configure(eventStore.Object, serializationStrategy.Object, true);

            EventBus.Instance.Awaiting(x => x.RestoreReadModelAync(until)).ShouldNotThrow();

            eventStore.Verify(x => x.GetAllAsync(until), Times.Once);
            serializationStrategy.Verify(x => x.Deserialize(It.IsAny<string>(), typeof(TestEvent)), Times.Exactly(3));
        }
    }
}
