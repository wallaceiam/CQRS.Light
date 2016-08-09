using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CQRS.Light.Contracts;
using FluentAssertions;

namespace DDD.Light.Core.Tests
{
    [TestClass]
    public class EntityTests
    {
        [TestMethod]
        public void Entity_ConstructorAssignsValues()
        {
            var guid = Guid.NewGuid();
            var entity = new TestEntity(guid);

            entity.Id.Should().Be(guid);
        }

        public class TestEntity : Entity
        {
            public TestEntity(Guid id)
                :base(id)
            {

            }
        }
    }
}
