using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace CQRS.Light.Core
{
    public class Transaction<T>
    {
        private Transaction(){}

        public Transaction(T message, IEnumerable<Func<T, Task>> handlers)
            :this()
        {
            Message = message;
            Id = Guid.NewGuid();
            ProcessedActions = new List<Func<T, Task>>();
            NotProcessedActions = new Queue<Func<T, Task>>(handlers);
        }

        public Guid Id { get; private set; }
        public T Message { get; set; }
        public List<Func<T, Task>> ProcessedActions { get; private set; }
        public Queue<Func<T, Task>> NotProcessedActions { get; private set; }

        public async Task CommitAsync()
        {
            while (NotProcessedActions.Count > 0)
            {
                var handler = NotProcessedActions.Peek();
                try
                {
                    await handler.Invoke(Message);
                    ProcessedActions.Add(handler);
                    NotProcessedActions.Dequeue(); //Remove it if everything is fine
                }
                catch (Exception ex)
                {
                    //To Do: Rollback?!
                    throw;
                }
            }
        }
    }
}