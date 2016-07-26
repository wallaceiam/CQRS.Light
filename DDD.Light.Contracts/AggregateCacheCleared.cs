using System;

namespace DDD.Light.Contracts.CQRS
{
    public class AggregateCacheCleared
    {
        public string SerializedAggregateId { get; private set; }
        public Type AggregateIdType { get; private set; }
        public Type AggregateType { get; private set; }

        public AggregateCacheCleared(string serializedAggregateId, Type aggregateIdType, Type aggregateType)
        {
            if (string.IsNullOrWhiteSpace(serializedAggregateId))
                throw new ArgumentNullException("serializedAggregateId");

            SerializedAggregateId = serializedAggregateId;
            AggregateIdType = aggregateIdType;
            AggregateType = aggregateType;
        }
    }
}