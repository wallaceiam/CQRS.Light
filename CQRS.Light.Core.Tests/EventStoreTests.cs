using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CQRS.Light.Core;
using FluentAssertions;
using CQRS.Light.Contracts;
using Moq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace DDD.Light.Core.Tests
{
    [TestClass]
    public class EventStoreTests
    {
        [TestInitialize]
        public void TestInitialize()
        {
            EventStore.Instance.Reset();
        }

        [TestMethod]
        public void EventStore_ShouldOnlyBeOneInstance()
        {
            var a = EventStore.Instance;
            var b = EventStore.Instance;

            a.Should().Be(b);
        }

        [TestMethod]
        public void EventStore_ConfigureShouldNotAllowNulls()
        {
            EventStore.Instance.Invoking(x => x.Configure(null, null, null)).ShouldThrow<ArgumentNullException>();
        }

        [TestMethod]
        public void EventStore_ConfigureShouldNotAllowAnyNulls()
        {
            var repo = new Mock<IRepository<AggregateEvent>>();

            EventStore.Instance.Invoking(x => x.Configure(repo.Object, null, null))
                .ShouldThrow<ArgumentNullException>();
        }

        [TestMethod]
        public void EventStore_ConfigureShouldReallyNotAllowAnyNulls()
        {
            var repo = new Mock<IRepository<AggregateEvent>>();
            var serializationStrategy = new Mock<ISerializationStrategy>();

            EventStore.Instance.Invoking(x => x.Configure(repo.Object, serializationStrategy.Object, null))
                .ShouldThrow<ArgumentNullException>();
        }

        [TestMethod]
        public void EventStore_GetAllAsyncShouldThrowExceptionIfConfigureHasNotBeenCalled()
        {
            EventStore.Instance.Awaiting(x => x.GetAllAsync()).ShouldThrow<InvalidOperationException>();
        }

        [TestMethod]
        public void EventStore_GetAllAsyncShouldUseTheRepo()
        {
            var repo = new Mock<IRepository<AggregateEvent>>();
            repo.Setup(x => x.GetAllAsync()).Returns(Task.FromResult<IEnumerable<AggregateEvent>>(null)).Verifiable();
            var serializationStrategy = new Mock<ISerializationStrategy>();
            var aggregateBus = new Mock<IAggregateBus>();

            EventStore.Instance.Configure(repo.Object, serializationStrategy.Object, aggregateBus.Object);

            EventStore.Instance.Awaiting(x => x.GetAllAsync()).ShouldNotThrow();
        }

        [TestMethod]
        public void EventStore_GetAllAsyncUntilShouldThrowExceptionIfConfigureHasNotBeenCalled()
        {
            var until = DateTime.UtcNow;
            EventStore.Instance.Awaiting(x => x.GetAllAsync(until)).ShouldThrow<InvalidOperationException>();
        }

        [TestMethod]
        public async Task EventStore_GetAllAsyncUntilShouldUseTheRepo()
        {
            var until = DateTime.UtcNow;
            var repo = new Mock<IRepository<AggregateEvent>>();
            repo.Setup(x => x.GetAsync())
                .Returns(Task.FromResult<IQueryable<AggregateEvent>>(
                    new List<AggregateEvent>() {
                        new AggregateEvent() { CreatedOn = until.AddMinutes(-1) },
                        new AggregateEvent() { CreatedOn = until.AddMinutes(-10) },
                        new AggregateEvent() { CreatedOn = until.AddMinutes(1) },
                    }.AsQueryable()))
                .Verifiable();
            var serializationStrategy = new Mock<ISerializationStrategy>();
            var aggregateBus = new Mock<IAggregateBus>();

            EventStore.Instance.Configure(repo.Object, serializationStrategy.Object, aggregateBus.Object);

            var result = await EventStore.Instance.GetAllAsync(until);

            result.Should().NotBeNull();
            var count = result.Count();
            count.Should().Be(2);
        }

        [TestMethod]
        public void EventStore_CountAsyncShouldThrowExceptionIfConfigureHasNotBeenCalled()
        {
            EventStore.Instance.Awaiting(x => x.CountAsync()).ShouldThrow<InvalidOperationException>();
        }

        [TestMethod]
        public async Task EventStore_CountAsyncShouldUseTheRepo()
        {
            var repo = new Mock<IRepository<AggregateEvent>>();
            repo.Setup(x => x.CountAsync()).Returns(Task.FromResult<long>(1234)).Verifiable();
            var serializationStrategy = new Mock<ISerializationStrategy>();
            var aggregateBus = new Mock<IAggregateBus>();

            EventStore.Instance.Configure(repo.Object, serializationStrategy.Object, aggregateBus.Object);

            var result = await EventStore.Instance.CountAsync();

            result.Should().Be(1234);
        }

        [TestMethod]
        public void EventStore_SaveAsyncShouldThrowExceptionIfConfigureHasNotBeenCalled()
        {
            EventStore.Instance.Awaiting(x => x.SaveAsync(new AggregateEvent())).ShouldThrow<InvalidOperationException>();
        }

        [TestMethod]
        public async Task EventStore_SaveAsyncShouldUseTheRepo()
        {
            var @event = new AggregateEvent();
            var repo = new Mock<IRepository<AggregateEvent>>();
            repo.Setup(x => x.SaveAsync(@event)).Returns(Task.FromResult<object>(null)).Verifiable();
            var serializationStrategy = new Mock<ISerializationStrategy>();
            var aggregateBus = new Mock<IAggregateBus>();

            EventStore.Instance.Configure(repo.Object, serializationStrategy.Object, aggregateBus.Object);

            await EventStore.Instance.SaveAsync(@event);

            repo.Verify(x => x.SaveAsync(@event), Times.Once);
        }

        [TestMethod]
        public void EventStore_GetEventsAsyncShouldThrowExceptionIfConfigureHasNotBeenCalled()
        {
            EventStore.Instance.Awaiting(x => x.GetEventsAsync<TestEvent>()).ShouldThrow<InvalidOperationException>();
        }

        [TestMethod]
        public async Task EventStore_GetEventsAsyncShouldReturnOnlyTEvents()
        {
            var event1 = new AggregateEvent() { EventType = typeof(TestEvent).AssemblyQualifiedName, AggregateId = Guid.NewGuid(), AggregateIdType = typeof(Guid).ToString(), AggregateType = typeof(TestAggregate).AssemblyQualifiedName, CreatedOn = DateTime.UtcNow.AddMinutes(-5), Id = Guid.Empty, SerializedEvent = "{}" };
            var event2 = new AggregateEvent() { EventType = typeof(TestEvent2).AssemblyQualifiedName, AggregateId = Guid.NewGuid(), AggregateIdType = typeof(Guid).ToString(), AggregateType = typeof(TestAggregate).AssemblyQualifiedName, CreatedOn = DateTime.UtcNow.AddMinutes(-4), Id = Guid.Empty, SerializedEvent = "{}" };
            var event3 = new AggregateEvent() { EventType = typeof(TestEvent).AssemblyQualifiedName, AggregateId = Guid.NewGuid(), AggregateIdType = typeof(Guid).ToString(), AggregateType = typeof(TestAggregate).AssemblyQualifiedName, CreatedOn = DateTime.UtcNow.AddMinutes(-3), Id = Guid.Empty, SerializedEvent = "{}" };

            var repo = new Mock<IRepository<AggregateEvent>>();
            repo.Setup(x => x.GetAsync())
                .Returns(Task.FromResult<IQueryable<AggregateEvent>>(
                new List<AggregateEvent>()
                {
                    event1, event2, event3
                }.AsQueryable()))
                .Verifiable();

            var serializationStrategy = new Mock<ISerializationStrategy>();
            serializationStrategy.Setup(x => x.Deserialize("{}", typeof(TestEvent)))
                .Returns(new TestEvent()).Verifiable();
            var aggregateBus = new Mock<IAggregateBus>();

            EventStore.Instance.Configure(repo.Object, serializationStrategy.Object, aggregateBus.Object);

            var result = await EventStore.Instance.GetEventsAsync<TestEvent>();

            result.Should().NotBeNull();
            result.Count().Should().Be(2);

            repo.Verify(x => x.GetAsync(), Times.Once);
            serializationStrategy.Verify(x => x.Deserialize("{}", typeof(TestEvent)), Times.Exactly(2));
        }

        [TestMethod]
        public void EventStore_LatestEventTimestampAsyncShouldThrowExceptionIfConfigureHasNotBeenCalled()
        {
            EventStore.Instance.Awaiting(x => x.LatestEventTimestampAsync(Guid.Empty))
                .ShouldThrow<InvalidOperationException>();
        }

        [TestMethod]
        public async Task EventStore_LatestEventTimestampAsyncShouldUseTheRepoAndMatchTheLatestEventDate()
        {
            var latest = DateTime.UtcNow.AddMinutes(-3);
            var event1 = new AggregateEvent() { EventType = typeof(TestEvent).AssemblyQualifiedName, AggregateId = Guid.Empty, AggregateIdType = typeof(Guid).ToString(), AggregateType = typeof(TestAggregate).AssemblyQualifiedName, CreatedOn = DateTime.UtcNow.AddMinutes(-5), Id = Guid.Empty, SerializedEvent = "{}" };
            var event2 = new AggregateEvent() { EventType = typeof(TestEvent2).AssemblyQualifiedName, AggregateId = Guid.Empty, AggregateIdType = typeof(Guid).ToString(), AggregateType = typeof(TestAggregate).AssemblyQualifiedName, CreatedOn = DateTime.UtcNow.AddMinutes(-4), Id = Guid.Empty, SerializedEvent = "{}" };
            var event3 = new AggregateEvent() { EventType = typeof(TestEvent).AssemblyQualifiedName, AggregateId = Guid.Empty, AggregateIdType = typeof(Guid).ToString(), AggregateType = typeof(TestAggregate).AssemblyQualifiedName, CreatedOn = latest, Id = Guid.Empty, SerializedEvent = "{}" };

            var repo = new Mock<IRepository<AggregateEvent>>();
            repo.Setup(x => x.GetAsync())
                .Returns(Task.FromResult<IQueryable<AggregateEvent>>(
                new List<AggregateEvent>()
                {
                    event1, event2, event3
                }.AsQueryable()))
                .Verifiable();

            var serializationStrategy = new Mock<ISerializationStrategy>();
            var aggregateBus = new Mock<IAggregateBus>();

            EventStore.Instance.Configure(repo.Object, serializationStrategy.Object, aggregateBus.Object);

            var result = await EventStore.Instance.LatestEventTimestampAsync(Guid.Empty);

            result.Should().Be(latest);

            repo.Verify(x => x.GetAsync(), Times.Once);
        }

        [TestMethod]
        public async Task EventStore_LatestEventTimestampAsyncShouldUseTheRepoAndReturnDateTimeMinIfNoEventsAreFound()
        {
            var event1 = new AggregateEvent() { EventType = typeof(TestEvent).AssemblyQualifiedName, AggregateId = Guid.Empty, AggregateIdType = typeof(Guid).ToString(), AggregateType = typeof(TestAggregate).AssemblyQualifiedName, CreatedOn = DateTime.UtcNow.AddMinutes(-5), Id = Guid.Empty, SerializedEvent = "{}" };
            var event2 = new AggregateEvent() { EventType = typeof(TestEvent2).AssemblyQualifiedName, AggregateId = Guid.Empty, AggregateIdType = typeof(Guid).ToString(), AggregateType = typeof(TestAggregate).AssemblyQualifiedName, CreatedOn = DateTime.UtcNow.AddMinutes(-4), Id = Guid.Empty, SerializedEvent = "{}" };
            var event3 = new AggregateEvent() { EventType = typeof(TestEvent).AssemblyQualifiedName, AggregateId = Guid.Empty, AggregateIdType = typeof(Guid).ToString(), AggregateType = typeof(TestAggregate).AssemblyQualifiedName, CreatedOn = DateTime.UtcNow.AddMinutes(-3), Id = Guid.Empty, SerializedEvent = "{}" };

            var repo = new Mock<IRepository<AggregateEvent>>();
            repo.Setup(x => x.GetAsync())
                .Returns(Task.FromResult<IQueryable<AggregateEvent>>(
                new List<AggregateEvent>()
                {
                    event1, event2, event3
                }.AsQueryable()))
                .Verifiable();

            var serializationStrategy = new Mock<ISerializationStrategy>();
            var aggregateBus = new Mock<IAggregateBus>();

            EventStore.Instance.Configure(repo.Object, serializationStrategy.Object, aggregateBus.Object);

            var result = await EventStore.Instance.LatestEventTimestampAsync(Guid.NewGuid());

            result.Should().Be(DateTime.MinValue);

            repo.Verify(x => x.GetAsync(), Times.Once);
        }

        [TestMethod]
        public void EventStore_GetByIdAsyncShouldThrowExceptionIfConfigureHasNotBeenCalled()
        {
            EventStore.Instance.Awaiting(x => x.GetByIdAsync<TestAggregate>(Guid.Empty))
                .ShouldThrow<InvalidOperationException>();
        }

        [TestMethod]
        public async Task EventStore_GetByIdAsyncShouldReturnDefaultTForNoEvents()
        {
            var event1 = new AggregateEvent() { EventType = typeof(TestEvent).AssemblyQualifiedName, AggregateId = Guid.Empty, AggregateIdType = typeof(Guid).ToString(), AggregateType = typeof(TestAggregate).AssemblyQualifiedName, CreatedOn = DateTime.UtcNow.AddMinutes(-5), Id = Guid.Empty, SerializedEvent = "{}" };
            var event2 = new AggregateEvent() { EventType = typeof(TestEvent).AssemblyQualifiedName, AggregateId = Guid.Empty, AggregateIdType = typeof(Guid).ToString(), AggregateType = typeof(TestAggregate).AssemblyQualifiedName, CreatedOn = DateTime.UtcNow.AddMinutes(-4), Id = Guid.Empty, SerializedEvent = "{}" };
            var event3 = new AggregateEvent() { EventType = typeof(TestEvent).AssemblyQualifiedName, AggregateId = Guid.Empty, AggregateIdType = typeof(Guid).ToString(), AggregateType = typeof(TestAggregate).AssemblyQualifiedName, CreatedOn = DateTime.UtcNow.AddMinutes(-3), Id = Guid.Empty, SerializedEvent = "{}" };

            var repo = new Mock<IRepository<AggregateEvent>>();
            repo.Setup(x => x.GetAsync())
                .Returns(Task.FromResult<IQueryable<AggregateEvent>>(
                new List<AggregateEvent>()
                {
                    event1, event2, event3
                }.AsQueryable()))
                .Verifiable();

            var serializationStrategy = new Mock<ISerializationStrategy>();
            serializationStrategy.Setup(x => x.Deserialize("{}", typeof(TestEvent)))
                .Returns(new TestEvent()).Verifiable();
            var aggregateBus = new Mock<IAggregateBus>();

            EventStore.Instance.Configure(repo.Object, serializationStrategy.Object, aggregateBus.Object);

            var result = await EventStore.Instance.GetByIdAsync<TestAggregateRoot>(Guid.Empty);

            result.Should().NotBeNull();
            result.WasApplyEventCalled.Should().BeTrue();

            repo.Verify(x => x.GetAsync(), Times.Once);
            serializationStrategy.Verify(x => x.Deserialize("{}", typeof(TestEvent)), Times.Exactly(3));

        }

        [TestMethod]
        public void EventStore_GetByIdAsyncShouldThrowExceptionIfNoAcceptableConstructorIsFound()
        {
           var repo = new Mock<IRepository<AggregateEvent>>();

            var serializationStrategy = new Mock<ISerializationStrategy>();
            var aggregateBus = new Mock<IAggregateBus>();

            EventStore.Instance.Configure(repo.Object, serializationStrategy.Object, aggregateBus.Object);

            EventStore.Instance.Awaiting(x => x.GetByIdAsync<MissingConstructorAggregateRoot>(Guid.Empty))
                .ShouldThrow<NotImplementedException>();
        }

        [TestMethod]
        public void EventStore_GetByIdAsyncShouldThrowExceptionIfNoAcceptableApplyEventMethodIsFound()
        {
            var event1 = new AggregateEvent() { EventType = typeof(TestEvent).AssemblyQualifiedName, AggregateId = Guid.Empty, AggregateIdType = typeof(Guid).ToString(), AggregateType = typeof(TestAggregate).AssemblyQualifiedName, CreatedOn = DateTime.UtcNow.AddMinutes(-5), Id = Guid.Empty, SerializedEvent = "{}" };
            var event2 = new AggregateEvent() { EventType = typeof(TestEvent2).AssemblyQualifiedName, AggregateId = Guid.Empty, AggregateIdType = typeof(Guid).ToString(), AggregateType = typeof(TestAggregate).AssemblyQualifiedName, CreatedOn = DateTime.UtcNow.AddMinutes(-4), Id = Guid.Empty, SerializedEvent = "{}" };

            var repo = new Mock<IRepository<AggregateEvent>>();
            repo.Setup(x => x.GetAsync())
                .Returns(Task.FromResult<IQueryable<AggregateEvent>>(
                new List<AggregateEvent>()
                {
                    event1, event2
                }.AsQueryable()))
                .Verifiable();

            var serializationStrategy = new Mock<ISerializationStrategy>();
            serializationStrategy.Setup(x => x.Deserialize("{}", typeof(TestEvent)))
                .Returns(new TestEvent()).Verifiable();
            var aggregateBus = new Mock<IAggregateBus>();

            EventStore.Instance.Configure(repo.Object, serializationStrategy.Object, aggregateBus.Object);

            EventStore.Instance.Awaiting(x => x.GetByIdAsync<TestAggregateRoot>(Guid.Empty))
                .ShouldThrow<NotImplementedException>();
            

            repo.Verify(x => x.GetAsync(), Times.Once);
            serializationStrategy.Verify(x => x.Deserialize("{}", typeof(TestEvent)), Times.Once);
            serializationStrategy.Verify(x => x.Deserialize("{}", typeof(TestEvent2)), Times.Once);

        }

        
        [TestMethod]
        public void EventStore_GetByIdUntilAsyncShouldThrowExceptionIfConfigureHasNotBeenCalled()
        {
            var until = DateTime.UtcNow.AddMinutes(-5);

            EventStore.Instance.Awaiting(x => x.GetByIdAsync<TestAggregate>(Guid.Empty, until))
                .ShouldThrow<InvalidOperationException>();
        }

        [TestMethod]
        public async Task EventStore_GetByIdUntilAsyncShouldReturnDefaultTForNoEvents()
        {
            var until = DateTime.UtcNow.AddMinutes(-5);
            var event1 = new AggregateEvent() { EventType = typeof(TestEvent).AssemblyQualifiedName, AggregateId = Guid.Empty, AggregateIdType = typeof(Guid).ToString(), AggregateType = typeof(TestAggregate).AssemblyQualifiedName, CreatedOn = DateTime.UtcNow.AddMinutes(-5), Id = Guid.Empty, SerializedEvent = "{}" };
            var event2 = new AggregateEvent() { EventType = typeof(TestEvent).AssemblyQualifiedName, AggregateId = Guid.Empty, AggregateIdType = typeof(Guid).ToString(), AggregateType = typeof(TestAggregate).AssemblyQualifiedName, CreatedOn = DateTime.UtcNow.AddMinutes(-4), Id = Guid.Empty, SerializedEvent = "{}" };
            var event3 = new AggregateEvent() { EventType = typeof(TestEvent).AssemblyQualifiedName, AggregateId = Guid.Empty, AggregateIdType = typeof(Guid).ToString(), AggregateType = typeof(TestAggregate).AssemblyQualifiedName, CreatedOn = DateTime.UtcNow.AddMinutes(-3), Id = Guid.Empty, SerializedEvent = "{}" };

            var repo = new Mock<IRepository<AggregateEvent>>();
            repo.Setup(x => x.GetAsync())
                .Returns(Task.FromResult<IQueryable<AggregateEvent>>(
                new List<AggregateEvent>()
                {
                    event1, event2, event3
                }.AsQueryable()))
                .Verifiable();

            var serializationStrategy = new Mock<ISerializationStrategy>();
            serializationStrategy.Setup(x => x.Deserialize("{}", typeof(TestEvent)))
                .Returns(new TestEvent()).Verifiable();
            var aggregateBus = new Mock<IAggregateBus>();

            EventStore.Instance.Configure(repo.Object, serializationStrategy.Object, aggregateBus.Object);

            var result = await EventStore.Instance.GetByIdAsync<TestAggregateRoot>(Guid.Empty, until);

            result.Should().NotBeNull();
            result.WasApplyEventCalled.Should().BeTrue();

            repo.Verify(x => x.GetAsync(), Times.Once);
            serializationStrategy.Verify(x => x.Deserialize("{}", typeof(TestEvent)), Times.Exactly(3));

        }

        [TestMethod]
        public void EventStore_GetByIdUntilAsyncShouldThrowExceptionIfNoAcceptableConstructorIsFound()
        {
            var until = DateTime.UtcNow.AddMinutes(-5);
            var repo = new Mock<IRepository<AggregateEvent>>();

            var serializationStrategy = new Mock<ISerializationStrategy>();
            var aggregateBus = new Mock<IAggregateBus>();

            EventStore.Instance.Configure(repo.Object, serializationStrategy.Object, aggregateBus.Object);

            EventStore.Instance.Awaiting(x => x.GetByIdAsync<MissingConstructorAggregateRoot>(Guid.Empty, until))
                .ShouldThrow<NotImplementedException>();
        }

        [TestMethod]
        public void EventStore_GetByIdUntilAsyncShouldThrowExceptionIfNoAcceptableApplyEventMethodIsFound()
        {
            var until = DateTime.UtcNow.AddMinutes(-5);
            var event1 = new AggregateEvent() { EventType = typeof(TestEvent).AssemblyQualifiedName, AggregateId = Guid.Empty, AggregateIdType = typeof(Guid).ToString(), AggregateType = typeof(TestAggregate).AssemblyQualifiedName, CreatedOn = DateTime.UtcNow.AddMinutes(-5), Id = Guid.Empty, SerializedEvent = "{}" };
            var event2 = new AggregateEvent() { EventType = typeof(TestEvent2).AssemblyQualifiedName, AggregateId = Guid.Empty, AggregateIdType = typeof(Guid).ToString(), AggregateType = typeof(TestAggregate).AssemblyQualifiedName, CreatedOn = DateTime.UtcNow.AddMinutes(-4), Id = Guid.Empty, SerializedEvent = "{}" };

            var repo = new Mock<IRepository<AggregateEvent>>();
            repo.Setup(x => x.GetAsync())
                .Returns(Task.FromResult<IQueryable<AggregateEvent>>(
                new List<AggregateEvent>()
                {
                    event1, event2
                }.AsQueryable()))
                .Verifiable();

            var serializationStrategy = new Mock<ISerializationStrategy>();
            serializationStrategy.Setup(x => x.Deserialize("{}", typeof(TestEvent)))
                .Returns(new TestEvent()).Verifiable();
            var aggregateBus = new Mock<IAggregateBus>();

            EventStore.Instance.Configure(repo.Object, serializationStrategy.Object, aggregateBus.Object);

            EventStore.Instance.Awaiting(x => x.GetByIdAsync<TestAggregateRoot>(Guid.Empty, until))
                .ShouldThrow<NotImplementedException>();


            repo.Verify(x => x.GetAsync(), Times.Once);
            serializationStrategy.Verify(x => x.Deserialize("{}", typeof(TestEvent)), Times.Once);
            serializationStrategy.Verify(x => x.Deserialize("{}", typeof(TestEvent2)), Times.Once);

        }

        [TestMethod]
        public void EventStore_GetByIdObjectAsyncShouldThrowExceptionIfConfigureHasNotBeenCalled()
        {
            EventStore.Instance.Awaiting(x => x.GetByIdAsync(Guid.Empty))
                .ShouldThrow<InvalidOperationException>();
        }

        [TestMethod]
        public async Task EventStore_GetByIdObjectAsyncShouldReturnDefaultTForNoEvents()
        {
            var event1 = new AggregateEvent() { EventType = typeof(TestEvent).AssemblyQualifiedName, AggregateId = Guid.Empty, AggregateIdType = typeof(Guid).ToString(), AggregateType = typeof(TestAggregateRoot).AssemblyQualifiedName, CreatedOn = DateTime.UtcNow.AddMinutes(-5), Id = Guid.Empty, SerializedEvent = "{}" };
            var event2 = new AggregateEvent() { EventType = typeof(TestEvent).AssemblyQualifiedName, AggregateId = Guid.Empty, AggregateIdType = typeof(Guid).ToString(), AggregateType = typeof(TestAggregateRoot).AssemblyQualifiedName, CreatedOn = DateTime.UtcNow.AddMinutes(-4), Id = Guid.Empty, SerializedEvent = "{}" };
            var event3 = new AggregateEvent() { EventType = typeof(TestEvent).AssemblyQualifiedName, AggregateId = Guid.Empty, AggregateIdType = typeof(Guid).ToString(), AggregateType = typeof(TestAggregateRoot).AssemblyQualifiedName, CreatedOn = DateTime.UtcNow.AddMinutes(-3), Id = Guid.Empty, SerializedEvent = "{}" };

            var repo = new Mock<IRepository<AggregateEvent>>();
            repo.Setup(x => x.GetAsync())
                .Returns(Task.FromResult<IQueryable<AggregateEvent>>(
                new List<AggregateEvent>()
                {
                    event1, event2, event3
                }.AsQueryable()))
                .Verifiable();

            var serializationStrategy = new Mock<ISerializationStrategy>();
            serializationStrategy.Setup(x => x.Deserialize("{}", typeof(TestEvent)))
                .Returns(new TestEvent()).Verifiable();
            var aggregateBus = new Mock<IAggregateBus>();

            EventStore.Instance.Configure(repo.Object, serializationStrategy.Object, aggregateBus.Object);

            var result = await EventStore.Instance.GetByIdAsync(Guid.Empty);

            result.Should().NotBeNull();
            result.Should().BeOfType(typeof(TestAggregateRoot));
            var aggregateRoot = result as TestAggregateRoot;
            aggregateRoot.Should().NotBeNull();
            aggregateRoot.WasApplyEventCalled.Should().BeTrue();

            repo.Verify(x => x.GetAsync(), Times.Exactly(2));
            serializationStrategy.Verify(x => x.Deserialize("{}", typeof(TestEvent)), Times.Exactly(3));

        }

        [TestMethod]
        public void EventStore_GetByIdObjectAsyncShouldThrowExceptionIfNoAcceptableConstructorIsFound()
        {
            var event1 = new AggregateEvent() { EventType = typeof(TestEvent).AssemblyQualifiedName, AggregateId = Guid.Empty, AggregateIdType = typeof(Guid).ToString(), AggregateType = typeof(TestAggregate).AssemblyQualifiedName, CreatedOn = DateTime.UtcNow.AddMinutes(-5), Id = Guid.Empty, SerializedEvent = "{}" };
            var event2 = new AggregateEvent() { EventType = typeof(TestEvent2).AssemblyQualifiedName, AggregateId = Guid.Empty, AggregateIdType = typeof(Guid).ToString(), AggregateType = typeof(TestAggregate).AssemblyQualifiedName, CreatedOn = DateTime.UtcNow.AddMinutes(-4), Id = Guid.Empty, SerializedEvent = "{}" };

            var repo = new Mock<IRepository<AggregateEvent>>();
            repo.Setup(x => x.GetAsync())
                .Returns(Task.FromResult<IQueryable<AggregateEvent>>(
                new List<AggregateEvent>()
                {
                    event1, event2
                }.AsQueryable()))
                .Verifiable();

            var serializationStrategy = new Mock<ISerializationStrategy>();
            var aggregateBus = new Mock<IAggregateBus>();

            EventStore.Instance.Configure(repo.Object, serializationStrategy.Object, aggregateBus.Object);

            EventStore.Instance.Awaiting(x => x.GetByIdAsync(Guid.Empty))
                .ShouldThrow<NotImplementedException>();
        }

        [TestMethod]
        public void EventStore_GetByIdObjectAsyncShouldThrowExceptionIfNoAcceptableApplyEventMethodIsFound()
        {
            var event1 = new AggregateEvent() { EventType = typeof(TestEvent).AssemblyQualifiedName, AggregateId = Guid.Empty, AggregateIdType = typeof(Guid).ToString(), AggregateType = typeof(TestAggregateRoot).AssemblyQualifiedName, CreatedOn = DateTime.UtcNow.AddMinutes(-5), Id = Guid.Empty, SerializedEvent = "{}" };
            var event2 = new AggregateEvent() { EventType = typeof(TestEvent2).AssemblyQualifiedName, AggregateId = Guid.Empty, AggregateIdType = typeof(Guid).ToString(), AggregateType = typeof(TestAggregateRoot).AssemblyQualifiedName, CreatedOn = DateTime.UtcNow.AddMinutes(-4), Id = Guid.Empty, SerializedEvent = "{}" };

            var repo = new Mock<IRepository<AggregateEvent>>();
            repo.Setup(x => x.GetAsync())
                .Returns(Task.FromResult<IQueryable<AggregateEvent>>(
                new List<AggregateEvent>()
                {
                    event1, event2
                }.AsQueryable()))
                .Verifiable();

            var serializationStrategy = new Mock<ISerializationStrategy>();
            serializationStrategy.Setup(x => x.Deserialize("{}", typeof(TestEvent)))
                .Returns(new TestEvent()).Verifiable();
            serializationStrategy.Setup(x => x.Deserialize("{}", typeof(TestEvent2)))
                .Returns(new TestEvent2()).Verifiable();
            var aggregateBus = new Mock<IAggregateBus>();

            EventStore.Instance.Configure(repo.Object, serializationStrategy.Object, aggregateBus.Object);

            EventStore.Instance.Awaiting(x => x.GetByIdAsync(Guid.Empty))
                .ShouldThrow<NotImplementedException>();


            repo.Verify(x => x.GetAsync(), Times.Exactly(2));
            serializationStrategy.Verify(x => x.Deserialize("{}", typeof(TestEvent)), Times.Once);
            serializationStrategy.Verify(x => x.Deserialize("{}", typeof(TestEvent2)), Times.Once);

        }

        [TestMethod]
        public async Task EventStore_GetByIdObjectAsyncShouldReturnNullIfNoEvents()
        {
            var event1 = new AggregateEvent() { EventType = typeof(TestEvent).AssemblyQualifiedName, AggregateId = Guid.Empty, AggregateIdType = typeof(Guid).ToString(), AggregateType = typeof(TestAggregateRoot).AssemblyQualifiedName, CreatedOn = DateTime.UtcNow.AddMinutes(-5), Id = Guid.Empty, SerializedEvent = "{}" };
            var event2 = new AggregateEvent() { EventType = typeof(TestEvent).AssemblyQualifiedName, AggregateId = Guid.Empty, AggregateIdType = typeof(Guid).ToString(), AggregateType = typeof(TestAggregateRoot).AssemblyQualifiedName, CreatedOn = DateTime.UtcNow.AddMinutes(-4), Id = Guid.Empty, SerializedEvent = "{}" };
            var event3 = new AggregateEvent() { EventType = typeof(TestEvent).AssemblyQualifiedName, AggregateId = Guid.Empty, AggregateIdType = typeof(Guid).ToString(), AggregateType = typeof(TestAggregateRoot).AssemblyQualifiedName, CreatedOn = DateTime.UtcNow.AddMinutes(-3), Id = Guid.Empty, SerializedEvent = "{}" };

            var repo = new Mock<IRepository<AggregateEvent>>();
            repo.Setup(x => x.GetAsync())
                .Returns(Task.FromResult<IQueryable<AggregateEvent>>(
                new List<AggregateEvent>()
                {
                    event1, event2, event3
                }.AsQueryable()))
                .Verifiable();

            var serializationStrategy = new Mock<ISerializationStrategy>();
            serializationStrategy.Setup(x => x.Deserialize("{}", typeof(TestEvent)))
                .Returns(new TestEvent()).Verifiable();
            var aggregateBus = new Mock<IAggregateBus>();

            EventStore.Instance.Configure(repo.Object, serializationStrategy.Object, aggregateBus.Object);

            var result = await EventStore.Instance.GetByIdAsync(Guid.NewGuid());

            result.Should().BeNull();
            
            repo.Verify(x => x.GetAsync(), Times.Exactly(1));
            serializationStrategy.Verify(x => x.Deserialize("{}", typeof(TestEvent)), Times.Never);

        }

        [TestMethod]
        public async Task EventStore_GetByIdObjectAsyncShouldReturnNullIfEvenContainsAndInvalidAggregateType()
        {
            var event1 = new AggregateEvent() { EventType = typeof(TestEvent).AssemblyQualifiedName, AggregateId = Guid.Empty, AggregateIdType = typeof(Guid).ToString(), AggregateType = "SomeRandomAggregateRoot", CreatedOn = DateTime.UtcNow.AddMinutes(-5), Id = Guid.Empty, SerializedEvent = "{}" };
            var event2 = new AggregateEvent() { EventType = typeof(TestEvent).AssemblyQualifiedName, AggregateId = Guid.Empty, AggregateIdType = typeof(Guid).ToString(), AggregateType = "SomeRandomAggregateRoot", CreatedOn = DateTime.UtcNow.AddMinutes(-4), Id = Guid.Empty, SerializedEvent = "{}" };
            var event3 = new AggregateEvent() { EventType = typeof(TestEvent).AssemblyQualifiedName, AggregateId = Guid.Empty, AggregateIdType = typeof(Guid).ToString(), AggregateType = "SomeRandomAggregateRoot", CreatedOn = DateTime.UtcNow.AddMinutes(-3), Id = Guid.Empty, SerializedEvent = "{}" };

            var repo = new Mock<IRepository<AggregateEvent>>();
            repo.Setup(x => x.GetAsync())
                .Returns(Task.FromResult<IQueryable<AggregateEvent>>(
                new List<AggregateEvent>()
                {
                    event1, event2, event3
                }.AsQueryable()))
                .Verifiable();

            var serializationStrategy = new Mock<ISerializationStrategy>();
            serializationStrategy.Setup(x => x.Deserialize("{}", typeof(TestEvent)))
                .Returns(new TestEvent()).Verifiable();
            var aggregateBus = new Mock<IAggregateBus>();

            EventStore.Instance.Configure(repo.Object, serializationStrategy.Object, aggregateBus.Object);

            var result = await EventStore.Instance.GetByIdAsync(Guid.Empty);

            result.Should().BeNull();

            repo.Verify(x => x.GetAsync(), Times.Exactly(1));
            serializationStrategy.Verify(x => x.Deserialize("{}", typeof(TestEvent)), Times.Never);

        }
    }
}
