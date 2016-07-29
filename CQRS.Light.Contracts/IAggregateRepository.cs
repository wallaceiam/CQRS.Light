using System;

namespace CQRS.Light.Contracts
{
    public interface IAggregateRepository<TAggregate> where TAggregate : IAggregateRoot
    {
        void Add(TAggregate aggregate);
        TAggregate GetById(Guid aggregateId);
        void Configure(Func<Type, object> getInstance);
    }

}