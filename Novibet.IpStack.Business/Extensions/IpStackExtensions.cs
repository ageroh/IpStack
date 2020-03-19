using Novibet.IpStack.Abstractions;
using Novibet.IpStack.Business.Models;

namespace Novibet.IpStack.Business.Extensions
{
    public static class IpStackExtensions
    {
        public static Ip ToIp(this IPDetails ipDetails, string ipAddress)
        {
            return new Ip
            {
                City = new City { Name = ipDetails.City },
                Continent = new Continent { Name = ipDetails.Continent },
                Country = new Country { Name = ipDetails.Country },
                Latitude = ipDetails.Latitude,
                Longitude = ipDetails.Longitude,
                IpAddress = ipAddress,
            };
        }
    }
}
