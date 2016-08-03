using DDD.Light.Realtor.Domain.Event.Offer;
using CQRS.Light.Core;
using System.Threading.Tasks;
using CQRS.Light.Contracts;

namespace DDD.Light.Realtor.Application.EventHandler.Offer
{
    public class OfferMadeHandler : EventHandler<OfferMade>
    {
        public OfferMadeHandler(IEventBus eventBus)
            : base(eventBus)
        {

        }
        public override Task HandleAsync(OfferMade @event)
        {
            throw new System.NotImplementedException();
        }
    }
}