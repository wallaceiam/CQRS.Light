using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CQRS.Light.Contracts;
using FluentAssertions;

namespace DDD.Light.Core.Tests
{
    [TestClass]
    public class AggregateCacheClearedTests
    {
        [TestMethod]
        public void AggregateCacheCleared_ConstructorAssignsValues()
        {
            var aggregateCache = new AggregateCacheCleared(
                "myserializedid",
                typeof(string),
                typeof(AggregateCacheCleared));

            aggregateCache.SerializedAggregateId.Should().Be("myserializedid");
            aggregateCache.AggregateIdType.Should().Be(typeof(string));
            aggregateCache.AggregateType.Should().Be(typeof(AggregateCacheCleared));
        }

        [TestMethod]
        public void AggregateCacheCleared_ConstructorDoesntLikeNulls()
        {
            // Arrange
            Action a = () => new AggregateCacheCleared(
                null,
                typeof(string),
                typeof(AggregateCacheCleared));         // null is an invalid argument

            a.ShouldThrow<ArgumentNullException>();
        }
    }
}
