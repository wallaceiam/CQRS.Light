using System;

namespace DDD.Light.Contracts.EventStore
{
    public interface IEventSerializationStrategy
    {
        string SerializeEvent(object @event);
        object DeserializeEvent(string serializedEvent, Type eventType);
    }
}