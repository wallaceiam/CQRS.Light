using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CQRS.Light.Contracts;
using System.Threading.Tasks;

namespace CQRS.Light.Core
{
    public class EventStore : IEventStore
    {
        private static volatile EventStore _instance;
        private IRepository<AggregateEvent> _repo;
        private static object token = new Object();
        private ISerializationStrategy _serializationStrategy;

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

        public void Configure(IRepository<AggregateEvent> repo, ISerializationStrategy SerializationStrategy)
        {
            if (repo == null) throw new ArgumentNullException("repo");
            if (SerializationStrategy == null) throw new ArgumentNullException("SerializationStrategy");
            
            _repo = repo;
            _serializationStrategy = SerializationStrategy;
        }

        public void Reset()
        {
            _repo = null;
            _serializationStrategy = null;
        }

        public Task<IEnumerable<AggregateEvent>> GetAllAsync()
        {
            VerifyIsConfigured();
            return _repo.GetAllAsync();
        }

        public async Task<IEnumerable<AggregateEvent>> GetAllAsync(DateTime until)
        {
            VerifyIsConfigured();
            return (await _repo.GetAsync()).Where(x => DateTime.Compare(x.CreatedOn, until) <= 0);
        }

        public async Task<long> CountAsync()
        {
            VerifyIsConfigured();
            return await _repo.CountAsync();
        }

        public async Task<DateTime> LatestEventTimestampAsync(Guid aggregateId)
        {
            VerifyIsConfigured();
            //this seems overkill to deserialize to get the created on
            //var aggregateEvents = (await _repo.GetAsync()).Where(x => _SerializationStrategy.DeserializeEvent(x.SerializedAggregateId, Type.GetType(x.AggregateIdType)).Equals(aggregateId)).OrderByDescending(x => x.CreatedOn);
            //return aggregateEvents.Any() ? aggregateEvents.First().CreatedOn : new DateTime();
            var serializedAggregateId = _serializationStrategy.Serialize(aggregateId);
            var latestAggregateEvent = (await _repo.GetAsync()).Where(x => x.SerializedAggregateId == serializedAggregateId).OrderByDescending(x => x.CreatedOn).FirstOrDefault();
            return latestAggregateEvent != null ? latestAggregateEvent.CreatedOn : DateTime.MinValue;
        }

        private void VerifyIsConfigured()
        {
            if (_repo == null || _serializationStrategy == null)
                throw new InvalidOperationException("EventStore.Instance.Configure must be called before it can be used.");
        }

        public async Task<TAggregate> GetByIdAsync<TAggregate>(Guid id) 
        {
            VerifyIsConfigured();

            var constructors = (typeof(TAggregate)).GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance);
            var aggregate = (TAggregate)constructors[0].Invoke(new object[] { });

            (await _repo.GetAsync()).Where(x => _serializationStrategy.Deserialize(x.SerializedAggregateId, Type.GetType(x.AggregateIdType)).Equals(id)).OrderBy(x => x.CreatedOn).ToList().ForEach(aggregateEvent =>
                {
                    var eventType = Type.GetType(aggregateEvent.EventType);
                    var @event = _serializationStrategy.Deserialize(aggregateEvent.SerializedEvent, eventType);
                    var method = typeof(TAggregate).GetMethod("ApplyEvent", BindingFlags.NonPublic | BindingFlags.Instance, null, new[]{eventType}, null);
                    method.Invoke(aggregate, new[] { @event });
                });
            return aggregate;
        }
        
        public async Task<TAggregate> GetByIdAsync<TAggregate>(Guid id, DateTime until) 
        {
            VerifyIsConfigured();

            var constructors = (typeof(TAggregate)).GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance);
            var aggregate = (TAggregate)constructors[0].Invoke(new object[] { });

            (await _repo.GetAsync()).Where(x => _serializationStrategy.Deserialize(x.SerializedAggregateId, Type.GetType(x.AggregateIdType)).Equals(id) && DateTime.Compare(x.CreatedOn, until) <= 0).OrderBy(x => x.CreatedOn).ToList().ForEach(aggregateEvent =>
                {
                    var eventType = Type.GetType(aggregateEvent.EventType);
                    var @event = _serializationStrategy.Deserialize(aggregateEvent.SerializedEvent, eventType);
                    var method = typeof(TAggregate).GetMethod("ApplyEvent", BindingFlags.NonPublic | BindingFlags.Instance, null, new[]{eventType}, null);
                    method.Invoke(aggregate, new[] { @event });
                });
            return aggregate;
        }

        public async Task<object> GetByIdAsync(Guid id)
        {
            VerifyIsConfigured();

            if (!(await _repo.GetAsync()).Any(x => _serializationStrategy.Deserialize(x.SerializedAggregateId, Type.GetType(x.AggregateIdType)).Equals(id))) return null;

            var serializedAggregateType = (await _repo.GetAsync()).First(x => _serializationStrategy.Deserialize(x.SerializedAggregateId, Type.GetType(x.AggregateIdType)).Equals(id)).AggregateType;

            var aggregateType = Type.GetType(serializedAggregateType);

            if (aggregateType == null) return null;

            var constructors = aggregateType.GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance);
            var aggregate = constructors[0].Invoke(new object[] { });

            (await _repo.GetAsync()).Where(x => _serializationStrategy.Deserialize(x.SerializedAggregateId, Type.GetType(x.AggregateIdType)).Equals(id)).OrderBy(x => x.CreatedOn).ToList().ForEach(aggregateEvent =>
            {
                var eventType = Type.GetType(aggregateEvent.EventType);
                var @event = _serializationStrategy.Deserialize(aggregateEvent.SerializedEvent, eventType);
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
            VerifyIsConfigured();
            await _repo.SaveAsync(aggregateEvent);
        }

        public async Task<IEnumerable<TEvent>> GetEventsAsync<TEvent>()
        {
            VerifyIsConfigured();

            var deserializedEvents = new List<TEvent>();
            var serializedEvents = (await GetAllAsync()).Where(e => Type.GetType(e.EventType) == typeof(TEvent)).ToList();
            serializedEvents.ForEach(s =>
                {
                    var deserializedEvent = (TEvent)_serializationStrategy.Deserialize(s.SerializedEvent, typeof (TEvent));
                    deserializedEvents.Add(deserializedEvent);
                });

            return deserializedEvents;

        }
    }
}