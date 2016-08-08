using System;
using System.Collections.Generic;
using CQRS.Light.Contracts;
using System.Threading.Tasks;

namespace CQRS.Light.Core
{
    public class EventHandlersDatabase<T> : IEventHandlersDatabase<T>
    {
        private static volatile IEventHandlersDatabase<T> _instance;
        private static object token = new Object();
        private readonly List<Func<T, Task>> _registeredHandlerActions;

        public static IEventHandlersDatabase<T> Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (token)
                    {
                        if (_instance == null)
                            _instance = new EventHandlersDatabase<T>();
                    }
                }
                return _instance;
            }
        }

        private EventHandlersDatabase()
        {
            _registeredHandlerActions = new List<Func<T, Task>>();
        }


        public void Add(IEventHandler<T> eventHandler)
        {
            Add(eventHandler.HandleAsync);
        }

        public void Add(Func<T, Task> eventHandlerAction)
        {
            _registeredHandlerActions.Add(eventHandlerAction);
        }

        public IEnumerable<Func<T,Task>> Get()
        {
            return _registeredHandlerActions;
        }

        public void Clear()
        {
            _registeredHandlerActions.Clear();
        }

    }
}