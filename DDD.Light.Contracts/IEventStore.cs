using System;
using System.Collections.Generic;
using DDD.Light.Contracts.Repo;
using System.Threading.Tasks;

namespace DDD.Light.Contracts.EventStore
{
    public interface IEventStore
    {
        Task<TAggregate> GetByIdAsync<TAggregate>(Guid id);
        Task<TAggregate> GetByIdAsync<TAggregate>(Guid id, DateTime until);
        Task<object> GetByIdAsync(Guid id);
        void Save(AggregateEvent aggregateEvent);
        void Configure(IRepository<AggregateEvent> repo, IEventSerializationStrategy serializationStrategy);
        Task<IEnumerable<AggregateEvent>> GetAllAsync();
        Task<IEnumerable<AggregateEvent>> GetAllAsync(DateTime until);
        Task<long> CountAsync();
        Task<DateTime> LatestEventTimestampAsync(Guid aggregateId);
        Task<IEnumerable<TEvent>> GetEventsAsync<TEvent>();
    }
}