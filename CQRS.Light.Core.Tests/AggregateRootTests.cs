using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CQRS.Light.Core;
using FluentAssertions;
using CQRS.Light.Contracts;
using Moq;
using System.Threading.Tasks;

namespace CQRS.Light.Core.Tests
{
    [TestClass]
    public class AggregateRootTests
    {
        [TestInitialize]
        public void TestInitialize()
        {
            AggregateBus.Instance.Reset();
        }

        [TestMethod]
        public void AggregateRoot_ConstructorShouldAssignValues()
        {
            var guid = Guid.NewGuid();
            var aggregateBus = new Mock<IAggregateBus>();
            var testAggreageRoot = new TestAggregateRoot(aggregateBus.Object, guid);

            testAggreageRoot.Id.Should().Be(guid);
        }

        [TestMethod]
        public void AggregateRoot_ConstructorShouldNotLikeNullAggregateBus()
        {
            var guid = Guid.NewGuid();
            Action a = () => new TestAggregateRoot(null, guid);

            a.ShouldThrow<ArgumentNullException>();

            
        }

        [TestMethod]
        public void AggregateRoot_PublishAndApplyEventAsyncShouldThrowExceptionIfApplyEventMethodDoesntExist()
        {
            var guid = Guid.NewGuid();
            var @event = new TestEvent2();
            var aggregateBus = new Mock<IAggregateBus>();
            aggregateBus.Setup(x => x.PublishAsync<TestAggregateRoot, TestEvent2>(guid, @event)).Returns(Task.FromResult<object>(null)).Verifiable();
            var testAggregateRoot = new TestAggregateRoot(aggregateBus.Object, guid);

            testAggregateRoot.Awaiting(x => x.PublishAndApplyEventAsync<TestAggregateRoot, TestEvent2>(@event)).ShouldThrow<ApplicationException>();

            testAggregateRoot.WasApplyEventCalled.Should().BeFalse();
        }

        [TestMethod]
        public void AggregateRoot_PublishAndApplyEventAsyncShouldCallApplyEventIfExists()
        {
            var guid = Guid.NewGuid();
            var @event = new TestEvent2();
            var aggregateBus = new Mock<IAggregateBus>();
            aggregateBus.Setup(x => x.PublishAsync<TestAggregateRoot, TestEvent2>(guid, @event)).Returns(Task.FromResult<object>(null)).Verifiable();
            var testAggregateRoot = new TestAggregateRoot(aggregateBus.Object, guid);

            testAggregateRoot.Awaiting(x => x.PublishAndApplyEventAsync<TestAggregateRoot, TestEvent>(new TestEvent())).ShouldNotThrow();

            testAggregateRoot.WasApplyEventCalled.Should().BeTrue();
        }

        [TestMethod]
        public void AggregateRoot_PublishAndApplyEventAsync2ShouldThrowExceptionIfApplyEventMethodDoesntExist()
        {
            var guid = Guid.NewGuid();
            var @event = new TestEvent2();
            var aggregateBus = new Mock<IAggregateBus>();
            aggregateBus.Setup(x => x.PublishAsync<TestAggregateRoot, TestEvent2>(guid, @event)).Returns(Task.FromResult<object>(null)).Verifiable();
            var testAggregateRoot = new TestAggregateRoot(aggregateBus.Object, guid);

            testAggregateRoot.Awaiting(x => x.PublishAndApplyEventAsync<TestEvent2>(@event)).ShouldThrow<ApplicationException>();

            testAggregateRoot.WasApplyEventCalled.Should().BeFalse();
        }

        [TestMethod]
        public void AggregateRoot_PublishAndApplyEventAsync2ShouldCallApplyEventIfExists()
        {
            var guid = Guid.NewGuid();
            var eventBus = new Mock<IEventBus>();
            var aggregateBus = AggregateBus.Instance;
            aggregateBus.Configure(eventBus.Object);
            //aggregateBus.Setup(x => x.PublishAsync<TestAggregateRoot,TestEvent2>(guid, @event)).Returns(Task.FromResult<object>(null)).Verifiable();
            var testAggregateRoot = new TestAggregateRoot(aggregateBus, guid);

            testAggregateRoot.Awaiting(x => x.PublishAndApplyEventAsync<TestEvent>(new TestEvent())).ShouldNotThrow();

            testAggregateRoot.WasApplyEventCalled.Should().BeTrue();
        }
    }

}
