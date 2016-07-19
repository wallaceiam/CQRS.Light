using DDD.Light.Realtor.Domain.Event.Offer;
using DDD.Light.CQRS;
using System.Threading.Tasks;

namespace DDD.Light.Realtor.Application.EventHandler.Offer
{
    public class OfferMadeHandler : EventHandler<OfferMade>
    {
        public override Task HandleAsync(OfferMade @event)
        {
            throw new System.NotImplementedException();
        }
    }
}