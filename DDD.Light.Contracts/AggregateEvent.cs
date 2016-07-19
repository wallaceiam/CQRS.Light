using System;
using DDD.Light.Contracts.Repo;

namespace DDD.Light.Contracts.EventStore
{  
    public class AggregateEvent : IEntity
    {
        public Guid Id { get; set; }
        public string AggregateType { get; set; }
        public DateTime CreatedOn { get; set; }
        public string EventType { get; set; }        
        public string SerializedEvent { get; set; }
        public string AggregateIdType { get; set; }
        public string SerializedAggregateId { get; set; }
    }
}