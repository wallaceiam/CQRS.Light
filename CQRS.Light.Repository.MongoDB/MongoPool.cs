using System;
using System.Collections.Generic;
using MongoDB.Driver;

namespace CQRS.Light.Repository.MongoDB
{
    public sealed class MongoPool
    {
        private static volatile MongoPool _instance;
        private static object token = new Object();
        private readonly Dictionary<string, IMongoClient> _mongoClients;

        private MongoPool()
        {
            _mongoClients = new Dictionary<string, IMongoClient>();
        }

        public static MongoPool Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (token)
                    {
                        if (_instance == null)
                            _instance = new MongoPool();
                    }
                }
                return _instance;
            }
        }

        public IMongoClient GetClient(string connectionString)
        {
            if (!_mongoClients.ContainsKey(connectionString))
            {
                var mongoClient = new MongoClient(connectionString);
                _mongoClients.Add(connectionString, mongoClient);
            }
            return _mongoClients[connectionString];
        }
    }
}
