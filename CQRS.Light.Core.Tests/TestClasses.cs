using CQRS.Light.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDD.Light.Core.Tests
{
    public class TestAggregate : IAggregateRoot
    {

        public Guid Id { get; set; }

        public bool WasApplyEventCalled { get; protected set; }

        public TestAggregate()
        {
            this.WasApplyEventCalled = false;
        }

        public Task PublishAndApplyEventAsync<TEvent>(TEvent @event)
        {
            throw new NotImplementedException();
        }

        public Task PublishAndApplyEventAsync<TAggregate, TEvent>(TEvent @event) where TAggregate : IAggregateRoot
        {
            throw new NotImplementedException();
        }

        protected void ApplyEvent(TestEvent @event)
        {
            this.WasApplyEventCalled = true;
        }
    }

    public class MissingApplyTestAggregate : IAggregateRoot
    {

        public Guid Id { get; set; }

        public Task PublishAndApplyEventAsync<TEvent>(TEvent @event)
        {
            throw new NotImplementedException();
        }

        public Task PublishAndApplyEventAsync<TAggregate, TEvent>(TEvent @event) where TAggregate : IAggregateRoot
        {
            throw new NotImplementedException();
        }

        public void ThisDoesNotApplyEvent(TestEvent @event)
        {

        }
    }
    public class TestEvent
    {

    }
}
