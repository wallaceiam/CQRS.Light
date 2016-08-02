using System;
using CQRS.Light.Contracts;
using DDD.Light.Realtor.API.Command.Realtor;
using DDD.Light.Realtor.Domain.Model.Listing;
using CQRS.Light.Core;
using System.Threading.Tasks;

namespace DDD.Light.Realtor.Application.CommandHandler.Realtor
{
    public class PostListingHandler : CommandHandler<PostListing>
    {
        private readonly IEventStore _eventStore;

        public PostListingHandler(ICommandBus commandBus, IEventStore eventStore)
            :base(commandBus)
        {
            _eventStore = eventStore;
        }

        public override async Task HandleAsync(PostListing command)
        {
            var listing = new Listing(
                command.ListingId,
                new Location(command.Street, command.City, command.State, command.Zip),
                new Description(command.NumberOfBathrooms, command.NumberOfBedrooms, command.YearBuilt),                
                command.Price
            );

            var realtor = await _eventStore.GetByIdAsync<Domain.Model.Realtor.Realtor>(Guid.Parse("10000000-0000-0000-0000-000000000000"));
            realtor.PostListing(listing.Id);
        }
    }
}