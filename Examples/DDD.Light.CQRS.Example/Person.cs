using DDD.Light.CQRS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDD.Light.Messaging.Example
{
    public class Person : AggregateRoot
    {
        protected string name;
        protected string location;

        protected Person(Guid id) : base(id)
        {
        }

        
    }
}
