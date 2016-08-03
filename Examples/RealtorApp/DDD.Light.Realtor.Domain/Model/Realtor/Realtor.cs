using System;
using System.Collections.Generic;
using DDD.Light.Realtor.Domain.Event.Realtor;
using CQRS.Light.Core;
using CQRS.Light.Contracts;

namespace DDD.Light.Realtor.Domain.Model.Realtor
{
    // aggregate root
    public class Realtor : AggregateRoot
    {
        private List<Guid> _offerIds; 
        private List<Guid> _postedListingIds; 
        private List<Guid> _newListingIds; 

        private Realtor()
            :base(null)
        {  
        }

        public Realtor(IAggregateBus aggregateBus, Guid id)
            : base(aggregateBus, id)
        {
//            Publish<Realtor, RealtorWasSetUp>(new RealtorWasSetUp(id));
            PublishAndApplyEventAsync(new RealtorWasSetUp(id)).ConfigureAwait(true);
        }

        // API
        public void NotifyThatOfferWasMade(Guid offerId)
        {
            PublishAndApplyEventAsync(new RealtorNotifiedThatOfferWasMade(offerId)).ConfigureAwait(true);
        }

        public void AddNewListing(Guid listingId)
        {
            PublishAndApplyEventAsync(new RealtorAddedNewListing(listingId)).ConfigureAwait(true);
        }

        public void PostListing(Guid listingId)
        {
            PublishAndApplyEventAsync(new RealtorPostedListing(listingId)).ConfigureAwait(true);
        }

        // Apply Events
        private void ApplyEvent(RealtorWasSetUp @event)
        {
            Id = @event.RealtorId;
        }

        private void ApplyEvent(RealtorAddedNewListing @event)
        {
            if (_newListingIds == null)
                _newListingIds = new List<Guid>();
            _newListingIds.Add(@event.ListingId);
        }
        
        private void ApplyEvent(RealtorPostedListing @event)
        {
            _newListingIds.Remove(@event.ListingId);
            if (_postedListingIds == null)
                _postedListingIds = new List<Guid>();
            _postedListingIds.Add(@event.ListingId);
        }

        private void ApplyEvent(RealtorNotifiedThatOfferWasMade @event)
        {
            if (_offerIds == null)
                _offerIds = new List<Guid>();
            _offerIds.Add(@event.OfferId);
        }
    }
}