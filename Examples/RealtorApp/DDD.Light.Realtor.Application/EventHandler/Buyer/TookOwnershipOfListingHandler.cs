using DDD.Light.Realtor.Domain.Event.Buyer;
using CQRS.Light.Core;
using System.Threading.Tasks;

namespace DDD.Light.Realtor.Application.EventHandler.Buyer
{
    public class TookOwnershipOfListingHandler : EventHandler<TookOwnershipOfListing>
    {
        public override Task HandleAsync(TookOwnershipOfListing @event)
        {
            throw new System.NotImplementedException();
        }
    }
}