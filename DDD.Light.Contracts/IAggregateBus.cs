using DDD.Light.Contracts.AggregateCache;
using DDD.Light.Contracts.CQRS;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DDD.Light.Contracts.AggregateBus
{
    public interface IAggregateBus
    {
        List<IAggregateCache> RegisteredAggregateCaches { get; }
        void Configure(IEventBus eventBus, IAggregateCache aggregateCache);
        void Reset();
        Task PublishAsync<TAggregate, TEvent>(Guid aggregateId, TEvent @event) where TAggregate : IAggregateRoot;
    }
}