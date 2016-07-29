using System;
using System.Reflection;
using CQRS.Light.Contracts;
using CQRS.Light.Core;
using System.Threading.Tasks;

namespace CQRS.Light.Core
{
    public abstract class AggregateRoot : Entity, IAggregateRoot
    {
        protected AggregateRoot()
        {
            
        }

        protected AggregateRoot(Guid id)
        {
            Id = id;
        }

        public async Task PublishAndApplyEventAsync<TAggregate, TEvent>(TEvent @event) where TAggregate : IAggregateRoot
        {
            await AggregateBus.Instance.PublishAsync<TAggregate, TEvent>(Id, @event);
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
                var result = (Task)genericPublishMethod.Invoke(AggregateBus.Instance, new[] {Id, @event as Object});
                await result;
            }
            catch (Exception ex)
            {
                throw new ApplicationException(string.Format("DDD.Light.Core.AggregateRoot -> PublishOnAggregateBusThroughReflection: Failed to get and invoke Publish method on AggregateBus.Instance. Event type {0} did not get published", typeof(TEvent)), ex);
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
                throw new ApplicationException(string.Format("DDD.Light.Core.InProcess.AggregateRoot -> ApplyEventOnAggregate: Failed to apply event on aggregate type: {0} through reflection. Event type {1} did not get applied", GetType(), typeof(TEvent)), ex);
            }
        }
    }
}