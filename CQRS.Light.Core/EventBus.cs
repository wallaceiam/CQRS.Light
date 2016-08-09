using System;
using System.Linq;
using System.Reflection;
using CQRS.Light.Contracts;
using System.Threading.Tasks;

namespace CQRS.Light.Core
{
    public class EventBus : IEventBus
    {
        private static volatile IEventBus _instance;
        private static object token = new Object();
        private IEventStore _eventStore;
        private ISerializationStrategy _serializationStrategy;
        private bool _checkLatestEventTimestampPriorToSavingToEventStore;

        public static IEventBus Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (token)
                    {
                        if (_instance == null)
                            _instance = new EventBus();
                    }
                }
                return _instance;
            }
        }

        private EventBus() { }

        public void Subscribe<T>(IEventHandler<T> handler)
        {
            EventHandlersDatabase<T>.Instance.Add(handler);
        }

        public void Subscribe<T>(Func<T, Task> handleMethod)
        {
            EventHandlersDatabase<T>.Instance.Add(handleMethod);
        }

        public async Task PublishAsync<TEvent>(Type aggregateType, Guid aggregateId, TEvent @event)
        {
            VerifyIsConfigured();
            await StoreEventAsync(aggregateType, aggregateId, @event);
            await HandleEventAsync(@event);
        }

        public async Task PublishAsync<TAggregate, T>(Guid aggregateId, T @event)
        {
            VerifyIsConfigured();
            await StoreEventAsync(typeof(TAggregate), aggregateId, @event);
            await HandleEventAsync(@event);
        }

        public void Configure(IEventStore eventStore, ISerializationStrategy SerializationStrategy)
        {
            Configure(eventStore, SerializationStrategy, true);
        }

        //todo: make reason behind checkLatestEventTimestampPriorToSavingToEventStore less ambiguious
        public void Configure(IEventStore eventStore, ISerializationStrategy SerializationStrategy, bool checkLatestEventTimestampPriorToSavingToEventStore)
        {
            if (eventStore == null) throw new ArgumentNullException("eventStore");
            if (SerializationStrategy == null) throw new ArgumentNullException("SerializationStrategy");

            _eventStore = eventStore;
            _serializationStrategy = SerializationStrategy;
            _checkLatestEventTimestampPriorToSavingToEventStore = checkLatestEventTimestampPriorToSavingToEventStore;
        }

        public void Reset()
        {
            _eventStore = null;
            _serializationStrategy = null;
            _checkLatestEventTimestampPriorToSavingToEventStore = false;
        }

        private void VerifyIsConfigured()
        {
            if (_eventStore == null || _serializationStrategy == null)
                throw new InvalidOperationException("EventBus.Instance.Configure must be called before it can be used.");
        }

        private async Task HandleEventAsync<T>(T @event)
        {
            if (!Equals(@event, default(T)))
            {
                var transaction = new Transaction<T>(@event, EventHandlersDatabase<T>.Instance.Get().ToList());
                await transaction.CommitAsync();
            }
        }

        private async Task StoreEventAsync<TEvent>(Type aggregateType, Guid aggregateId, TEvent @event)
        {
            try
            {
                if (!(@event is AggregateCacheCleared)) //don't want to publish or persist these events
                {
                    if (_checkLatestEventTimestampPriorToSavingToEventStore)
                    {
                        var latestCreatedOnInEventStore = await _eventStore.LatestEventTimestampAsync(aggregateId);
                        if (DateTime.Compare(DateTime.UtcNow, latestCreatedOnInEventStore) < 0)
                        //earlier than in event store
                        {
                            var serializedAggregateId = _serializationStrategy.Serialize(aggregateId);
                            await PublishAsync(GetType(), aggregateId, new AggregateCacheCleared(serializedAggregateId, typeof(Guid), aggregateType));
                        }
                    }
                    await _eventStore.SaveAsync(new AggregateEvent
                    {
                        Id = Guid.NewGuid(),
                        AggregateType = aggregateType.AssemblyQualifiedName,
                        EventType = typeof(TEvent).AssemblyQualifiedName,
                        CreatedOn = DateTime.UtcNow,
                        SerializedEvent = _serializationStrategy.Serialize(@event),
                        SerializedAggregateId = _serializationStrategy.Serialize(aggregateId),
                        AggregateIdType = typeof(Guid).AssemblyQualifiedName
                    });
                }
            }
            catch (Exception ex)
            {
                throw new ApplicationException("CQRS.Light.Core.EventBus -> StoreEvent<T>: Saving to event store failed", ex);
            }
        }

        public IEventStore GetEventStore()
        {
            return _eventStore;
        }

        public async Task RestoreReadModelAync()
        {
            VerifyIsConfigured();
            var events = await _eventStore.GetAllAsync();
            foreach (var @event in events)
            {
                await HandleRestoreReadModelEventAsync(@event);
            }
            //(await _eventStore.GetAllAsync()).ToList().ForEach(async x => await HandleRestoreReadModelEventAsync(x));
        }

        public async Task RestoreReadModelAync(DateTime until)
        {
            VerifyIsConfigured();
            var events = await _eventStore.GetAllAsync(until);
            foreach(var @event in events)
            {
                await HandleRestoreReadModelEventAsync(@event);
            }
            //(await _eventStore.GetAllAsync(until)).ToList().ForEach(async x => await HandleRestoreReadModelEventAsync(x));
        }

        private async Task HandleRestoreReadModelEventAsync(AggregateEvent aggregateEvent)
        {
            var eventType = Type.GetType(aggregateEvent.EventType);
            var @event = _serializationStrategy.Deserialize(aggregateEvent.SerializedEvent, eventType);
            var method = GetType().GetMethod("HandleEventAsync", BindingFlags.NonPublic | BindingFlags.Instance)
                     .MakeGenericMethod(eventType);
            var result = (Task)method.Invoke(Instance, new[] { @event });
            await result;
        }

    }
}
