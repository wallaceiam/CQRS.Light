using System;
using CQRS.Light.Contracts;

namespace DDD.Light.Realtor.API.Query.Model
{
    public abstract class Entity : IEntity
    {
        public Guid Id { get; set; }       
    }
}