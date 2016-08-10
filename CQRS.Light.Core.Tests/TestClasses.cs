using CQRS.Light.Contracts;
using CQRS.Light.Core;
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

    public class TestAggregateRoot : AggregateRoot
    {
        public TestAggregateRoot(IAggregateBus aggregateBus, Guid guid)
            : base(aggregateBus, guid)
        {
            this.WasApplyEventCalled = false;
        }

        protected TestAggregateRoot(IAggregateBus aggregateBus)
            :base(aggregateBus)
        {

        }

        public bool WasApplyEventCalled { get; protected set; }

        private void ApplyEvent(TestEvent @event)
        {
            this.WasApplyEventCalled = true;
        }

        private void ApplyEventDoesNotApply(TestEvent2 @event)
        {

        }
    }

    public class MissingConstructorAggregateRoot : AggregateRoot
    {
        public MissingConstructorAggregateRoot(IAggregateBus aggregateBus, Guid guid)
            : base(aggregateBus, guid)
        {
            this.WasApplyEventCalled = false;
        }


        public bool WasApplyEventCalled { get; protected set; }

        private void ApplyEvent(TestEvent @event)
        {
            this.WasApplyEventCalled = true;
        }

        private void ApplyEventDoesNotApply(TestEvent2 @event)
        {

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

    public class TestEvent2
    {

    }

    public class TestCommand
    {

    }

    public class TestCommandHandler : CommandHandler<TestCommand>
    {
        public TestCommandHandler(ICommandBus commandBus)
            :base(commandBus)
        {

        }
        public bool WasHandleAsyncCalled { get; protected set; }
        public override Task HandleAsync(TestCommand command)
        {
            this.WasHandleAsyncCalled = true;
            return Task.FromResult<object>(null);
        }
    }

    public class TestEventHandler : CQRS.Light.Core.EventHandler<TestEvent>
    {
        public TestEventHandler(IEventBus eventBus)
            : base(eventBus)
        {

        }

        public override Task HandleAsync(TestEvent @event)
        {
            throw new NotImplementedException();
        }
    }
}
