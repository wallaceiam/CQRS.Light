using System;
using System.Collections.Generic;
using DDD.Light.CQRS;
using DDD.Light.Realtor.Domain.Event.Buyer;

namespace DDD.Light.Realtor.Domain.Model.Buyer
{
    public class Buyer : AggregateRoot
    {
        private List<Property> _properties;
        private List<Guid> _offerIds;
        private Guid _prospectId;

        private Buyer()
        {
        }

        public Buyer(Guid id, Guid prospectId) : base(id)
        {
            _prospectId = prospectId;
            PublishAndApplyEventAsync(new ProspectPromotedToBuyer(id, prospectId)).ConfigureAwait(true);
        }


        // API
        public void TakeOwnershipOf(Guid listingId)
        {
            // todo implement
            PublishAndApplyEventAsync(new TookOwnershipOfListing()).ConfigureAwait(true);
        }

        public void NotifyOfAcceptedOffer(Guid offerId)
        {
            //todo implement
            PublishAndApplyEventAsync(new NotifiedOfAcceptedOffer()).ConfigureAwait(true);
        }

        public void NotifyOfRejectedOffer(Offer.Offer offer)
        {
            throw new NotImplementedException();
        }

        public void PurchaseProperty()
        {
            throw new NotImplementedException();
        }

        public virtual void MakeAnOffer(Guid listingId, decimal price)
        {
            throw new NotImplementedException();
        }


        // Apply Events
        private void ApplyEvent(TookOwnershipOfListing @event)
        {
            throw new NotImplementedException();
        }
        
        private void ApplyEvent(NotifiedOfAcceptedOffer @event)
        {
            throw new NotImplementedException();
        }







    }
}