using System;
using DDD.Light.Contracts.EventStore;
using Newtonsoft.Json;

namespace DDD.Light.Core
{
    public class JsonEventSerializationStrategy : IEventSerializationStrategy
    {
        public string SerializeEvent(object @event)
        {
            return JsonConvert.SerializeObject(@event);
        }

        public object DeserializeEvent(string serializedEvent, Type eventType)
        {
            return JsonConvert.DeserializeObject(serializedEvent, eventType);
        }
    }
}