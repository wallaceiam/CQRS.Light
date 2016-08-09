using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CQRS.Light.Core;
using FluentAssertions;

namespace DDD.Light.Core.Tests
{
    [TestClass]
    public class JsonSerializationStrategyTests
    {
        [TestMethod]
        public void JsonSerializationStrategy_SerializeShouldReturnAValidString()
        {
            var strategy = new JsonSerializationStrategy();

            var result = strategy.Serialize(new SerializableClass() { Bool = true, Int = 123, String = "345" });

            result.Should().Be("{\"Bool\":true,\"Int\":123,\"String\":\"345\"}");
        }

        [TestMethod]
        public void JsonSerializationStrategy_DeserializeShouldReturnAValidObject()
        {
            var strategy = new JsonSerializationStrategy();

            var result = strategy.Deserialize("{\"Bool\":true,\"Int\":123,\"String\":\"345\"}", typeof(SerializableClass));

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
