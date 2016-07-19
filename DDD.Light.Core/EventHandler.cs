using DDD.Light.Contracts.CQRS;
using System.Threading.Tasks;

namespace DDD.Light.CQRS
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