using System;

namespace DDD.Light.Contracts.Repo
{
    public interface IEntity
    {
        Guid Id { get; set; }
    }
}