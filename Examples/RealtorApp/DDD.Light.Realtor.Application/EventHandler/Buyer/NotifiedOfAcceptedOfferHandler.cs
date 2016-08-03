using DDD.Light.Realtor.Domain.Event.Buyer;
using CQRS.Light.Core;
using System.Threading.Tasks;
using CQRS.Light.Contracts;

namespace DDD.Light.Realtor.Application.EventHandler.Buyer
{
    public class NotifiedOfAcceptedOfferHandler : EventHandler<NotifiedOfAcceptedOffer>
    {
        public NotifiedOfAcceptedOfferHandler(IEventBus eventBus)
            :base(eventBus)
        {

        }
        public override Task HandleAsync(NotifiedOfAcceptedOffer @event)
        {
            throw new System.NotImplementedException();
        }
    }
}