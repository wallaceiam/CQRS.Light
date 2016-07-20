using System;
using DDD.Light.Contracts.CQRS;
using DDD.Light.Contracts.EventStore;
using System.Threading.Tasks;

namespace DDD.Light.Contracts.AggregateCache
{
    public interface IAggregateCache
    {
        void Configure(IEventStore eventStore, Func<Type, object> getAggregateCacheRepositoryInstance);
        Task<TAggregate> GetByIdAsync<TAggregate>(Guid id) where TAggregate : IAggregateRoot;
        Task HandleAsync<TAggregate, TEvent>(Guid aggregateId, TEvent @event) where TAggregate : IAggregateRoot;
        Task ClearAsync(Guid aggregateId, Type aggregateType);
    }
}
