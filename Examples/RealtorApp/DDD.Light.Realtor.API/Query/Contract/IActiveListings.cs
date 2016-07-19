using System;
using System.Collections.Generic;
using DDD.Light.Realtor.API.Query.Model;
using System.Threading.Tasks;

namespace DDD.Light.Realtor.API.Query.Contract
{
    public interface IActiveListings 
    {
        Task<IEnumerable<Listing>> All();
        Task<Listing> ById(Guid id);
        Task<IEnumerable<Listing>> UnderMillionDollars();
    }
}