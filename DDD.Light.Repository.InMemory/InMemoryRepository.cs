using System;
using System.Collections.Generic;
using System.Linq;
using CQRS.Light.Contracts;
using System.Threading.Tasks;

namespace CQRS.Light.Repository.InMemory
{
    public class InMemoryRepository<TAggregate> : IRepository<TAggregate> where TAggregate : IEntity
    {
        private static List<TAggregate> _db; 

        public InMemoryRepository()
        {
            _db = new List<TAggregate>(); 
        }

        public Task<TAggregate> GetByIdAsync(Guid id)
        {
            return Task.FromResult<TAggregate>(_db.FirstOrDefault(i => i.Id.Equals(id)));
        }

        public Task<IEnumerable<TAggregate>> GetAllAsync()
        {
            return Task.FromResult<IEnumerable<TAggregate>>(_db);
        }

        public Task<IQueryable<TAggregate>> GetAsync()
        {
            return Task.FromResult<IQueryable<TAggregate>>(_db.AsQueryable() );
        }

        public Task SaveAsync(TAggregate item)
        {
            _db.Add(item);
            return Task.FromResult<object>(null);
        }

        public Task SaveAllAsync(IEnumerable<TAggregate> items)
        {
            Parallel.ForEach(items, async x => await SaveAsync(x));
            return Task.FromResult<object>(null);
        }

        public Task DeleteAsync(Guid id)
        {
            var item = _db.FirstOrDefault(i => i.Id.Equals(id));
            if (!Equals(item, default(TAggregate))) 
                DeleteAsync(item);
            return Task.FromResult<object>(null);
        }

        public Task DeleteAsync(TAggregate item)
        {
            _db.Remove(item);
            return Task.FromResult<object>(null);
        }

        public Task DeleteAllAsync()
        {
            _db.Clear();
            return Task.FromResult<object>(null);
        }

        public Task<long> CountAsync()
        {
            return Task.FromResult((long)_db.Count);
        }
    }
}
