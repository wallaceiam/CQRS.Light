using System;
using CQRS.Light.Contracts;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace CQRS.Light.Core
{
    public class JsonSerializationStrategy : ISerializationStrategy
    {
        private readonly JsonSerializerSettings deserializeSettings = new JsonSerializerSettings() { ContractResolver = new JsonSerializationContractResolver() };
        public string Serialize(object @object)
        {
            return JsonConvert.SerializeObject(@object);
        }

        public object Deserialize(string serializedObject, Type objectType)
        {
            return JsonConvert.DeserializeObject(serializedObject, objectType, deserializeSettings);
        }
    }

    internal class JsonSerializationContractResolver : Newtonsoft.Json.Serialization.DefaultContractResolver
    {
        protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
        {
            var props = type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                            .Select(p => base.CreateProperty(p, memberSerialization))
                        .Union(type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                                   .Select(f => base.CreateProperty(f, memberSerialization)))
                        .ToList();
            props.ForEach(p => { p.Writable = true; p.Readable = true; });
            return props;
        }
    }
}