using System;
using System.Collections.Generic;
using System.Linq;
using DDD.Light.Repo.Contracts;
using System.Threading.Tasks;

namespace DDD.Light.Repo.InMemory
{
    public class InMemoryRepository<TAggregate> : IRepository<TAggregate> where TAggregate : IEntity
    {
        private static List<TAggregate> _db; 

        public InMemoryRepository()
        {
            _db = new List<TAggregate>(); 
        }

        public Task<TAggregate> GetById(Guid id)
        {
            return Task.FromResult<TAggregate>(_db.FirstOrDefault(i => i.Id.Equals(id)));
        }

        public Task<IEnumerable<TAggregate>> GetAll()
        {
            return Task.FromResult<IEnumerable<TAggregate>>(_db);
        }

        public Task<IQueryable<TAggregate>> Get()
        {
            return Task.FromResult<IQueryable<TAggregate>>(_db.AsQueryable() );
        }

        public Task Save(TAggregate item)
        {
            return Task.Run(() => _db.Add(item));
        }

        public void SaveAll(IEnumerable<TAggregate> items)
        {
            Parallel.ForEach(items, async x => await Save(x));
            //items.ToList().ForEach(Save);
        }

        public void Delete(Guid id)
        {
            var item = _db.FirstOrDefault(i => i.Id.Equals(id));
            if (!Equals(item, default(TAggregate))) 
                Delete(item);
        }

        public void Delete(TAggregate item)
        {
            _db.Remove(item);
        }

        public void DeleteAll()
        {
            _db.Clear();
        }

        public long Count()
        {
            return _db.Count;
        }
    }
}
