using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using AttributeRouting.Web.Http;
using CQRS.Light.Contracts;
using DDD.Light.Realtor.API.Command.Realtor;
using DDD.Light.Realtor.API.Query.Contract;
using DDD.Light.Realtor.REST.API.Resources;
using System.Threading.Tasks;

namespace DDD.Light.Realtor.REST.API.Controllers
{
    public class RealtorController : ApiController
    {
        private readonly ICommandBus _commandBus;
        private readonly IActiveListings _activeListings;

        public RealtorController(ICommandBus commandBus, IActiveListings activeListings)
        {
            _commandBus = commandBus;
            _activeListings = activeListings;
        }

        [POST("api/realtor/listings")]
        public async Task<HttpResponseMessage> PostListing([FromBody]RealtorListing realtorListing)
        {
            var listingId = Guid.NewGuid();
            var postListing = new PostListing(
                listingId,
                realtorListing.NumberOfBathrooms,
                realtorListing.NumberOfBedrooms,
                realtorListing.YearBuilt,
                realtorListing.Street,
                realtorListing.City,
                realtorListing.State,
                realtorListing.Zip,
                realtorListing.Price);
            try
            {
                await _commandBus.DispatchAsync(postListing);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
            return Request.CreateResponse(HttpStatusCode.Created, postListing);
        }
        
        [GET("api/realtor/listings")]
        public HttpResponseMessage GetListings()
        {
            try
            {
                 var listings = _activeListings.All();
                 return Request.CreateResponse(HttpStatusCode.OK, listings);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
            
        }
    }
}
