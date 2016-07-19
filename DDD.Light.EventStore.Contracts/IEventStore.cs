using System;
using System.Collections.Generic;
using DDD.Light.Repo.Contracts;
using System.Threading.Tasks;

namespace DDD.Light.EventStore.Contracts
{
    public interface IEventStore
    {
        Task<TAggregate> GetById<TAggregate>(Guid id);
        Task<TAggregate> GetById<TAggregate>(Guid id, DateTime until);
        Task<object> GetById(Guid id);
        void Save(AggregateEvent aggregateEvent);
        void Configure(IRepository<AggregateEvent> repo, IEventSerializationStrategy serializationStrategy);
        Task<IEnumerable<AggregateEvent>> GetAll();
        Task<IEnumerable<AggregateEvent>> GetAll(DateTime until);
        long Count();
        Task<DateTime> LatestEventTimestamp(Guid aggregateId);
        Task<IEnumerable<TEvent>> GetEvents<TEvent>();
    }
}