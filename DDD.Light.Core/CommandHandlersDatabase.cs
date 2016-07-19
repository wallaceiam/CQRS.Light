using System;
using System.Collections.Generic;
using DDD.Light.Contracts.CQRS;
using System.Threading.Tasks;

namespace DDD.Light.CQRS
{
    public class CommandHandlersDatabase<T> : ICommandHandlersDatabase<T>
    {
        private static volatile ICommandHandlersDatabase<T> _instance;
        private static object token = new Object();
        //private readonly List<Action<T>> _registeredHandlerActions;
        private readonly List<Task<T>> _registeredHandlerActions;

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
            _registeredHandlerActions = //new List<Action<T>>();
                new List<Task<T>>();
        }

        public void Add(ICommandHandler<T> commandHandler)
        {
            _registeredHandlerActions.Add(commandHandler.HandleAsync);
        }

        public void Add(Action<T> commandHandlerAction)
        {
            _registeredHandlerActions.Add(commandHandlerAction);
        }

        public IEnumerable<Action<T>> Get()
        {
            return _registeredHandlerActions;
        }

    }
}