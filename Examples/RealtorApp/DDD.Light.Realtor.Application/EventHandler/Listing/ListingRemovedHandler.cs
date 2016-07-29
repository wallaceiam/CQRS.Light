using DDD.Light.Realtor.API.Query.Model;
using DDD.Light.Realtor.Domain.Event.Listing;
using CQRS.Light.Contracts;
using CQRS.Light.Core;
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