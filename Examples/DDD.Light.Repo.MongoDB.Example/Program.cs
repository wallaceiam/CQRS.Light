using DDD.Light.Repository.MongoDB;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace DDD.Light.Repo.MongoDB.Example
{
    class Program
    {
        private static void Main(string[] args)
        {
            RunAsync().ConfigureAwait(false);
        }

        private static async Task RunAsync()
        { 
            // before running example, make sure MongoDB is running by executing %MONGO_INSTALL_DIR%\bin\mongod.exe

            // connection string to your MongoDB
            const string connectionString = "mongodb://localhost";
            // name of the database
            const string databaseName = "MyCompanyDB";
            // name of the collection (loosely equivalet to a TABLE in releational database)
            const string collectionName = "people";
            // instantiate a repository
            // in real application, this should be setup as a singleton (can be done through StructureMap, Unity or another DI container)
            var personRepository = new MongoRepository<Person>(connectionString, databaseName, collectionName);

            Console.Write("Please enter person's first name: ");
            var firstName = Console.ReadLine();

            Console.Write("Please enter person's last name: ");
            var lastName = Console.ReadLine();

            var id = Guid.NewGuid();

            var newPerson = new Person
            {
                Id = id,
                FirstName = firstName,
                LastName = lastName
            };

            // save to repository
            await personRepository.SaveAsync(newPerson);

            Console.WriteLine("person saved");

            // get by id
            var personFoundById = await personRepository.GetByIdAsync(id);
            Console.WriteLine("get by id " + id + " result: " + personFoundById.FirstName + " " + personFoundById.LastName);

            // query repository
            var peopleFoundByQuery = (await personRepository.GetAsync()).Where(p => p.FirstName == newPerson.FirstName);
            Console.WriteLine("get by matching FirstName == " + newPerson.FirstName);
            peopleFoundByQuery.ToList().ForEach(p => Console.WriteLine(" match: " + p.FirstName + " " + p.LastName));
            

            Console.ReadLine();

        }
    }
}
