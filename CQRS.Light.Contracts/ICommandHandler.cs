using System.Threading.Tasks;

namespace CQRS.Light.Contracts
{
    public interface ICommandHandler<T>
    {
        Task HandleAsync(T command);
    }
}