using DDD.Light.Realtor.Domain.Event.Buyer;
using DDD.Light.CQRS;
using System.Threading.Tasks;

namespace DDD.Light.Realtor.Application.EventHandler.Buyer
{
    public class NotifiedOfAcceptedOfferHandler : EventHandler<NotifiedOfAcceptedOffer>
    {
        public override Task HandleAsync(NotifiedOfAcceptedOffer @event)
        {
            throw new System.NotImplementedException();
        }
    }
}