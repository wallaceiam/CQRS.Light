using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FluentAssertions;
using CQRS.Light.Contracts;
using CQRS.Light.Core;
using System.Threading.Tasks;
using System.Linq;
using Moq;

namespace CQRS.Light.Core.Tests
{
    [TestClass]
    public class AggregateBusTests
    {
        [TestInitialize]
        public void TestInitialize()
        {
            AggregateBus.Instance.Reset();
        }

        [TestMethod]
        public void AggregateBus_ShouldBeOnlyOneInstance()
        {
            var instanceA = AggregateBus.Instance;
            var instanceB = AggregateBus.Instance;

            instanceA.ShouldBeEquivalentTo(instanceB);
        }

        [TestMethod]
        public void AggregateBus_ShouldSubscribeWhenConfigured()
        {
            var eventBus = new Moq.Mock<IEventBus>();
            eventBus.Setup(e => e.Subscribe<AggregateCacheCleared>(Moq.It.IsAny<Func<AggregateCacheCleared, Task>>())).Verifiable();
            var aggregateCache = new Moq.Mock<IAggregateCache>();
            //aggregateCache.Setup(x => x.Clear(Moq.It.IsAny<Guid>(), Moq.It.IsAny<Type>())).Verifiable();

            AggregateBus.Instance.Configure(eventBus.Object, aggregateCache.Object);

            AggregateBus.Instance.RegisteredAggregateCaches.Should().NotBeNull();
            AggregateBus.Instance.RegisteredAggregateCaches.Contains(aggregateCache.Object).Should().BeTrue();

            eventBus.Verify(e => e.Subscribe<AggregateCacheCleared>(Moq.It.IsAny<Func<AggregateCacheCleared, Task>>()), Moq.Times.Once);
        }

        [TestMethod]
        public void AggregateBus_RegisteredAggregateCachesShouldBeEmptyIfConfigureHasntBeenCalled()
        {
            AggregateBus.Instance.RegisteredAggregateCaches.Count.Should().Be(0);
        }

        [TestMethod]
        public void AggregateBus_RegisteredAggregateCachesShouldContainAtLeastTheConfiguredValue()
        {
            var eventBus = new Moq.Mock<IEventBus>();
            eventBus.Setup(e => e.Subscribe<AggregateCacheCleared>(Moq.It.IsAny<Func<AggregateCacheCleared, Task>>())).Verifiable();
            var aggregateCache = new Moq.Mock<IAggregateCache>();

            AggregateBus.Instance.Configure(eventBus.Object, aggregateCache.Object);

            AggregateBus.Instance.RegisteredAggregateCaches.FirstOrDefault().Should().Be(aggregateCache.Object);
        }

        [TestMethod]
        public void AggregateBus_CallingPublishWithoutConfigurationShouldRaiseException()
        {
            var @event = new TestEvent();

            AggregateBus.Instance.Reset();

            AggregateBus.Instance.Awaiting(m => m.PublishAsync<TestAggregate, TestEvent>(Guid.NewGuid(), @event))
                .ShouldThrow<ApplicationException>();
        }


        [TestMethod]
        public void AggregateBus_PublishWillPassToTheEventBus()
        {
            var guid = Guid.NewGuid();
            var @event = new TestEvent();
            var eventBus = new Moq.Mock<IEventBus>();
            eventBus.Setup(m => m.PublishAsync<TestAggregate, TestEvent>(guid, @event))
                .Returns(Task.FromResult<object>(null)).Verifiable();
            var aggregateCache = new Moq.Mock<IAggregateCache>();
            aggregateCache.Setup(x => x.HandleAsync<TestAggregate, TestEvent>(guid, @event))
                .Returns(Task.FromResult<object>(null)).Verifiable();

            AggregateBus.Instance.Configure(eventBus.Object, aggregateCache.Object);
            AggregateBus.Instance.Awaiting(m => m.PublishAsync<TestAggregate, TestEvent>(guid, @event))
                .ShouldNotThrow();

            eventBus.Verify(m => m.PublishAsync<TestAggregate, TestEvent>(guid, @event), Moq.Times.Once);
            aggregateCache.Verify(m => m.HandleAsync<TestAggregate, TestEvent>(guid, @event), Moq.Times.Once);
        }


        [TestMethod]
        public void AggregateBus_ShouldHandleAggregateCacheClearEvent()
        {
            var guid = Guid.NewGuid();
            var eventStore = new Mock<IEventStore>();

            var repo = new Mock<IRepository<TestAggregateRoot>>();
            repo.Setup(x => x.DeleteAsync(guid)).Returns(Task.FromResult<object>(null)).Verifiable();

            var funcWasCalled = false;
            Func<Type, object> func = (x) => { funcWasCalled = true;
                return repo.Object.As<IRepository<TestAggregateRoot>>(); };


            AggregateCache.Instance.Configure(eventStore.Object, func);

            AggregateBus.Instance.Configure(EventBus.Instance, AggregateCache.Instance);
            EventBus.Instance.Configure(eventStore.Object, new JsonSerializationStrategy());

            EventBus.Instance.Awaiting(
                x => x.PublishAsync<AggregateCacheCleared>(typeof(TestAggregateRoot), guid,
                new AggregateCacheCleared(guid, typeof(Guid), typeof(TestAggregateRoot)))
                ).ShouldNotThrow();

            repo.Verify(x => x.DeleteAsync(guid), Times.Once);
        }
    }
}
