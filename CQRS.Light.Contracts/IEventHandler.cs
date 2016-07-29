using System.Threading.Tasks;

namespace CQRS.Light.Contracts
{
    public interface IEventHandler<T>
    {
        Task HandleAsync(T @event);
    }
}