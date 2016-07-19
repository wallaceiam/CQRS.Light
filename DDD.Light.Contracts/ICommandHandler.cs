using System.Threading.Tasks;

namespace DDD.Light.Contracts.CQRS
{
    public interface ICommandHandler<T>
    {
        Task HandleAsync(T command);
    }
}