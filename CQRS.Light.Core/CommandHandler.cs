using CQRS.Light.Contracts;
using System.Threading.Tasks;

namespace CQRS.Light.Core
{
    public abstract class CommandHandler<T> : ICommandHandler<T>, IHandler
    {
        private ICommandBus _commandBus;
        public CommandHandler(ICommandBus commandBus)
            :this()
        {
            _commandBus = commandBus;
        }

        private CommandHandler()
        {

        }

        public abstract Task HandleAsync(T command);
        public void Subscribe()
        {
            _commandBus.Subscribe(this);
        }
    }
}