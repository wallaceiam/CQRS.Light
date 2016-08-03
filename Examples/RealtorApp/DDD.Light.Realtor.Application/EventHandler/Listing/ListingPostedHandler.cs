using CQRS.Light.Contracts;
using DDD.Light.Realtor.API.Query.Model;
using DDD.Light.Realtor.Domain.Event.Listing;
using DDD.Light.Realtor.Domain.Event.Realtor;
using CQRS.Light.Core;
using System.Threading.Tasks;

namespace DDD.Light.Realtor.Application.EventHandler.Listing
{
    public class ListingPostedHandler : EventHandler<ListingPosted>
    {
        private readonly IRepository<API.Query.Model.Listing> _activeListings;

        public ListingPostedHandler(IEventBus eventBus, IRepository<API.Query.Model.Listing> activeListings)
            :base(eventBus)
        {
            _activeListings = activeListings;
        }

        public override async Task HandleAsync(ListingPosted @event)
        {
            var activeListing = new API.Query.Model.Listing
                {
                    Id = @event.Id,
                    NumberOfBathrooms = @event.NumberOfBathrooms,
                    NumberOfBedrooms = @event.NumberOfBedrooms,
                    YearBuilt = @event.YearBuilt,
                    Street = @event.Street,
                    City = @event.City,
                    State = @event.State,
                    Zip = @event.Zip,
                    Price = @event.Price
                };
            await _activeListings.SaveAsync(activeListing);
        }
    }
}