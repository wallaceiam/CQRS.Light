using System;
using System.Collections.Generic;
using DDD.Light.Contracts.CQRS;
using System.Threading.Tasks;

namespace DDD.Light.CQRS
{
    public class EventHandlersDatabase<T> : IEventHandlersDatabase<T>
    {
        private static volatile IEventHandlersDatabase<T> _instance;
        private static object token = new Object();
        private readonly List<Task<T>> _registeredHandlerActions;

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
            _registeredHandlerActions = new List<Task<T>>();
            _instanceID = Guid.NewGuid();
        }

        private readonly Guid _instanceID;
        public Guid GetUniqueInstanceID()
        {
            return _instanceID;
        }

        public void Add(IEventHandler<T> eventHandler)
        {
            _registeredHandlerActions.Add(eventHandler.HandleAsync);
        }

        public void Add(Action<T> eventHandlerAction)
        {
            _registeredHandlerActions.Add(new Task<T>(eventHandlerAction));
        }

        public IEnumerable<Task<T>> Get()
        {
            return _registeredHandlerActions;
        }

    }
}