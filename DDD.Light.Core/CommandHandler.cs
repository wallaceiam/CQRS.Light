using CQRS.Light.Contracts;
using System.Threading.Tasks;

namespace CQRS.Light.Core
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