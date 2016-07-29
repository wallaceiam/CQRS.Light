using System;

namespace CQRS.Light.Contracts
{
    public interface IEventSerializationStrategy
    {
        string SerializeEvent(object @event);
        object DeserializeEvent(string serializedEvent, Type eventType);
    }
}