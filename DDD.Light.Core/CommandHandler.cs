using DDD.Light.Contracts.CQRS;
using System.Threading.Tasks;

namespace DDD.Light.CQRS
{
    public abstract class CommandHandler<T> : ICommandHandler<T>, IHandler
    {
        public abstract Task HandleAsync(T command);
        public void Subscribe()
        {
            CommandBus.Instance.Subscribe(this);
        }
    }
}