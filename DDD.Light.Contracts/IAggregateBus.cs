using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CQRS.Light.Contracts
{
    public interface IAggregateBus
    {
        List<IAggregateCache> RegisteredAggregateCaches { get; }
        void Configure(IEventBus eventBus, IAggregateCache aggregateCache);
        void Reset();
        Task PublishAsync<TAggregate, TEvent>(Guid aggregateId, TEvent @event) where TAggregate : IAggregateRoot;
    }
}