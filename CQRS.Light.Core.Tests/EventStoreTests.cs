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
            EventStore.Instance.Invoking(x => x.Configure(null, null)).ShouldThrow<ArgumentNullException>();
        }

        [TestMethod]
        public void EventStore_ConfigureShouldNotAllowAnyNulls()
        {
            var repo = new Mock<IRepository<AggregateEvent>>();

            EventStore.Instance.Invoking(x => x.Configure(repo.Object, null)).ShouldThrow<ArgumentNullException>();
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

            EventStore.Instance.Configure(repo.Object, serializationStrategy.Object);

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

            EventStore.Instance.Configure(repo.Object, serializationStrategy.Object);

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

            EventStore.Instance.Configure(repo.Object, serializationStrategy.Object);

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

            EventStore.Instance.Configure(repo.Object, serializationStrategy.Object);

            await EventStore.Instance.SaveAsync(@event);

            repo.Verify(x => x.SaveAsync(@event), Times.Once);
        }
    }
}
