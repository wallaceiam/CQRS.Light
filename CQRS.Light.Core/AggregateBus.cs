using System;
using System.Collections.Generic;
using CQRS.Light.Contracts;
using System.Threading.Tasks;
using System.Linq;

namespace CQRS.Light.Core
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

        public void Configure(IEventBus eventBus)
        {
            this.Configure(eventBus, null);
        }

        public void Configure(IEventBus eventBus, IAggregateCache aggregateCache)
        {
            _eventBus = eventBus;

            if (aggregateCache != null)
            {
                _registeredAggregateCaches.Add(aggregateCache);

                eventBus.Subscribe((AggregateCacheCleared e) => AggregateCacheClearAsync(e));
            }
        }

        private async Task AggregateCacheClearAsync(AggregateCacheCleared e)
        {
            foreach(var aggregateCache in _registeredAggregateCaches)
            {
                var mi = aggregateCache.GetType().GetMethod("ClearAsync", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
                mi = mi.MakeGenericMethod(e.AggregateType);
                Task result = (Task)mi.Invoke(aggregateCache, null);
                await result;
            }
            //return _registeredAggregateCaches.ForEach(x => await x.ClearAsync(Guid.Parse(e.SerializedAggregateId), e.AggregateType);
        }

        public void Reset()
        {
            _eventBus = null;
            _registeredAggregateCaches.Clear();
        }

        private AggregateBus()
        {
            _registeredAggregateCaches = new List<IAggregateCache>();
        }
               
        public async Task PublishAsync<TAggregate, TEvent>(Guid aggregateId, TEvent @event) where TAggregate : IAggregateRoot
        {
            if (_eventBus == null) throw new ApplicationException("AggregateBus -> Publish failed. EventBus is not configured");
            await _eventBus.PublishAsync<TAggregate, TEvent>(aggregateId, @event);
            if(_registeredAggregateCaches.Any())
                await Task.WhenAll(_registeredAggregateCaches.Select(aggregateCache => aggregateCache.HandleAsync<TAggregate, TEvent>(aggregateId, @event)));
        }

    }
}
