using System;
using DDD.Light.Contracts.Repo;

namespace DDD.Light.Realtor.API.Query.Model
{
    public abstract class Entity : IEntity
    {
        public Guid Id { get; set; }       
    }
}