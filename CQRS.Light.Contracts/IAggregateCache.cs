using System;
using CQRS.Light.Contracts;
using System.Threading.Tasks;

namespace CQRS.Light.Contracts
{
    public interface IAggregateCache
    {
        void Configure(IEventStore eventStore, Func<Type, object> getAggregateCacheRepositoryInstance);
        void Reset();

        IEventStore EventStore { get; }
        Task<TAggregate> GetByIdAsync<TAggregate>(Guid id) where TAggregate : IAggregateRoot;
        Task HandleAsync<TAggregate, TEvent>(Guid aggregateId, TEvent @event) where TAggregate : IAggregateRoot;
        Task ClearAsync<TAggregate>(Guid aggregateId) where TAggregate : IAggregateRoot;
    }
}
