namespace DDD.Light.Contracts.CQRS
{
    public interface IEventHandler<T>
    {
        void Handle(T @event);
    }
}