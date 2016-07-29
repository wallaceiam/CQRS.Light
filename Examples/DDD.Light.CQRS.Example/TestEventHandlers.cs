using System;
using CQRS.Light.Contracts;
using System.Threading.Tasks;

namespace DDD.Light.Messaging.Example
{
    public class PersonArrivedEventHandler : IEventHandler<PersonArrivedEvent>
    {
        public Task HandleAsync(PersonArrivedEvent personArrivedEvent)
        {
            Console.WriteLine("PersonArrivedEventHandler ::: Handling PersonArrivedEvent... Name: "
                + personArrivedEvent.Name + " Location: " + personArrivedEvent.Location);
            return Task.FromResult<object>(null);
        }
    }

    public class PersonLeftEventHandler : IEventHandler<PersonLeftEvent>
    {
        public Task HandleAsync(PersonLeftEvent personLeftEvent)
        {
            Console.WriteLine("PersonLeftEventHandler ::: Handling PersonLeftEvent... Name: " 
                + personLeftEvent.Name + " Location: " + personLeftEvent.Location);
            return Task.FromResult<object>(null);
        }
    }

    public class PersonLeftAndSpokeEventHandler : IEventHandler<PersonLeftEvent>
    {
        private readonly string _word;

        public PersonLeftAndSpokeEventHandler(string word)
        {
            _word = word;
        }

        public Task HandleAsync(PersonLeftEvent personLeftEvent)
        {
            Console.WriteLine("PersonLeftAndSpokeEventHandler ::: Handling PersonLeftEvent... Name: " 
                + personLeftEvent.Name + " Location: " + personLeftEvent.Location + " and said the word: " + _word);
            return Task.FromResult<object>(null);
        }
    }

}
