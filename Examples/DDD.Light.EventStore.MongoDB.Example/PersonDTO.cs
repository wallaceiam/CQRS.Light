﻿using DDD.Light.Contracts.Repo;

namespace DDD.Light.EventStore.MongoDB.Example
{
    public class PersonDTO : Entity
    {
        public string Name { get; set; }
        public bool WasRenamed { get; set; }
    }
}