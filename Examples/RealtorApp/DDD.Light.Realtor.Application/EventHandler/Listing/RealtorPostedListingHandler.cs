using DDD.Light.Contracts.EventStore;
using DDD.Light.Realtor.Domain.Event.Realtor;
using DDD.Light.CQRS;
using System.Threading.Tasks;

namespace DDD.Light.Realtor.Application.EventHandler.Listing
{
    public class RealtorPostedListingHandler : EventHandler<RealtorPostedListing>
    {
        private readonly IEventStore _eventStore;

        public RealtorPostedListingHandler(IEventStore eventStore)
        {
            _eventStore = eventStore;
        }

        public override async Task HandleAsync(RealtorPostedListing @event)
        {
            var listing = await _eventStore.GetByIdAsync<Domain.Model.Listing.Listing>(@event.ListingId);
            listing.Post();
        }
    }
}