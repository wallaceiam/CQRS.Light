using System;

namespace CQRS.Light.Contracts
{
    public class AggregateCacheCleared
    {
        public Guid AggregateId { get; private set; }
        public Type AggregateIdType { get; private set; }
        public Type AggregateType { get; private set; }

        public AggregateCacheCleared(Guid aggregateId, Type aggregateIdType, Type aggregateType)
        {
            if (aggregateId == Guid.Empty )
                throw new ArgumentNullException("serializedAggregateId");

            AggregateId = aggregateId;
            AggregateIdType = aggregateIdType;
            AggregateType = aggregateType;
        }
    }
}