using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DDD.Light.Contracts.CQRS
{
    public interface IEventHandlersDatabase<T>
    {
        void Add(IEventHandler<T> eventHandler);
        void Add(Func<T, Task> handleMethod);
        IEnumerable<Func<T, Task>> Get();
    }

}