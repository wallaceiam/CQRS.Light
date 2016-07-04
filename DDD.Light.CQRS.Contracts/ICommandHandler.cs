using System.Threading.Tasks;

namespace DDD.Light.CQRS.Contracts
{
    public interface ICommandHandler<T>
    {
        void Handle(T command);

        Task HandleAsync(T command);
    }
}