using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CQRS.Light.Contracts
{
    public interface ICommandHandlersDatabase<T>
    {
        void Add(ICommandHandler<T> commandHandler);
        void Add(Func<T, Task> handleMethod);
        IEnumerable<Func<T, Task>> Get();
    }

}