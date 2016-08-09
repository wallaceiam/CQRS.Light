using System;
using CQRS.Light.Core;
using CQRS.Light.Contracts;
using CQRS.Light.Repository.InMemory;
using CQRS.Light.Repository.MongoDB;
using System.Threading.Tasks;

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

            CQRS.Light.Core.EventStore.Instance.Configure(aggregateEventRepo, new JsonSerializationStrategy());
            EventBus.Instance.Configure(CQRS.Light.Core.EventStore.Instance, new JsonSerializationStrategy(), false);
            
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
            var person = new Person(AggregateBus.Instance, id);
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
            var person = new Person(AggregateBus.Instance, id);
            person.NameMe(name);

            Console.Write("Enter person's Name: ");
            var renamedName = Console.ReadLine();
            person = await CQRS.Light.Core.EventStore.Instance.GetByIdAsync<Person>(id); 
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
