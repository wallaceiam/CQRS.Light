using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CQRS.Light.Core;
using FluentAssertions;

namespace DDD.Light.Core.Tests
{
    [TestClass]
    public class AggregateRootTests
    {
        [TestMethod]
        public void AggregateRoot_ConstructorShouldAssignValues()
        {
            var guid = Guid.NewGuid();
            var testAggreageRoot = new TestAggregateRoot(null, guid);

            testAggreageRoot.Id.Should().Be(guid);
        }
    }

}
