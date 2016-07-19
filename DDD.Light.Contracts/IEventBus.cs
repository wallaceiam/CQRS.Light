using System;
using DDD.Light.Contracts.EventStore;
using System.Threading.Tasks;

namespace DDD.Light.Contracts.CQRS
{
    public interface IEventBus
    {
        void Subscribe<T>(IEventHandler<T> handler);
        void Subscribe<T>(Action<T> handler);
        Task Publish<T>(Type aggregateType, Guid aggregateId, T @event);
        Task Publish<TAggregate, T>(Guid aggregateId, T @event);
        void Configure(IEventStore eventStore, IEventSerializationStrategy eventSerializationStrategy, bool checkLatestEventTimestampPriorToSavingToEventStore);
        Task RestoreReadModel();
        Task RestoreReadModel(DateTime until);
        IEventStore GetEventStore();
    }
}