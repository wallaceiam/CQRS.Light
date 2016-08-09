using System;

namespace CQRS.Light.Contracts
{
    public interface ISerializationStrategy
    {
        string Serialize(object @object);
        object Deserialize(string serializedObject, Type objectType);
    }
}