using System;
using System.Collections.Generic;
using System.Linq;
using DDD.Light.Contracts.Repo;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Threading.Tasks;

namespace DDD.Light.Repository.MongoDB
{
    public class MongoRepository<TAggregate> : IRepository<TAggregate>
        where TAggregate : IEntity
    {
        private readonly IMongoCollection<TAggregate> _collection;

        public MongoRepository(string connectionString, string databaseName, string collectionName)
        {
            var client = MongoPool.Instance.GetClient(connectionString);
            var database = client.GetDatabase(databaseName);
            _collection = database.GetCollection<TAggregate>(collectionName);
        }

        public async Task<TAggregate> GetByIdAsync(Guid id)
        {
            var filter = Builders<TAggregate>.Filter.Eq(s => s.Id, id);
            using (var cursor = await _collection.FindAsync(filter))
            {
                return await cursor.FirstOrDefaultAsync();
            }
        }

        public async Task<IEnumerable<TAggregate>> GetAllAsync()
        {
            using (var cursor = await _collection.FindAsync(new BsonDocument()))
            {
                return await cursor.ToListAsync();
            }
        }

        public async Task<IQueryable<TAggregate>> GetAsync()
        {
            using (var cursor = await _collection.FindAsync(new BsonDocument()))
            {
                return (await cursor.ToListAsync()).AsQueryable();
            }
        }

        public async Task SaveAsync(TAggregate item)
        {
            var filter = Builders<TAggregate>.Filter.Eq(s => s.Id, item.Id);
            var result = await _collection.ReplaceOneAsync(filter, item);
        }

        public Task SaveAllAsync(IEnumerable<TAggregate> items)
        {
            Parallel.ForEach(items, async x => await SaveAsync(x));
            return Task.FromResult<object>(null);
        }

        public async Task DeleteAsync(Guid id)
        {
            var filter = Builders<TAggregate>.Filter.Eq(s => s.Id, id);
            await _collection.DeleteOneAsync(filter);
        }

        public async Task DeleteAsync(TAggregate item)
        {
            await DeleteAsync(item.Id);
        }

        public async Task DeleteAllAsync()
        {
            await _collection.DeleteManyAsync(new BsonDocument());
        }

        public async Task<long> CountAsync()
        {
            return await _collection.CountAsync(new BsonDocument());
        }
    }
}
