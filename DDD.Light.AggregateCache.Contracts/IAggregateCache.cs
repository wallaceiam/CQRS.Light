using System;
using DDD.Light.CQRS.Contracts;
using DDD.Light.EventStore.Contracts;
using System.Threading.Tasks;

namespace DDD.Light.AggregateCache.Contracts
{
    public interface IAggregateCache
    {
        void Configure(IEventStore eventStore, Func<Type, object> getAggregateCacheRepositoryInstance);
        Task<TAggregate> GetById<TAggregate>(Guid id) where TAggregate : IAggregateRoot;
        Task Handle<TAggregate, TEvent>(Guid aggregateId, TEvent @event) where TAggregate : IAggregateRoot;
        void Clear(Guid aggregateId, Type aggregateType);
    }
}
