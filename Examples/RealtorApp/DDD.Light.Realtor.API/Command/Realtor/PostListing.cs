using System;

namespace DDD.Light.Realtor.API.Command.Realtor
{
    public class PostListing
    {
        public Guid ListingId { get; private set; }
        public int NumberOfBathrooms { get; private set; }
        public int NumberOfBedrooms { get; private set; }
        public int YearBuilt { get; private set; }
        public string Street { get; private set; }
        public string City { get; private set; }
        public string State { get; private set; }
        public string Zip { get; private set; }
        public decimal Price { get; private set; }

        public PostListing(
            Guid listingId,
            int numberOfBathrooms,
            int numberOfBedrooms,
            int yearBuilt,
            string street,
            string city,
            string state,
            string zip,
            decimal price
        )
        {
            ListingId = listingId;
            NumberOfBathrooms = numberOfBathrooms;
            NumberOfBedrooms = numberOfBedrooms;
            YearBuilt = yearBuilt;
            Street = street;
            City = city;
            State = state;
            Zip = zip;
            Price = price;
        }
    }
}