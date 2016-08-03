using System;
using System.Collections.Generic;
using CQRS.Light.Core;
using DDD.Light.Realtor.Domain.Event.Listing;
using CQRS.Light.Contracts;

namespace DDD.Light.Realtor.Domain.Model.Listing
{
    // aggregate root
    public class Listing : AggregateRoot
    {
        private Location _location;
        private Description _description;
        private decimal _price;
        private bool _posted;
        private IEnumerable<Guid> _offers;

        private Listing()
            :base(null)
        {
        }

        public Listing(IAggregateBus aggregateBus, Guid id, Location location, Description description, decimal price)
            : base(aggregateBus, id)
        {
            _location = location;
            _description = description;
            _price = price;
            _posted = false;

            PublishAndApplyEventAsync(new ListingCreated(id, location, description, price)).ConfigureAwait(true);
        }

        // API
        public void Remove()
        {
            PublishAndApplyEventAsync(new ListingRemoved(Id)).ConfigureAwait(true);
        }
        
        public void Post()
        {
            PublishAndApplyEventAsync(new ListingPosted(
                Id, 
                _description.NumberOfBathrooms, 
                _description.NumberOfBedrooms, 
                _description.YearBuilt,
                _location.Street,
                _location.City,
                _location.State,
                _location.Zip,
                _price)
            ).ConfigureAwait(true);
        }

        // Apply Domain Events to rebuild aggregate
        private void ApplyEvent(ListingRemoved @event)
        {
            _posted = false;
        }
        
        private void ApplyEvent(ListingCreated @event)
        {
            Id = @event.Id;
            _location = @event.Location;
            _description = @event.Description;
            _price = @event.Price;
            _posted = false;
        }
    }
}