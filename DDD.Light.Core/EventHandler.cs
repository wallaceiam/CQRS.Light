using CQRS.Light.Contracts;
using System.Threading.Tasks;

namespace CQRS.Light.Core
{
    public abstract class EventHandler<T> : IEventHandler<T>, IHandler
    {
        public abstract Task HandleAsync(T @event);
        public void Subscribe()
        {
            EventBus.Instance.Subscribe(this);
        }
    }
}