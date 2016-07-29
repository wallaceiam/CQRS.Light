using System;
using CQRS.Light.Contracts;
using Newtonsoft.Json;

namespace CQRS.Light.Core
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