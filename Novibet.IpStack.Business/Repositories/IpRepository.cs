using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Logging;
using Novibet.IpStack.Abstractions;
using Novibet.IpStack.Business.Data;
using Novibet.IpStack.Business.Extensions;
using Novibet.IpStack.Business.Models;

namespace Novibet.IpStack.Business.Repositories
{
    public class IpRepository : IIpRepository
    {
        private readonly IpStackContext _dbContext;

        public IpRepository(IpStackContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Ip> GetIpAsync(string ipAddress)
        {
            var ip = await _dbContext.IpAddressess.FirstOrDefaultAsync(z => z.IpAddress == ipAddress);

            return ip;
        }

        public async Task<Ip> AddIpAsync(IPDetails clientIpDetail, string ipAddress)
        {
            var ipDetail = await IpDetails(clientIpDetail, ipAddress);

            await _dbContext.IpAddressess.AddAsync(ipDetail);

            await _dbContext.SaveChangesAsync();

            return ipDetail;
        }

        public async Task<Ip> UpdateIpAsync(IPDetails clientIpDetail, string ipAddress)
        {
            var ipDetail = await IpDetails(clientIpDetail, ipAddress);

            var currentIp = await _dbContext.IpAddressess.FirstOrDefaultAsync(x => x.IpAddress == ipAddress);

            currentIp.Longitude = ipDetail.Longitude;
            currentIp.Latitude = ipDetail.Latitude;
            currentIp.CityId = ipDetail.CityId;
            currentIp.CountryId = ipDetail.CountryId;
            currentIp.ContinentId = ipDetail.ContinentId;

            await _dbContext.SaveChangesAsync();

            return ipDetail;
        }

        private async Task<Ip> IpDetails(IPDetails clientIpDetail, string ipAddress)
        {
            var continent = await GetOrAddContinent(clientIpDetail);

            var country = await GetOrAddCountry(clientIpDetail);

            var city = await GetOrAddCity(clientIpDetail);

            var ip = clientIpDetail.ToIp(ipAddress, city.Id, country.Id, continent.Id);
            return ip;
        }

        public async Task<City> GetOrAddCity(IPDetails ipDetails)
        {
            var city = await _dbContext.Cities.FirstOrDefaultAsync(z => z.Name == ipDetails.City);
            if (city != null)
            {
                return city;
            }

            var cityInserted = new City { Name = ipDetails.City };

            await _dbContext.Cities.AddAsync(cityInserted);
            await _dbContext.SaveChangesAsync();

            return cityInserted;
        }

        public async Task<Country> GetOrAddCountry(IPDetails ipDetails)
        {
            var country = await _dbContext.Countries.FirstOrDefaultAsync(z => z.Name == ipDetails.Country);
            if (country != null)
            {
                return country;
            }

            var countryInserted = new Country { Name = ipDetails.Country };

            await _dbContext.Countries.AddAsync(countryInserted);
            await _dbContext.SaveChangesAsync();

            return countryInserted;
        }

        public async Task<Continent> GetOrAddContinent(IPDetails ipDetails)
        {
            var continent = await _dbContext.Continents.FirstOrDefaultAsync(z => z.Name == ipDetails.Continent);
            if (continent != null)
            {
                return continent;
            }

            var continentInserted = new Continent { Name = ipDetails.Continent };

            await _dbContext.Continents.AddAsync(continentInserted);
            await _dbContext.SaveChangesAsync();

            return continentInserted;
        }
    }
}
