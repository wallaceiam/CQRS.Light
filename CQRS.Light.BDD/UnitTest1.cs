using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CQRS.Light.Contracts;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace CQRS.Light.BDD
{
    [TestClass]
    public class UnitTest1 : BDDTest<SimplePersonAggregate>
    {
        [TestMethod]
        public void SimpleBDDRename()
        {
            var guid = Guid.NewGuid();

            Given(new Born(guid)).
            When(x => x.RenameAsync("John Smith")).
            Then(new Renamed(guid, "John Smith"));
        }

        [TestMethod]
        public void OtherBDDRename()
        {
            var guid = Guid.NewGuid();

            Given(new Born(guid)).
            When(x => x.RenameAsync("Jane Smith")).
            Then(new Renamed(guid, "Jane Smith")).
            AndThen(x => Assert.AreEqual(x.Name, "Jane Smith"));
        }

        [TestMethod]
        public void CannotRenameTwice()
        {
            var guid = Guid.NewGuid();

            Given(new Born(guid)).
                AndGiven(new Renamed(guid, "John Smith")).
            When(x => x.RenameAsync("Jane Smith")).
            ThenFailWith<PersonalAlreadyHasANameException>();
            AndThen(x => Assert.AreEqual(x.Name, "John Smith"));
        }
    }

    public class SimplePersonAggregate : AggregateRoot
    {
        public SimplePersonAggregate()
            : this(MoqAggregateBus.Instance)
        {

        }

        public SimplePersonAggregate(IAggregateBus aggregateBus)
            : base(aggregateBus)
        {

        }

        public string Name { get; protected set; }

        public async Task RenameAsync(string name)
        {
            if (!string.IsNullOrWhiteSpace(this.Name))
                throw new PersonalAlreadyHasANameException();

            await PublishAndApplyEventAsync<Renamed>(new Renamed(this.Id, name));
        }

        private void ApplyEvent(Born @event)
        {
            this.Id = @event.AggreageId;
        }

        private void ApplyEvent(Renamed @event)
        {
            this.Name = @event.Name;
        }
    }

    public class Born
    {
        public Guid AggreageId { get; protected set; }

        public Born(Guid aggregateId)
        {
            this.AggreageId = aggregateId;
        }
    }

    public class Renamed
    {
        public Guid AggreageId { get; protected set; }
        public string Name { get; protected set; }

        public Renamed(Guid aggregateId, string name)
        {
            this.AggreageId = aggregateId;
            this.Name = name;
        }
    }

    public class Rename
    {
        public Guid AggreageId { get; protected set; }
        public string Name { get; protected set; }

        public Rename(Guid aggregateId, string name)
        {
            this.AggreageId = aggregateId;
            this.Name = name;
        }
    }

    public class PersonalAlreadyHasANameException : Exception
    {

    }

    
}
