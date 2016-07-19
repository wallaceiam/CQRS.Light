using DDD.Light.Contracts.Repo;

namespace DDD.Light.Contracts.CQRS
{
    public interface IAggregateRoot : IEntity
    {
        void PublishAndApplyEvent<TEvent>(TEvent @event);
        void PublishAndApplyEvent<TAggregate, TEvent>(TEvent @event) where TAggregate : IAggregateRoot;
    }
}
