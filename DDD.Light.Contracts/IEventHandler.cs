using System.Threading.Tasks;

namespace DDD.Light.Contracts.CQRS
{
    public interface IEventHandler<T>
    {
        Task HandleAsync(T @event);
    }
}