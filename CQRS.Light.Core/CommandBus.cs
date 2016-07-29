using System;
using System.Linq;
using System.Threading.Tasks;
using CQRS.Light.Contracts;

namespace CQRS.Light.Core
{
    public class CommandBus : ICommandBus
    {
        private static volatile ICommandBus _instance;
        private static object token = new Object();

        public static ICommandBus Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (token)
                    {
                        if (_instance == null)
                            _instance = new CommandBus();
                    }
                }
                return _instance;
            }
        }

        private CommandBus(){}

        public void Subscribe<T>(ICommandHandler<T> handler)
        {
            CommandHandlersDatabase<T>.Instance.Add(handler);
        }

        public void Subscribe<T>(Func<T, Task> handleMethod)
        {
            CommandHandlersDatabase<T>.Instance.Add(handleMethod);
        }

        public async Task DispatchAsync<T>(T command) 
        {
            if (!Equals(command, default(T)))
            {
                var transaction = new Transaction<T>(command, CommandHandlersDatabase<T>.Instance.Get().ToList());
                await transaction.CommitAsync();
            }
        }
    }
}
