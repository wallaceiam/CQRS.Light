using System;
using System.Linq;
using System.Reflection;
using DDD.Light.Contracts.CQRS;
using DDD.Light.Contracts.EventStore;
using System.Threading.Tasks;

namespace DDD.Light.CQRS
{
    public class EventBus : IEventBus
    {
        private static volatile IEventBus _instance;
        private static object token = new Object();
        private IEventStore _eventStore;
        private IEventSerializationStrategy _eventSerializationStrategy;
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

        private EventBus(){}

        public void Subscribe<T>(IEventHandler<T> handler)
        {
            EventHandlersDatabase<T>.Instance.Add(handler);
        }

        public void Subscribe<T>(Action<T> handleMethod)
        {
            EventHandlersDatabase<T>.Instance.Add(handleMethod);
        }

        public async Task PublishAsync<TEvent>(Type aggregateType, Guid aggregateId, TEvent @event)
        {
            await StoreEvent(aggregateType, aggregateId, @event);
            HandleEvent(@event);
        }

        public async Task PublishAsync<TAggregate, T>(Guid aggregateId, T @event)
        {
            await StoreEvent(typeof(TAggregate), aggregateId, @event);
            HandleEvent(@event);
        }

        //todo: make reason behind checkLatestEventTimestampPriorToSavingToEventStore less ambiguious
        public void Configure(IEventStore eventStore, IEventSerializationStrategy eventSerializationStrategy, bool checkLatestEventTimestampPriorToSavingToEventStore)
        {
            _eventStore = eventStore;
            _eventSerializationStrategy = eventSerializationStrategy;
            _checkLatestEventTimestampPriorToSavingToEventStore = checkLatestEventTimestampPriorToSavingToEventStore;
        }

        private void HandleEvent<T>(T @event)
        {
            try
            {
                if (!Equals(@event, default(T)))
                    new Transaction<T>(@event, EventHandlersDatabase<T>.Instance.Get().ToList()).Commit();
            }
            catch (Exception)
            {
                throw new ApplicationException("Transaction<T>(@event, EventHandlersDatabase<T>.Instance.Get().ToList()).Commit() failed");
            }
        }

        private async Task StoreEvent<TEvent>(Type aggregateType, Guid aggregateId, TEvent @event)
        {
            if (_eventStore == null) throw new ApplicationException("Event Store is not configured. Use 'EventBus.Instance.Configure(eventStore, eventSerializationStrategy);' to configure it.");
            try
            {
                if (_checkLatestEventTimestampPriorToSavingToEventStore)
                {
                    var latestCreatedOnInEventStore = await _eventStore.LatestEventTimestampAsync(aggregateId);
                    if (DateTime.Compare(DateTime.UtcNow, latestCreatedOnInEventStore) < 0)
                        //earlier than in event store
                    {
                        var serializedAggregateId = _eventSerializationStrategy.SerializeEvent(aggregateId);
                        await PublishAsync(GetType(), aggregateId, new AggregateCacheCleared(serializedAggregateId, typeof(Guid), aggregateType));
                    }
                }
                _eventStore.Save(new AggregateEvent
                    {
                        Id = Guid.NewGuid(),
                        AggregateType = aggregateType.AssemblyQualifiedName,
                        EventType = typeof (TEvent).AssemblyQualifiedName,
                        CreatedOn = DateTime.UtcNow,
                        SerializedEvent = _eventSerializationStrategy.SerializeEvent(@event),
                        SerializedAggregateId = _eventSerializationStrategy.SerializeEvent(aggregateId),
                        AggregateIdType = typeof(Guid).AssemblyQualifiedName
                    });
            }
            catch (Exception ex)
            {
                throw new ApplicationException("DDD.Light.CQRS.InProcess.EventBus -> StoreEvent<T>: Saving to event store failed", ex);
            }
        }

        public IEventStore GetEventStore()
        {
            return _eventStore;
        }

        public async Task RestoreReadModelAync()
        {
            (await _eventStore.GetAllAsync()).ToList().ForEach(HandleRestoreReadModelEvent);
        }

        public async Task RestoreReadModelAync(DateTime until)
        {
            (await _eventStore.GetAllAsync(until)).ToList().ForEach(HandleRestoreReadModelEvent);
        }

        private void HandleRestoreReadModelEvent(AggregateEvent aggregateEvent)
        {
            var eventType = Type.GetType(aggregateEvent.EventType);
            var @event = _eventSerializationStrategy.DeserializeEvent(aggregateEvent.SerializedEvent, eventType);
            GetType().GetMethod("HandleEvent", BindingFlags.NonPublic | BindingFlags.Instance)
                     .MakeGenericMethod(eventType)
                     .Invoke(Instance, new[] {@event});
        }

    }
}
