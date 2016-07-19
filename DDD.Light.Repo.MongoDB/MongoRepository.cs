using System;
using System.Collections.Generic;
using System.Linq;
using DDD.Light.Contracts.Repo;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Threading.Tasks;

namespace DDD.Light.Repo.MongoDB
{
    public class MongoRepository<TAggregate> : IRepository<TAggregate>
        where TAggregate : IEntity 
    {
        private readonly  IMongoCollection<TAggregate> _collection;

        public MongoRepository(string connectionString, string databaseName, string collectionName)
        {
            var client = MongoPool.Instance.GetClient(connectionString);
            var database = client.GetDatabase(databaseName);
            _collection = database.GetCollection<TAggregate>(collectionName);
        }
 
        public Task<TAggregate> GetById(Guid id)
        {
            return _collection.Find(x => x.Id == id).FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<TAggregate>> GetAll()
        {
            return await _collection.Find(new BsonDocument()).ToListAsync();
        }

        public Task<IQueryable<TAggregate>> Get()
        {
            return Task.FromResult(_collection.AsQueryable<TAggregate>() as IQueryable<TAggregate>);
        }

        public async Task Save(TAggregate item)
        {
            var filter = Builders<TAggregate>.Filter.Eq(s => s.Id, item.Id);
            var result = await _collection.ReplaceOneAsync(filter, item);
        }

        public void SaveAll(IEnumerable<TAggregate> items)
        {
            Parallel.ForEach(items, async x => await Save(x));
        }

        public void Delete(Guid id)
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
            //_collection.RemoveAll();
        }

        public long Count()
        {
            //return _collection.Count();
            return 0;
        }
    }
}
