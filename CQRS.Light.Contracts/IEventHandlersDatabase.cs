using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CQRS.Light.Contracts
{
    public interface IEventHandlersDatabase<T>
    {
        void Add(IEventHandler<T> eventHandler);
        void Add(Func<T, Task> handleMethod);
        IEnumerable<Func<T, Task>> Get();
        void Clear();
    }

}