using System;
using System.Collections.Generic;
using CQRS.Light.Core;
using DDD.Light.Realtor.Domain.Event.Offer;
using CQRS.Light.Contracts;

namespace DDD.Light.Realtor.Domain.Model.Prospect
{
    // aggregate root
    public class Prospect : AggregateRoot
    {
        private List<Guid> _offerIds;

        private Prospect()
            :base(null)
        {            
        }

        public Prospect(IAggregateBus aggregateBus, Guid id)
            : base(aggregateBus, id)
        {
            
        }

        // API
        public void MakeAnOffer(Guid offerId)
        {
            PublishAndApplyEventAsync(new OfferMade(offerId)).ConfigureAwait(true);
        }

        // Apply Events
        private void ApplyEvent(OfferMade @event)
        {
            if (_offerIds == null)
                _offerIds = new List<Guid>();
            _offerIds.Add(@event.OfferId);
        }

    }
}