using System;
using CQRS.Light.Core;
using CQRS.Light.Contracts;

namespace DDD.Light.EventStore.MongoDB.Example
{
    public class Person : AggregateRoot
    {
        private string _name;

        private Person()
            :base(null)
        {
            
        }

        public Person(IAggregateBus aggregateBus, Guid id)
            :base(aggregateBus, id)
        {
            // cleanest way to call publish
            PublishAndApplyEventAsync(new PersonCreated(Id)).ConfigureAwait(true);
//            EventBus.Instance.Publish(GetType(), Id, new PersonCreated(Id));
        }


        // Aggregate API, all public

        public void NameMe(string name)
        {
            if (string.IsNullOrEmpty(_name))
            {
                // can call publish this way too, generic way
                PublishAndApplyEventAsync(new PersonNamed(Id, name)).ConfigureAwait(true);
//                EventBus.Instance.Publish<Person, PersonNamed>(Id, new PersonNamed(Id, name));
            }
            else
                // yes, this will work too, non generic way
                PublishAndApplyEventAsync(new PersonRenamed(Id, name)).ConfigureAwait(true);
//                EventBus.Instance.Publish(typeof(Person), Id, new PersonRenamed(Id, name));
        }



        // Apply Events (For rebuilding aggregate) all private

        private void ApplyEvent(PersonCreated personCreated)
        {
            Id = personCreated.Id;
        }

        private void ApplyEvent(PersonNamed personNamed)
        {
            _name = personNamed.Name;
        }

        private void ApplyEvent(PersonRenamed personNamed)
        {
            _name = personNamed.Name;
        }

    }
}