using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DDD.Light.Contracts.CQRS
{
    public interface ICommandHandlersDatabase<T>
    {
        void Add(ICommandHandler<T> commandHandler);
        void Add(Func<T, Task> handleMethod);
        IEnumerable<Func<T, Task>> Get();
    }

}