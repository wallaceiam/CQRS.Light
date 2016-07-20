using System;
using System.Threading.Tasks;

namespace DDD.Light.Contracts.CQRS
{
    public interface ICommandBus
    {
        void Subscribe<T>(ICommandHandler<T> handler);
        void Subscribe<T>(Func<T, Task> handler);
        void Dispatch<T>(T command);

        Task DispatchAsync<T>(T command);
    }
}