using DDD.Light.Contracts.AggregateCache;
using DDD.Light.Contracts.CQRS;
using System;

namespace DDD.Light.Contracts.AggregateBus
{
    public interface IAggregateBus
    {
        void Configure(IEventBus eventBus, IAggregateCache aggregateCache);
        void Publish<TAggregate, TEvent>(Guid aggregateId, TEvent @event) where TAggregate : IAggregateRoot;
    }
}