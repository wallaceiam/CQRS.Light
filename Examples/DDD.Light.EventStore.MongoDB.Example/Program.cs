using System;
using DDD.Light.CQRS;
using DDD.Light.Contracts.EventStore;
using DDD.Light.Contracts.Repo;
using DDD.Light.Core;
using System.Threading.Tasks;
using DDD.Light.Repository.InMemory;
using DDD.Light.Repository.MongoDB;

namespace DDD.Light.EventStore.MongoDB.Example
{
    class Program
    {
        static void Main(string[] args)
        {
            //var personReadModel = new MongoRepository<PersonDTO>("mongodb://localhost", "DDD_Light_MongoEventStore_Example", "Person_ReadModel");
            //var aggregateEventRepo = new MongoRepository<AggregateEvent>("mongodb://localhost", "DDD_Light_MongoEventStore_Example", "EventStore");

            var personReadModel = new InMemoryRepository<PersonDTO>();
            var aggregateEventRepo = new InMemoryRepository<AggregateEvent>();

            DDD.Light.Core.EventStore.Instance.Configure(aggregateEventRepo, new JsonEventSerializationStrategy());
            EventBus.Instance.Configure(DDD.Light.Core.EventStore.Instance, new JsonEventSerializationStrategy(), false);
            
            AggregateBus.Instance.Configure(EventBus.Instance, AggregateCache.Instance);
            EventBus.Instance.Subscribe(async (PersonCreated personCreated) =>
                {                    
                    var personDTO = new PersonDTO {Id = personCreated.Id};
                    await personReadModel.SaveAsync(personDTO);
                });
            
            EventBus.Instance.Subscribe(async (PersonNamed personNamed) =>
                {                    
                    var personDTO = await personReadModel.GetByIdAsync(personNamed.PersonId);
                    personDTO.Name = personNamed.Name;
                    personDTO.WasRenamed = false;
                    await personReadModel.SaveAsync(personDTO);
                });

            EventBus.Instance.Subscribe(async (PersonRenamed personRenamed) => 
                {                    
                    var personDTO = await personReadModel.GetByIdAsync(personRenamed.PersonId);
                    personDTO.Name = personRenamed.Name;                    
                    personDTO.WasRenamed = true;                    
                    await personReadModel.SaveAsync(personDTO);
                });


            NamePerson(personReadModel).ConfigureAwait(true);
            NameAndRenamePerson(personReadModel).ConfigureAwait(true);


            Console.ReadLine();
        }

        private static async Task NamePerson(IRepository<PersonDTO> personReadModel)
        {
            Console.Write("Enter person's Name: ");
            var name = Console.ReadLine();

            var id = Guid.NewGuid();
            var person = new Person(id);
            person.NameMe(name);

            var personDTO = await personReadModel.GetByIdAsync(id);
            Console.WriteLine("Person ID: " + personDTO.Id);
            Console.WriteLine("Person Name: " + personDTO.Name);
            Console.WriteLine("Person Was Renamed: " + personDTO.WasRenamed);
        }

        private static async Task NameAndRenamePerson(IRepository<PersonDTO> personReadModel)
        {
            Console.Write("Enter person's Name: ");
            var name = Console.ReadLine();

            var id = Guid.NewGuid();
            var person = new Person(id);
            person.NameMe(name);

            Console.Write("Enter person's Name: ");
            var renamedName = Console.ReadLine();
            person = await DDD.Light.Core.EventStore.Instance.GetByIdAsync<Person>(id); 
            //can also do this: 
            // person = MongoEventStore.Instance.GetById(id) as Person;
            person.NameMe(renamedName);

            var personDTO = await personReadModel.GetByIdAsync(id);
            Console.WriteLine("Person ID: " + personDTO.Id);
            Console.WriteLine("Person Name: " + personDTO.Name);
            Console.WriteLine("Person Was Renamed: " + personDTO.WasRenamed);
        }
    }
}
