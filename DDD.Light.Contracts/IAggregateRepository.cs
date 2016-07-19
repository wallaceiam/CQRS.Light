using System;
using DDD.Light.Contracts.CQRS;

namespace DDD.Light.Contracts.AggregateCache
{
    public interface IAggregateRepository<TAggregate> where TAggregate : IAggregateRoot
    {
        void Add(TAggregate aggregate);
        TAggregate GetById(Guid aggregateId);
        void Configure(Func<Type, object> getInstance);
    }

}