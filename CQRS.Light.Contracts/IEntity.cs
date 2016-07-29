using System;

namespace CQRS.Light.Contracts
{
    public interface IEntity
    {
        Guid Id { get; set; }
    }
}