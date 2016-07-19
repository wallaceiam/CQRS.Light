using System;

namespace DDD.Light.Contracts.Repo
{
    public abstract class Entity : IEntity
    {
        public Guid Id { get; set; }       

        protected Entity()
        {
            
        }

        protected Entity(Guid id)
        {
            Id = id;
        }
    }
}