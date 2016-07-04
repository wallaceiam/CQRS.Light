using System;
using System.Collections.Generic;
using System.Linq;
using DDD.Light.Repo.Contracts;
using MongoDB.Driver;
using System.Threading.Tasks;
using MongoDB.Bson;

namespace DDD.Light.Repo.MongoDB
{
    public class MongoRepository<TId, TAggregate> : IRepository<TId, TAggregate>
        where TAggregate : IEntity<TId> 
    {
        private readonly  IMongoCollection<TAggregate> _collection;

        public MongoRepository(string connectionString, string databaseName, string collectionName)
        {
            var client = MongoPool.Instance.GetClient(connectionString);
            //var server = client..GetServer();
            //var database = server.GetDatabase(databaseName);
            var database = client.GetDatabase(databaseName);
            _collection = database.GetCollection<TAggregate>(collectionName);
        }
 
        public TAggregate GetById(TId id)
        {
            //todo serialize or write wrapper
//            return _collection.FindOneById(id);
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<TAggregate>> GetAll()
        {
            return await _collection.Find(new BsonDocument()).ToListAsync();
        }

        public IQueryable<TAggregate> Get()
        {
            return _collection.AsQueryable();
        }

        public void Save(TAggregate item)
        {
            _collection.Save(item);
        }

        public void SaveAll(IEnumerable<TAggregate> items)
        {
            items.ToList().ForEach(Save);
        }

        public void Delete(TId id)
        {
            //todo serialize or wrapper
//            _collection.Remove(Query.EQ("_id", id));
            throw new NotImplementedException();
        }

        public void Delete(TAggregate item)
        {
            Delete(item.Id);
        }

        public void DeleteAll()
        {
            _collection.RemoveAll();
        }

        public long Count()
        {
            return _collection.Count();
        }
    }
}
