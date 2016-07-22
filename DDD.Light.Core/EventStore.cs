using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DDD.Light.Contracts.EventStore;
using DDD.Light.Contracts.Repo;
using System.Threading.Tasks;

namespace DDD.Light.Core
{
    public class EventStore : IEventStore
    {
        private static volatile EventStore _instance;
        private IRepository<AggregateEvent> _repo;
        private static object token = new Object();
        private IEventSerializationStrategy _serializationStrategy;

        public static IEventStore Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (token)
                    {
                        if (_instance == null)
                            _instance = new EventStore();
                    }
                }
                return _instance;
            }
        }

        public void Configure(IRepository<AggregateEvent> repo, IEventSerializationStrategy serializationStrategy)
        {
            _repo = repo;
            _serializationStrategy = serializationStrategy;
        }


        public Task<IEnumerable<AggregateEvent>> GetAllAsync()
        {
            return _repo.GetAllAsync();
        }

        public async Task<IEnumerable<AggregateEvent>> GetAllAsync(DateTime until)
        {
            return (await _repo.GetAsync()).Where(x => DateTime.Compare(x.CreatedOn, until) <= 0);
        }

        public async Task<long> CountAsync()
        {
            return await _repo.CountAsync();
        }

        public async Task<DateTime> LatestEventTimestampAsync(Guid aggregateId)
        {
            VerifyRepoIsConfigured();
            var aggregateEvents = (await _repo.GetAsync()).Where(x => _serializationStrategy.DeserializeEvent(x.SerializedAggregateId, Type.GetType(x.AggregateIdType)).Equals(aggregateId)).OrderByDescending(x => x.CreatedOn);
            return aggregateEvents.Any() ? aggregateEvents.First().CreatedOn : new DateTime();
        }

        private void VerifyRepoIsConfigured()
        {
            if (_repo == null) throw new Exception("Event Store Repository is not configured. Use EventStore.Instance.Configure(); to configure");
        }

        public async Task<TAggregate> GetByIdAsync<TAggregate>(Guid id) 
        {
            VerifyRepoIsConfigured();

            var constructors = (typeof(TAggregate)).GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance);
            var aggregate = (TAggregate)constructors[0].Invoke(new object[] { });

            (await _repo.GetAsync()).Where(x => _serializationStrategy.DeserializeEvent(x.SerializedAggregateId, Type.GetType(x.AggregateIdType)).Equals(id)).OrderBy(x => x.CreatedOn).ToList().ForEach(aggregateEvent =>
                {
                    var eventType = Type.GetType(aggregateEvent.EventType);
                    var @event = _serializationStrategy.DeserializeEvent(aggregateEvent.SerializedEvent, eventType);
                    var method = typeof(TAggregate).GetMethod("ApplyEvent", BindingFlags.NonPublic | BindingFlags.Instance, null, new[]{eventType}, null);
                    method.Invoke(aggregate, new[] { @event });
                });
            return aggregate;
        }
        
        public async Task<TAggregate> GetByIdAsync<TAggregate>(Guid id, DateTime until) 
        {
            VerifyRepoIsConfigured();

            var constructors = (typeof(TAggregate)).GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance);
            var aggregate = (TAggregate)constructors[0].Invoke(new object[] { });

            (await _repo.GetAsync()).Where(x => _serializationStrategy.DeserializeEvent(x.SerializedAggregateId, Type.GetType(x.AggregateIdType)).Equals(id) && DateTime.Compare(x.CreatedOn, until) <= 0).OrderBy(x => x.CreatedOn).ToList().ForEach(aggregateEvent =>
                {
                    var eventType = Type.GetType(aggregateEvent.EventType);
                    var @event = _serializationStrategy.DeserializeEvent(aggregateEvent.SerializedEvent, eventType);
                    var method = typeof(TAggregate).GetMethod("ApplyEvent", BindingFlags.NonPublic | BindingFlags.Instance, null, new[]{eventType}, null);
                    method.Invoke(aggregate, new[] { @event });
                });
            return aggregate;
        }

        public async Task<object> GetByIdAsync(Guid id)
        {
            VerifyRepoIsConfigured();

            if (!(await _repo.GetAsync()).Any(x => _serializationStrategy.DeserializeEvent(x.SerializedAggregateId, Type.GetType(x.AggregateIdType)).Equals(id))) return null;

            var serializedAggregateType = (await _repo.GetAsync()).First(x => _serializationStrategy.DeserializeEvent(x.SerializedAggregateId, Type.GetType(x.AggregateIdType)).Equals(id)).AggregateType;

            var aggregateType = Type.GetType(serializedAggregateType);

            if (aggregateType == null) return null;

            var constructors = aggregateType.GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance);
            var aggregate = constructors[0].Invoke(new object[] { });

            (await _repo.GetAsync()).Where(x => _serializationStrategy.DeserializeEvent(x.SerializedAggregateId, Type.GetType(x.AggregateIdType)).Equals(id)).OrderBy(x => x.CreatedOn).ToList().ForEach(aggregateEvent =>
            {
                var eventType = Type.GetType(aggregateEvent.EventType);
                var @event = _serializationStrategy.DeserializeEvent(aggregateEvent.SerializedEvent, eventType);
                var method = aggregateType.GetMethod("ApplyEvent", BindingFlags.NonPublic | BindingFlags.Instance, null, new[] { eventType }, null);
                try
                {
                    method.Invoke(aggregate, new[] {@event});
                }
                catch (Exception ex)
                {
                    throw new Exception("Please check if ApplyEvent for eventTyoe: " + aggregateEvent.EventType + " defined on aggregate: " + serializedAggregateType, ex);
                }
            });
            return aggregate;
        }

        public async Task SaveAsync(AggregateEvent aggregateEvent)
        {
            VerifyRepoIsConfigured();
            await _repo.SaveAsync(aggregateEvent);
        }

        public async Task<IEnumerable<TEvent>> GetEventsAsync<TEvent>()
        {
            if (_serializationStrategy == null) throw new ApplicationException("Serialization Strategy is not configured");

            var deserializedEvents = new List<TEvent>();
            var serializedEvents = (await GetAllAsync()).Where(e => Type.GetType(e.EventType) == typeof(TEvent)).ToList();
            serializedEvents.ForEach(s =>
                {
                    var deserializedEvent = (TEvent)_serializationStrategy.DeserializeEvent(s.SerializedEvent, typeof (TEvent));
                    deserializedEvents.Add(deserializedEvent);
                });

            return deserializedEvents;

        }
    }
}