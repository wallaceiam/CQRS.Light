using System;
using System.Collections.Generic;

namespace DDD.Light.Contracts.CQRS
{
    public interface ICommandHandlersDatabase<T>
    {
        void Add(ICommandHandler<T> commandHandler);
        void Add(Action<T> handleMethod);
        IEnumerable<Action<T>> Get();
    }

}