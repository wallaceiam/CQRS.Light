using CQRS.Light.Contracts;
using System.Threading.Tasks;

namespace CQRS.Light.Core
{
    public abstract class EventHandler<T> : IEventHandler<T>, IHandler
    {
        private IEventBus _eventBus;
        public EventHandler(IEventBus eventBus)
        {
            _eventBus = eventBus;
        }
        public abstract Task HandleAsync(T @event);
        public void Subscribe()
        {
            _eventBus.Subscribe(this);
        }
    }
}