using System;
using System.Reflection;
using CQRS.Light.Repository.MongoDB;
using CQRS.Light.Repository.InMemory;
//using log4net;
//using log4net.Config;
using CQRS.Light.Core;
using CQRS.Light.Contracts;

namespace DDD.Light.Messaging.Example
{
    class Program
    {
        static void Main(string[] args)
        {
            //XmlConfigurator.Configure();
            //var log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
            //EventStore.Instance.Configure(new MongoRepository<AggregateEvent>("mongodb://localhost", "DDD_Light_Messaging_Example", "EventStore"), new JsonSerializationStrategy());
            EventStore.Instance.Configure(new InMemoryRepository<AggregateEvent>(), new JsonSerializationStrategy());
            EventBus.Instance.Configure(EventStore.Instance, new JsonSerializationStrategy(), false);


            //log.Info("------- START ---------");
            Console.WriteLine("------- START ---------");

            // subscribe event handlers to handle events
            // in real life subscription would occur on app start, in Global.asax.cs on web apps
            // handle method in real life would call method(s) on aggregate root entity
            EventBus.Instance.Subscribe(new PersonLeftEventHandler());
            EventBus.Instance.Subscribe(new PersonLeftAndSpokeEventHandler("good bye"));
            EventBus.Instance.Subscribe(new PersonArrivedEventHandler());

            //todo: refer to RealtorApp for a CQRS example. Might update this one later
            // publish events to state something was done
            // events in real life would be published from methods in aggregate root entity
           var id = Guid.NewGuid();
            EventBus.Instance.PublishAsync(typeof(Person), id, new PersonLeftEvent("Jane Doe", "California")).ConfigureAwait(true);
            //            EventBus.Instance.Publish(Id, new PersonLeftEvent("Jane Doe", "California"));
            //            EventBus.Instance.Publish<PersonLeftEvent>(Id, null);
            //            EventBus.Instance.Publish(Id, new PersonArrivedEvent("Jane Doe", "New York"));

            //log.Info("------- END ---------");

            EventBus.Instance.RestoreReadModelAync().ConfigureAwait(true);
            Console.WriteLine("------- END ---------");

            Console.ReadLine();

        }
    }
}
