using System;
using System.Reflection;
using CQRS.Light.Contracts;
using CQRS.Light.Core;
using System.Threading.Tasks;

namespace CQRS.Light.Core
{
    public abstract class AggregateRoot : Entity, IAggregateRoot
    {
        private IAggregateBus _aggregateBus;
        protected AggregateRoot(IAggregateBus aggregateBus)
        {
            
        }

        protected AggregateRoot(IAggregateBus aggregateBus, Guid id)
            :this(aggregateBus)
        {
            Id = id;
            _aggregateBus = aggregateBus;
        }

        public async Task PublishAndApplyEventAsync<TAggregate, TEvent>(TEvent @event) where TAggregate : IAggregateRoot
        {
            await _aggregateBus.PublishAsync<TAggregate, TEvent>(Id, @event);
            ApplyEventOnAggregate(@event);
        }

        public async Task PublishAndApplyEventAsync<TEvent>(TEvent @event)
        {
            await PublishOnAggregateBusThroughReflectionAsync(@event);
            ApplyEventOnAggregate(@event);
        }

        private async Task PublishOnAggregateBusThroughReflectionAsync<TEvent>(TEvent @event)
        {
            try
            {
                var publishMethod = typeof (AggregateBus).GetMethod("PublishAsync");
                var genericPublishMethod = publishMethod.MakeGenericMethod(new[] {GetType(), typeof (TEvent)});
                var result = genericPublishMethod.Invoke(_aggregateBus, new[] { Id, @event as Object }) as Task;
                await result;
            }
            catch (Exception ex)
            {
                throw new ApplicationException(string.Format("CQRS.Light.Core.AggregateRoot -> ApplyEventOnAggregate: Failed to apply event on aggregate type: {0} through reflection. Event type {1} did not get applied.  Are you missing a private ApplyEvent({1} @event) on {0}?", GetType(), typeof(TEvent)), ex);
            }
        }

        private void ApplyEventOnAggregate<TEvent>(TEvent @event)
        {
            try
            {
                var method = GetType().GetMethod("ApplyEvent", BindingFlags.NonPublic | BindingFlags.Instance, null, new[] {typeof (TEvent)}, null);
                method.Invoke(this, new[] {@event as Object});
            }
            catch (Exception ex)
            {
                throw new ApplicationException(string.Format("CQRS.Light.Core.AggregateRoot -> ApplyEventOnAggregate: Failed to apply event on aggregate type: {0} through reflection. Event type {1} did not get applied.  Are you missing a private ApplyEvent({1} @event) on {0}?", GetType(), typeof(TEvent)), ex);
            }
        }
    }
}