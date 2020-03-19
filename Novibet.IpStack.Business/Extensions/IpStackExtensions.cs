using Novibet.IpStack.Abstractions;
using Novibet.IpStack.Business.Models;

namespace Novibet.IpStack.Business.Extensions
{
    public static class IpStackExtensions
    {
        public static Ip ToIp(this IPDetails ipDetails, string ipAddress, int cityId, int countryId, int continentId)
        {
            return new Ip
            {
                CityId = cityId,
                CountryId = countryId,
                ContinentId = continentId,
                Latitude = ipDetails.Latitude,
                Longitude = ipDetails.Longitude,
                IpAddress = ipAddress,
            };
        }

        public static Country ToCountry(this IPDetails ipDetails)
        {
            return new Country
            {
                Name = ipDetails.Country
            };
        }

        public static City ToCity(this IPDetails ipDetails)
        {
            return new City
            {
                Name = ipDetails.City
            };
        }

        public static Continent ToContinent(this IPDetails ipDetails)
        {
            return new Continent
            {
                Name = ipDetails.Continent
            };
        }

    }
}
