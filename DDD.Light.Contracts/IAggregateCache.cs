using System;
using DDD.Light.Contracts.CQRS;
using DDD.Light.Contracts.EventStore;
using System.Threading.Tasks;

namespace DDD.Light.Contracts.AggregateCache
{
    public interface IAggregateCache
    {
        void Configure(IEventStore eventStore, Func<Type, object> getAggregateCacheRepositoryInstance);
        Task<TAggregate> GetById<TAggregate>(Guid id) where TAggregate : IAggregateRoot;
        Task Handle<TAggregate, TEvent>(Guid aggregateId, TEvent @event) where TAggregate : IAggregateRoot;
        void Clear(Guid aggregateId, Type aggregateType);
    }
}
