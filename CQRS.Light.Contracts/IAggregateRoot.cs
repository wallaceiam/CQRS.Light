using System.Threading.Tasks;

namespace CQRS.Light.Contracts
{
    public interface IAggregateRoot : IEntity
    {
        Task PublishAndApplyEventAsync<TEvent>(TEvent @event);
        Task PublishAndApplyEventAsync<TAggregate, TEvent>(TEvent @event) where TAggregate : IAggregateRoot;
    }
}
