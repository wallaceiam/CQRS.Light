using DDD.Light.CQRS.Contracts;
using System.Threading.Tasks;

namespace DDD.Light.CQRS.InProcess
{
    public abstract class CommandHandler<T> : ICommandHandler<T>, IHandler
    {
        public abstract void Handle(T command);
        public abstract Task HandleAsync(T command);
        public void Subscribe()
        {
            CommandBus.Instance.Subscribe(this);
        }
    }
}