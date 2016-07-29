using System;
using System.Reflection;
using CQRS.Light.Contracts;
using System.Threading.Tasks;

namespace CQRS.Light.Core
{
    public class AggregateCache : IAggregateCache
    {
        private static volatile AggregateCache _instance;
        private static object token = new Object();
        private IEventStore _eventStore;

        private AggregateCache(){}

        public static IAggregateCache Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (token)
                    {
                        if (_instance == null)
                            _instance = new AggregateCache();
                    }
                }
                return _instance;
            }
        }

        private Func<Type, object> _getAggregateCacheRepositoryInstance;
        public void Configure(IEventStore eventStore, Func<Type, object> getAggregateCacheRepositoryInstance)
        {
            _eventStore = eventStore;
            _getAggregateCacheRepositoryInstance = getAggregateCacheRepositoryInstance;
        }

        public void Reset()
        {
            _eventStore = null;
            _getAggregateCacheRepositoryInstance = null; 
        }

        private IRepository<TAggregate> GetRepository<TAggregate>()
        {
            return _getAggregateCacheRepositoryInstance(typeof(IRepository<TAggregate>)) as IRepository<TAggregate>;
        }

        private IRepository<TAggregate> GetRepository<TAggregate>(TAggregate type)
        {
            return _getAggregateCacheRepositoryInstance(typeof(IRepository<TAggregate>)) as IRepository<TAggregate>;
        }

        public async Task<TAggregate> GetByIdAsync<TAggregate>(Guid id) where TAggregate : IAggregateRoot
        {
            var cachedAggregate = await GetRepository<TAggregate>().GetByIdAsync(id);
            if (Equals(cachedAggregate, default(TAggregate)))
            {
                var aggregate = await _eventStore.GetByIdAsync<TAggregate>(id);
                await GetRepository<TAggregate>().SaveAsync(aggregate);
                return aggregate;
            }
            return cachedAggregate;
        }

        public async Task HandleAsync<TAggregate, TEvent>(Guid aggregateId, TEvent @event) where TAggregate : IAggregateRoot
        {
            var aggregate = await GetRepository<TAggregate>().GetByIdAsync(aggregateId);
            if (Equals(aggregate, default(TAggregate)))
                aggregate = await _eventStore.GetByIdAsync<TAggregate>(aggregateId);
            if (!Equals(aggregate, default(TAggregate)))
                ApplyEvent<TAggregate, TEvent>(@event, aggregate);
        }

        public async Task ClearAsync(Guid aggregateId, Type aggregateType)
        {
            var aggregateRepo = GetRepository(aggregateType);
            await aggregateRepo.DeleteAsync(aggregateId);
        }

        private static void ApplyEvent<TAggregate, TEvent>(TEvent @event, TAggregate aggregate) where TAggregate : IAggregateRoot
        {
            var eventType = typeof (TEvent);
            var method = typeof (TAggregate).GetMethod("ApplyEvent", BindingFlags.NonPublic | BindingFlags.Instance, null, new[] {eventType}, null);
            method.Invoke(aggregate, new[] {@event as Object});
        }
    }
}
