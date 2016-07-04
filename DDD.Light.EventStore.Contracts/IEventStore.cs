﻿using System;
using System.Collections.Generic;
using DDD.Light.Repo.Contracts;
using System.Threading.Tasks;

namespace DDD.Light.EventStore.Contracts
{
    public interface IEventStore
    {
        TAggregate GetById<TId, TAggregate>(TId id);
        TAggregate GetById<TId, TAggregate>(TId id, DateTime until);
        object GetById<TId>(TId id);
        void Save(AggregateEvent aggregateEvent);
        void Configure(IRepository<Guid, AggregateEvent> repo, IEventSerializationStrategy serializationStrategy);
        Task<IEnumerable<AggregateEvent>> GetAll();
        IEnumerable<AggregateEvent> GetAll(DateTime until);
        long Count();
        DateTime LatestEventTimestamp<TId>(TId aggregateId);
        Task<IEnumerable<TEvent>> GetEvents<TEvent>();
    }
}