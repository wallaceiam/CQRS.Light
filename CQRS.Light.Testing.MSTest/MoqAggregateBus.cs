using CQRS.Light.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CQRS.Light.Testing.MSTest
{
    public class MoqAggregateBus : IAggregateBus
    {
        private static volatile IAggregateBus _instance;
        private static object token = new Object();
        private Queue<object> raisedEvents = new Queue<object>();

        public static IAggregateBus Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (token)
                    {
                        if (_instance == null)
                            _instance = new MoqAggregateBus();
                    }
                }
                return _instance;
            }
        }

        public List<IAggregateCache> RegisteredAggregateCaches
        {
            get { return null; }
        }

        public Queue<object> RaisedEvents { get { return this.raisedEvents; } }

        public void Configure(IEventBus eventBus)
        {
            this.Configure(eventBus, null);
        }

        public void Configure(IEventBus eventBus, IAggregateCache aggregateCache)
        {
        }

        public void Reset()
        {
            this.raisedEvents.Clear();
        }

        private MoqAggregateBus()
        {
        }


        public async Task PublishAsync<TAggregate, TEvent>(Guid aggregateId, TEvent @event) where TAggregate : IAggregateRoot
        {
            this.raisedEvents.Enqueue(@event);
            await Task.FromResult<object>(null);
        }

    }
}
