using System;
using System.Collections.Generic;
using DDD.Light.Contracts.AggregateBus;
using DDD.Light.Contracts.AggregateCache;
using DDD.Light.Contracts.CQRS;

namespace DDD.Light.Core
{
    public class AggregateBus : IAggregateBus
    {
        private static volatile IAggregateBus _instance;
        private static object token = new Object();
        private readonly List<IAggregateCache> _registeredAggregateCaches;
        private IEventBus _eventBus;

        public static IAggregateBus Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (token)
                    {
                        if (_instance == null)
                            _instance = new AggregateBus();
                    }
                }
                return _instance;
            }
        }

        public List<IAggregateCache> RegisteredAggregateCaches
        {
            get { return _registeredAggregateCaches; }
        }

        public void Configure(IEventBus eventBus, IAggregateCache aggregateCache)
        {
            _eventBus = eventBus;
            _registeredAggregateCaches.Add(aggregateCache);

            eventBus.Subscribe((AggregateCacheCleared e) => aggregateCache.Clear(Guid.Parse(e.SerializedAggregateId), e.AggregateType));
        }

        private AggregateBus()
        {
            _registeredAggregateCaches = new List<IAggregateCache>();
        }
               
        public void Publish<TAggregate, TEvent>(Guid aggregateId, TEvent @event) where TAggregate : IAggregateRoot
        {
            if (_eventBus == null) throw new ApplicationException("AggregateBus -> Publish failed. EventBus is not configured");
            _eventBus.PublishAsync<TAggregate, TEvent>(aggregateId, @event);
            _registeredAggregateCaches.ForEach(aggregateCache => aggregateCache.HandleAsync<TAggregate, TEvent>(aggregateId, @event));
        }

    }
}
