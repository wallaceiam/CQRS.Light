using DDD.Light.Contracts.Repo;
using System.Threading.Tasks;

namespace DDD.Light.Contracts.CQRS
{
    public interface IAggregateRoot : IEntity
    {
        Task PublishAndApplyEventAsync<TEvent>(TEvent @event);
        Task PublishAndApplyEventAsync<TAggregate, TEvent>(TEvent @event) where TAggregate : IAggregateRoot;
    }
}
