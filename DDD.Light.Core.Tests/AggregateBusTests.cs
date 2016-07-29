using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FluentAssertions;
using CQRS.Light.Contracts;
using CQRS.Light.Core;
using System.Threading.Tasks;
using System.Linq;

namespace DDD.Light.Core.Tests
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
        public void ShouldBeOnlyOneInstanceOfAnAggregateBus()
        {
            var instanceA = AggregateBus.Instance;
            var instanceB = AggregateBus.Instance;

            instanceA.ShouldBeEquivalentTo(instanceB);
        }

        [TestMethod]
        public void AggregateBusShouldSubscribe()
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
        public void RegisteredAggregateCachesShouldBeEmptyIfConfigureHasntBeenCalled()
        {
            AggregateBus.Instance.RegisteredAggregateCaches.Count.Should().Be(0);
        }

        [TestMethod]
        public void RegisteredAggregateCachesShouldContainAtLeastTheConfiguredValue()
        {
            var eventBus = new Moq.Mock<IEventBus>();
            eventBus.Setup(e => e.Subscribe<AggregateCacheCleared>(Moq.It.IsAny<Func<AggregateCacheCleared, Task>>())).Verifiable();
            var aggregateCache = new Moq.Mock<IAggregateCache>();

            AggregateBus.Instance.Configure(eventBus.Object, aggregateCache.Object);

            AggregateBus.Instance.RegisteredAggregateCaches.FirstOrDefault().Should().Be(aggregateCache.Object);
        }

        [TestMethod]
        public void CallingPublishWithoutConfigurationShouldRaiseException()
        {
            var @event = new TestEvent();

            AggregateBus.Instance.Reset();

            AggregateBus.Instance.Awaiting(m => m.PublishAsync<TestAggregate, TestEvent>(Guid.NewGuid(), @event))
                .ShouldThrow<ApplicationException>();
        }


        [TestMethod]
        public void PublishWillPassToTheEventBus()
        {
            var guid = Guid.NewGuid();
            var @event = new TestEvent();
            var eventBus = new Moq.Mock<IEventBus>();
            eventBus.Setup(m => m.PublishAsync<TestAggregate, TestEvent>(guid, @event)).Verifiable();
            var aggregateCache = new Moq.Mock<IAggregateCache>();

            AggregateBus.Instance.Configure(eventBus.Object, aggregateCache.Object);
            AggregateBus.Instance.Awaiting(m => m.PublishAsync<TestAggregate, TestEvent>(guid, @event))
                .ShouldNotThrow<ApplicationException>();

            eventBus.Verify(m => m.PublishAsync<TestAggregate, TestEvent>(guid, @event), Moq.Times.Once);
        }

        protected class TestAggregate : IAggregateRoot
        {
            public Guid Id
            {
                get
                {
                    throw new NotImplementedException();
                }

                set
                {
                    throw new NotImplementedException();
                }
            }

            public Task PublishAndApplyEventAsync<TEvent>(TEvent @event)
            {
                throw new NotImplementedException();
            }

            public Task PublishAndApplyEventAsync<TAggregate, TEvent>(TEvent @event) where TAggregate : IAggregateRoot
            {
                throw new NotImplementedException();
            }
        }
        protected class TestEvent
        {

        }
    }
}
