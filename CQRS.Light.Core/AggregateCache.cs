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
            if (eventStore == null) throw new ArgumentNullException("eventStore");
            if (getAggregateCacheRepositoryInstance == null) throw new ArgumentNullException("getAggregateCacheRepositoryInstance");

            _eventStore = eventStore;
            _getAggregateCacheRepositoryInstance = getAggregateCacheRepositoryInstance;
        }

        public void Reset()
        {
            _eventStore = null;
            _getAggregateCacheRepositoryInstance = null; 
        }

        public IEventStore EventStore { get { return _eventStore; } }

        private IRepository<TAggregate> GetRepository<TAggregate>() where TAggregate : IAggregateRoot
        {
            var repo =  _getAggregateCacheRepositoryInstance(typeof(IRepository<TAggregate>)) as IRepository<TAggregate>;
            if (repo == null)
                throw new InvalidOperationException(string.Format("No repository found for {0}", typeof(TAggregate)));
            return repo;
        }

        //private IRepository<TAggregate> GetRepository<TAggregate>(TAggregate type)
        //{
        //    var result = _getAggregateCacheRepositoryInstance(typeof(IRepository<TAggregate>));
        //    var repo = result as IRepository<TAggregate>;
        //    if (repo == null)
        //        throw new InvalidOperationException(string.Format("No repository found for {0}", typeof(TAggregate)));
        //    return repo;

        //}

        private void VerifyIsConfigure()
        {
            if (_eventStore == null) throw new InvalidOperationException("AggregateCache.Instance.Configured must be called before it can be used.");
            if (_getAggregateCacheRepositoryInstance == null) throw new InvalidOperationException("AggregateCache.Instance.Configured must be called before it can be used.");
        }

        public async Task<TAggregate> GetByIdAsync<TAggregate>(Guid id) where TAggregate : IAggregateRoot
        {
            VerifyIsConfigure();
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
            VerifyIsConfigure();
            var aggregate = await GetRepository<TAggregate>().GetByIdAsync(aggregateId);
            if (Equals(aggregate, default(TAggregate)))
                aggregate = await _eventStore.GetByIdAsync<TAggregate>(aggregateId);
            if (!Equals(aggregate, default(TAggregate)))
                ApplyEvent<TAggregate, TEvent>(@event, aggregate);
        }

        public async Task ClearAsync<TAggregate>(Guid aggregateId) where TAggregate : IAggregateRoot
        {
            VerifyIsConfigure();

            var aggregateRepo = GetRepository<TAggregate>();
            await aggregateRepo.DeleteAsync(aggregateId);
        }

        private static void ApplyEvent<TAggregate, TEvent>(TEvent @event, TAggregate aggregate) where TAggregate : IAggregateRoot
        {
            var eventType = typeof (TEvent);
            var method = typeof (TAggregate).GetMethod("ApplyEvent", BindingFlags.NonPublic | BindingFlags.Instance, null, new[] {eventType}, null);
            if (method == null)
                throw new InvalidOperationException(string.Format("{0} does not contain a non-public method ApplyEvent accepting parameter type {1}",
                    typeof(TAggregate).ToString(),
                    eventType));
            method.Invoke(aggregate, new[] {@event as Object});
        }
    }
}
