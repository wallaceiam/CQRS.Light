using DDD.Light.Contracts.CQRS;

namespace DDD.Light.CQRS
{
    public abstract class EventHandler<T> : IEventHandler<T>, IHandler
    {
        public abstract void Handle(T @event);
        public void Subscribe()
        {
            EventBus.Instance.Subscribe(this);
        }
    }
}