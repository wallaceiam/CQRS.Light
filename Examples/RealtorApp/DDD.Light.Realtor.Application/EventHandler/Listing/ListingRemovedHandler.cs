using DDD.Light.Realtor.API.Query.Model;
using DDD.Light.Realtor.Domain.Event.Listing;
using DDD.Light.Contracts.Repo;
using DDD.Light.CQRS;
using System.Threading.Tasks;

namespace DDD.Light.Realtor.Application.EventHandler.Listing
{
    public class ListingRemovedHandler : EventHandler<ListingRemoved>
    {
        private readonly IRepository<API.Query.Model.Listing> _activeListings;

        public ListingRemovedHandler(IRepository<API.Query.Model.Listing> activeListings)
        {
            _activeListings = activeListings;
        }

        public override async Task HandleAsync(ListingRemoved @event)
        {
            await _activeListings.DeleteAsync(@event.ListingId);
        }
    }
}