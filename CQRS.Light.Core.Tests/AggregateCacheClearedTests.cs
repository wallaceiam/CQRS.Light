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
            var guid = Guid.NewGuid();
            var aggregateCache = new AggregateCacheCleared(
                guid,
                typeof(string),
                typeof(AggregateCacheCleared));

            aggregateCache.AggregateId.Should().Be(guid);
            aggregateCache.AggregateIdType.Should().Be(typeof(string));
            aggregateCache.AggregateType.Should().Be(typeof(AggregateCacheCleared));
        }

        [TestMethod]
        public void AggregateCacheCleared_ConstructorDoesntLikeNulls()
        {
            // Arrange
            Action a = () => new AggregateCacheCleared(
                Guid.Empty,
                typeof(string),
                typeof(AggregateCacheCleared));         // null is an invalid argument

            a.ShouldThrow<ArgumentNullException>();
        }
    }
}
