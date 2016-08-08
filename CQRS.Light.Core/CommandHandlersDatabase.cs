using System;
using System.Collections.Generic;
using CQRS.Light.Contracts;
using System.Threading.Tasks;

namespace CQRS.Light.Core
{
    public class CommandHandlersDatabase<T> : ICommandHandlersDatabase<T>
    {
        private static volatile ICommandHandlersDatabase<T> _instance;
        private static object token = new Object();
        private readonly List<Func<T, Task>> _registeredHandlerActions;

        public static ICommandHandlersDatabase<T> Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (token)
                    {
                        if (_instance == null)
                            _instance = new CommandHandlersDatabase<T>();
                    }
                }
                return _instance;
            }
        }

        private CommandHandlersDatabase()
        {
            _registeredHandlerActions = new List<Func<T, Task>>();
        }

        public void Add(ICommandHandler<T> commandHandler)
        {
            Add(commandHandler.HandleAsync);
        }

        public void Add(Func<T, Task> commandHandlerAction)
        {
            _registeredHandlerActions.Add(commandHandlerAction);
        }

        public IEnumerable<Func<T, Task>> Get()
        {
            return _registeredHandlerActions;
        }

        public void Clear()
        {
            _registeredHandlerActions.Clear();
        }

    }
}