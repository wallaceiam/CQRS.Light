using System;
using System.Collections.Generic;
using System.Linq;
using DDD.Light.Repo.Contracts;
using System.Threading.Tasks;

namespace DDD.Light.Repo.InMemory
{
    public class InMemoryRepository<TId, TAggregate> : IRepository<TId, TAggregate> where TAggregate : IEntity<TId>
    {
        private static List<TAggregate> _db; 

        public InMemoryRepository()
        {
            _db = new List<TAggregate>(); 
        }

        public TAggregate GetById(TId id)
        {
            return _db.FirstOrDefault(i => i.Id.Equals(id));
        }

        public async Task<IEnumerable<TAggregate>> GetAll()
        {
            return await Task.FromResult<IEnumerable<TAggregate>>(_db);
        }

        public IQueryable<TAggregate> Get()
        {
            return _db.AsQueryable() ;
        }

        public void Save(TAggregate item)
        {
            _db.Add(item);
        }

        public void SaveAll(IEnumerable<TAggregate> items)
        {
            items.ToList().ForEach(Save);
        }

        public void Delete(TId id)
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
