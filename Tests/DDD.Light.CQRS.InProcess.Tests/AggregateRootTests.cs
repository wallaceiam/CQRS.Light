using System;
using System.Linq;
using DDD.Light.Contracts.CQRS;
using DDD.Light.Core;
using DDD.Light.Contracts.EventStore;
using DDD.Light.Contracts.Repo;
using DDD.Light.Repo.InMemory;
using NUnit.Framework;

namespace DDD.Light.CQRS.InProcess.Tests
{
    public class SomeAggregateRoot : AggregateRoot<Guid>
    {
        private string _message;
        private SomeAggregateRoot(){}

        public SomeAggregateRoot(Guid id, string message) : base(id)
        {
            PublishAndApplyEvent(new SomeAggregateRootCreated(message));
        }
        
        private void ApplyEvent(SomeAggregateRootCreated @event)
        {
            _message = @event.Message;
        }
    }

    public class SomeAggregateRootCreated
    {
        public string Message { get; private set; }

        public SomeAggregateRootCreated(string message)
        {
            Message = message;
        }
    }

    public class SomeAggregateRootCreatedHandler : EventHandler<SomeAggregateRootCreated>
    {
        public override void Handle(SomeAggregateRootCreated @event)
        {
            // no op
        }
    }

    public class MockHandler : IHandler
    {
        public void Subscribe()
        {
            // no op
        }
    }
    
    [TestFixture]
    public class AggregateRootTests
    {
        [Test]
        public void Should_PublishAndApplyEvent_NonGeneric()
        {
            // Arrange
            var serializationStrategy = new JsonEventSerializationStrategy();

            // if you want to use real database like mongo, use next two lines to configure and then use it to configure event store
//            var mongoAggregateEventsRepository = new MongoRepository<AggregateEvent>("mongodb://localhost", "DDD_Light_Tests_EventStore", "EventStore");
//            mongoAggregateEventsRepository.DeleteAll();

            var inMemoryAggregateEventsRepository = new InMemoryRepository<AggregateEvent>();
            inMemoryAggregateEventsRepository.DeleteAll();

            EventStore.Instance.Configure(inMemoryAggregateEventsRepository, serializationStrategy);
            EventBus.Instance.Configure(EventStore.Instance, serializationStrategy, false);

            Func<Type, object> getInstance = type => 
                {
                    if (type == typeof (SomeAggregateRootCreatedHandler))
                    {
                        return new SomeAggregateRootCreatedHandler();
                    }
                    if (type == typeof(MockHandler))
                    {
                        return new MockHandler();
                    }
                    if (type == typeof(IRepository<SomeAggregateRoot>))
                    {
                        return new InMemoryRepository<SomeAggregateRoot>();
                    }
                    throw new Exception("type " + type.ToString() + " could not be resolved");
                };
            HandlerSubscribtions.SubscribeAllHandlers(getInstance);

            AggregateCache.Instance.Configure(EventStore.Instance, getInstance);
            AggregateBus.Instance.Configure(EventBus.Instance, AggregateCache.Instance);

            const string createdMessage = "hello, I am created!";

            var id = Guid.NewGuid();

            // Act
            var ar = new SomeAggregateRoot(id, createdMessage);

            // Assert
            Assert.AreEqual(1, EventStore.Instance.Count());
            var firstEventType = EventStore.Instance.GetAll().Result.First();
            Assert.AreEqual(typeof(SomeAggregateRootCreated), Type.GetType(firstEventType.EventType));
            Assert.AreEqual(createdMessage, ((SomeAggregateRootCreated)serializationStrategy.DeserializeEvent(firstEventType.SerializedEvent, typeof(SomeAggregateRootCreated))).Message);
        }
    }
}
