using System;
using System.Threading.Tasks;

namespace DDD.Light.Contracts.CQRS
{
    public interface ICommandBus
    {
        void Subscribe<T>(ICommandHandler<T> handler);
        void Subscribe<T>(Func<T, Task> handler);

        Task DispatchAsync<T>(T command);
    }
}