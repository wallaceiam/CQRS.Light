using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FluentAssertions;
using Moq;
using CQRS.Light.Contracts;
using CQRS.Light.Core;
using System.Threading.Tasks;

namespace DDD.Light.Core.Tests
{
    [TestClass]
    public class AggregateCacheTests
    {
        [TestInitialize]
        public void TestInitialize()
        {
            AggregateCache.Instance.Reset();
        }

        [TestMethod]
        public void AggregateCache_ShouldBeOnlyOneInstance()
        {
            var a = AggregateCache.Instance;
            var b = AggregateCache.Instance;

            a.ShouldBeEquivalentTo(b);
        }

        [TestMethod]
        public void AggregateCache_ConfigureShouldAssignValues()
        {
            var eventStore = new Mock<IEventStore>();

            var funcWasCalled = false;
            Func<Type, object> func = (x) => { funcWasCalled = true; return Task.FromResult<object>(null); };

            AggregateCache.Instance.Configure(eventStore.Object, func);

            AggregateCache.Instance.EventStore.ShouldBeEquivalentTo(eventStore.Object);

            funcWasCalled.Should().BeFalse();

            AggregateCache.Instance.GetByIdAsync<TestAggregate>(Guid.NewGuid()).ConfigureAwait(true);

            funcWasCalled.Should().BeTrue();
            
        }

        [TestMethod]
        public void AggregateCache_ConfigureDoesntLikeNulls()
        {
            AggregateCache.Instance.Invoking(x => x.Configure(null, null)).ShouldThrow<ArgumentNullException>();
        }

        [TestMethod]
        public void AggregateCache_ConfigureReallyDoesntLikeNulls()
        {
            var eventStore = new Mock<IEventStore>();
            AggregateCache.Instance.Invoking(x => x.Configure(eventStore.Object, null)).ShouldThrow<ArgumentNullException>();
        }

        [TestMethod]
        public void AggregateCache_GetByIdAsyncWithoutConfigureShouldThrowException()
        {
            var guid = Guid.NewGuid();
            var testAggregate = new TestAggregate();

            AggregateCache.Instance.Awaiting(x => x.GetByIdAsync<TestAggregate>(guid)).ShouldThrow<InvalidOperationException>();
        }

        [TestMethod]
        public void AggregateCache_GetByIdAsyncUsesACachedVersion()
        {
            var guid = Guid.NewGuid();
            var testAggregate = new TestAggregate();
            var eventStore = new Mock<IEventStore>();
            eventStore.Setup(x => x.GetByIdAsync<TestAggregate>(guid)).Verifiable();

            var repo = new Mock<IRepository<TestAggregate>>();
            repo.Setup(x => x.GetByIdAsync(guid)).Returns(Task.FromResult<TestAggregate>(testAggregate)).Verifiable();
            var funcWasCalled = false;
            Func<Type, object> func = (x) => { funcWasCalled = true; return repo.Object; };

            AggregateCache.Instance.Configure(eventStore.Object, func);

            TestAggregate result = AggregateCache.Instance.GetByIdAsync<TestAggregate>(guid).Result;

            result.Should().Be(testAggregate);
            funcWasCalled.Should().BeTrue();

            eventStore.Verify(x => x.GetByIdAsync<TestAggregate>(guid), Times.Never);

        }


        [TestMethod]
        public void AggregateCache_GetByIdAsyncUsesTheEventStoreIfNoCache()
        {
            var guid = Guid.NewGuid();
            var testAggregate = new TestAggregate();
            var eventStore = new Mock<IEventStore>();
            eventStore.Setup(x => x.GetByIdAsync<TestAggregate>(guid)).Returns(Task.FromResult<TestAggregate>(testAggregate)).Verifiable();

            var repo = new Mock<IRepository<TestAggregate>>();
            repo.Setup(x => x.GetByIdAsync(guid)).Returns(Task.FromResult<TestAggregate>(null)).Verifiable();
            var funcWasCalled = false;
            Func<Type, object> func = (x) => { funcWasCalled = true; return repo.Object; };

            AggregateCache.Instance.Configure(eventStore.Object, func);

            TestAggregate result = AggregateCache.Instance.GetByIdAsync<TestAggregate>(guid).Result;

            funcWasCalled.Should().BeTrue();

            eventStore.Verify(x => x.GetByIdAsync<TestAggregate>(guid), Times.Once);
            result.Should().Be(testAggregate);

        }

        [TestMethod]
        public void AggregateCache_HandleAsyncWithoutConfigureShouldThrowException()
        {
            var guid = Guid.NewGuid();

            AggregateCache.Instance.Awaiting(x => x.HandleAsync<TestAggregate, TestEvent>(guid, new TestEvent())).ShouldThrow<InvalidOperationException>();
        }

        [TestMethod]
        public void AggregateCache_HandleAsyncUsesTheCachedVersion()
        {
            var guid = Guid.NewGuid();
            var testAggregate = new TestAggregate();
            var eventStore = new Mock<IEventStore>();

            var repo = new Mock<IRepository<TestAggregate>>();
            repo.Setup(x => x.GetByIdAsync(guid)).Returns(Task.FromResult<TestAggregate>(testAggregate)).Verifiable();
            var funcWasCalled = false;
            Func<Type, object> func = (x) => { funcWasCalled = true; return repo.Object; };

            var @event = new TestEvent();

            AggregateCache.Instance.Configure(eventStore.Object, func);

            AggregateCache.Instance.Awaiting(x => x.HandleAsync<TestAggregate, TestEvent>(guid, @event)).ShouldNotThrow();

            funcWasCalled.Should().BeTrue();

            eventStore.Verify(x => x.GetByIdAsync<TestAggregate>(guid), Times.Never);

            testAggregate.WasApplyEventCalled.Should().BeTrue();

        }

        [TestMethod]
        public void AggregateCache_HandleAsyncThrowsIfApplyEventMethodDoesntExistOnAggregate()
        {
            var guid = Guid.NewGuid();
            var testAggregate = new MissingApplyTestAggregate();
            var eventStore = new Mock<IEventStore>();

            var repo = new Mock<IRepository<MissingApplyTestAggregate>>();
            repo.Setup(x => x.GetByIdAsync(guid)).Returns(Task.FromResult<MissingApplyTestAggregate>(testAggregate)).Verifiable();
            var funcWasCalled = false;
            Func<Type, object> func = (x) => { funcWasCalled = true; return repo.Object; };

            var @event = new TestEvent();

            AggregateCache.Instance.Configure(eventStore.Object, func);

            AggregateCache.Instance.Awaiting(x => x.HandleAsync<MissingApplyTestAggregate, TestEvent>(guid, @event)).ShouldThrowExactly<NotImplementedException>();

            funcWasCalled.Should().BeTrue();

            eventStore.Verify(x => x.GetByIdAsync<MissingApplyTestAggregate>(guid), Times.Never);

        }

        [TestMethod]
        public void AggregateCache_HandleAsyncUsesTheEventStoreIfNoCachedVersion()
        {
            var guid = Guid.NewGuid();
            var testAggregate = new TestAggregate();
            var eventStore = new Mock<IEventStore>();
            eventStore.Setup(x => x.GetByIdAsync<TestAggregate>(guid)).Returns(Task.FromResult<TestAggregate>(testAggregate)).Verifiable();

            var repo = new Mock<IRepository<TestAggregate>>();
            repo.Setup(x => x.GetByIdAsync(guid)).Returns(Task.FromResult<TestAggregate>(null)).Verifiable();
            var funcWasCalled = false;
            Func<Type, object> func = (x) => {
                funcWasCalled = true;
                return repo.Object;
            };

            var @event = new TestEvent();

            AggregateCache.Instance.Configure(eventStore.Object, func);

            AggregateCache.Instance.Awaiting(x => x.HandleAsync<TestAggregate, TestEvent>(guid, @event)).ShouldNotThrow();

            funcWasCalled.Should().BeTrue();

            eventStore.Verify(x => x.GetByIdAsync<TestAggregate>(guid), Times.Once);

            testAggregate.WasApplyEventCalled.Should().BeTrue();

        }


        [TestMethod]
        public void AggregateCache_ClearAsyncWithoutConfigureShouldThrowException()
        {
            var guid = Guid.NewGuid();

            AggregateCache.Instance.Awaiting(x => x.ClearAsync<TestAggregate>(guid)).ShouldThrow<InvalidOperationException>();
        }

        [TestMethod]
        public void AggregateCache_ClearAsyncPassesToTheRepo()
        {
            var guid = Guid.NewGuid();
            var repo = new Mock<IRepository<TestAggregate>>();
            repo.Setup(x => x.DeleteAsync(guid)).Returns(Task.FromResult<object>(null)).Verifiable();

            var eventStore = new Mock<IEventStore>();

            var funcWasCalled = false;
            Func<Type, object> func = (x) => {
                funcWasCalled = true;
                return repo.Object;
            };

            AggregateCache.Instance.Configure(eventStore.Object, func);

            AggregateCache.Instance.Awaiting(x => x.ClearAsync<TestAggregate>(guid)).ShouldNotThrow();

            repo.Verify(x => x.DeleteAsync(guid), Times.Once);
            funcWasCalled.Should().BeTrue();
        }
    }
}
