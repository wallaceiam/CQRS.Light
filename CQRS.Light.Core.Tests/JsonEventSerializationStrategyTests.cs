using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CQRS.Light.Core;
using FluentAssertions;

namespace DDD.Light.Core.Tests
{
    [TestClass]
    public class JsonEventSerializationStrategyTests
    {
        [TestMethod]
        public void JsonEventSerializationStrategy_SerializeShouldReturnAValidString()
        {
            var strategy = new JsonEventSerializationStrategy();

            var result = strategy.SerializeEvent(new SerializableClass() { Bool = true, Int = 123, String = "345" });

            result.Should().Be("{\"Bool\":true,\"Int\":123,\"String\":\"345\"}");
        }

        [TestMethod]
        public void JsonEventSerializationStrategy_DeserializeShouldReturnAValidObject()
        {
            var strategy = new JsonEventSerializationStrategy();

            var result = strategy.DeserializeEvent("{\"Bool\":true,\"Int\":123,\"String\":\"345\"}", typeof(SerializableClass));

            result.Should().NotBeNull();
            result.Should().BeOfType(typeof(SerializableClass));
            (result as SerializableClass).Bool.Should().BeTrue();
            (result as SerializableClass).Int.Should().Be(123);
            (result as SerializableClass).String.Should().Be("345");
        }
    }

    public class SerializableClass
    {
        public bool Bool { get; set; }
        public int Int { get; set; }

        public string String { get; set; }
    }
}
