using System;
using System.Collections.Generic;
using System.Linq;
using DDD.Light.Realtor.API.Query.Contract;
using DDD.Light.Realtor.API.Query.Model;
using CQRS.Light.Contracts;
using System.Threading.Tasks;

namespace DDD.Light.Realtor.API.Query
{
    public class ActiveListings : IActiveListings
    {
        private readonly IRepository<Listing> _listingsRepo;

        public ActiveListings(IRepository<Listing> listingsRepo)
        {
            _listingsRepo = listingsRepo;
        }

        public async Task<IEnumerable<Listing>> All()
        {
            return await _listingsRepo.GetAllAsync();
        }

        public async Task<Listing> ById(Guid id)
        {
            return await _listingsRepo.GetByIdAsync(id);
        }

        public async Task<IEnumerable<Listing>> UnderMillionDollars()
        {
            return (await _listingsRepo.GetAsync()).Where(l => l.Price < 1000000);
        }

    }
}
