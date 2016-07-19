using System.Threading.Tasks;

namespace DDD.Light.Contracts.CQRS
{
    public interface ICommandHandler<T>
    {
        void Handle(T command);

        Task HandleAsync(T command);
    }
}