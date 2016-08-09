using System;
using CQRS.Light.Contracts;
using Newtonsoft.Json;

namespace CQRS.Light.Core
{
    public class JsonSerializationStrategy : ISerializationStrategy
    {
        public string Serialize(object @object)
        {
            return JsonConvert.SerializeObject(@object);
        }

        public object Deserialize(string serializedObject, Type objectType)
        {
            return JsonConvert.DeserializeObject(serializedObject, objectType);
        }
    }
}