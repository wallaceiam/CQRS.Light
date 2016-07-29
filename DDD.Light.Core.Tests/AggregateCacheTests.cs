using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FluentAssertions;
using Moq;
using CQRS.Light.Contracts;
using CQRS.Light.Core;

namespace DDD.Light.Core.Tests
{
    [TestClass]
    public class AggregateCacheTests
    {
        [TestInitialize]
        public void TestInitialize()
        {
            AggregateCache.Instance.Reset();
        }
        [TestMethod]
        public void ShouldBeOnlyOneInstanceOfAnAggregateCache()
        {
            var a = AggregateCache.Instance;
            var b = AggregateCache.Instance;

            a.ShouldBeEquivalentTo(b);
        }

        [TestMethod]
        public void ConfigureShouldAssignValues()
        {
            var eventStore = new Mock<IEventStore>();

            AggregateCache.Instance.Configure(eventStore.Object, null);

            
        }
    }
}
