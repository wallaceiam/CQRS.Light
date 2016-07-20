using System;
using DDD.Light.Contracts.EventStore;
using System.Threading.Tasks;

namespace DDD.Light.Contracts.CQRS
{
    public interface IEventBus
    {
        void Subscribe<T>(IEventHandler<T> handler);
        void Subscribe<T>(Func<T, Task> handler);
        Task PublishAsync<T>(Type aggregateType, Guid aggregateId, T @event);
        Task PublishAsync<TAggregate, T>(Guid aggregateId, T @event);
        void Configure(IEventStore eventStore, IEventSerializationStrategy eventSerializationStrategy, bool checkLatestEventTimestampPriorToSavingToEventStore);
        Task RestoreReadModelAync();
        Task RestoreReadModelAync(DateTime until);
        IEventStore GetEventStore();
    }
}