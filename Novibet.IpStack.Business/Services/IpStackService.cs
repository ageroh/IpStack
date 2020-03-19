using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Novibet.IpStack.Abstractions;
using Novibet.IpStack.Business.Data;
using Novibet.IpStack.Business.Extensions;
using Novibet.IpStack.Business.Models;
using static Novibet.IpStack.Business.Constants;

namespace Novibet.IpStack.Business.Services
{
    public interface IIpStackService
    {
        Task<Ip> GetIpCachedAsync(string ipAddress);
    }


    /// <summary>
    /// Provide caching, validation for Ip requests
    /// </summary>
    public class IpStackService : IIpStackService
    {
        private readonly ILogger<IpStackService> _logger;
        private readonly IIPInfoProvider _ipInfoProvider;
        private readonly IMemoryCache _memoryCache;
        private readonly IpStackContext _dbContext;
        private static readonly SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1);

        public IpStackService(ILogger<IpStackService> logger, IIPInfoProvider iPInfoProvider, IMemoryCache memoryCache, IpStackContext dbContext)
        {
            _logger = logger;
            _ipInfoProvider = iPInfoProvider;
            _memoryCache = memoryCache;
            _dbContext = dbContext;
        }


        public async Task<Ip> GetIpCachedAsync(string ipAddress)
        {

            var dynamicCacheKey = $"{CacheKeys.IpKey}:{ipAddress}";
            if (_memoryCache.TryGetValue(dynamicCacheKey, out Ip ipDetail))
            {
                return ipDetail;
            }

            await semaphoreSlim.WaitAsync();
            try
            {
                if (_memoryCache.TryGetValue(dynamicCacheKey, out ipDetail))
                {
                    return ipDetail;
                }

                ipDetail = await GetIpInternalAsync(ipAddress);
                if (ipDetail == null)
                {
                    return null;
                }

                var cacheExpirationOptions = new MemoryCacheEntryOptions().
                    SetAbsoluteExpiration(TimeSpan.FromSeconds(Constants.DefaultCacheExpirationSeconds)).
                    SetPriority(CacheItemPriority.Normal);

                _memoryCache.Set(dynamicCacheKey, ipDetail, cacheExpirationOptions);

                return ipDetail;
            }
            catch (Exception ex)
            {
                throw;
            }
            finally
            {
                semaphoreSlim.Release();
            }
        }

        private async Task<Ip> GetIpInternalAsync(string ipAddress)
        {
            var ipDetail = await _dbContext.IpAddressess.FirstOrDefaultAsync(z => z.IpAddress == ipAddress);
            if (ipDetail != null)
            {
                return ipDetail;
            }

            var clientIpDetail = await _ipInfoProvider.GetDetailsAsync(ipAddress);
            if (clientIpDetail != null)
            {
                GetOrAddContinent(clientIpDetail, out var continentId);

                GetOrAddCountry(clientIpDetail, out var countryId);

                GetOrAddCity(clientIpDetail, out var cityId);

                ipDetail = clientIpDetail.ToIp(ipAddress, cityId, countryId, continentId);

                ipDetail = _dbContext.IpAddressess.Add(ipDetail).Entity;

                await _dbContext.SaveChangesAsync();

                return ipDetail;
            }

            return null;
        }

        private void GetOrAddCity(IPDetails ipDetails, out int cityId)
        {
            var city = _dbContext.Cities.FirstOrDefault(z => z.Name == ipDetails.City);
            if (city != null)
            {
                cityId = city.Id;
                return;
            }

            city = _dbContext.Cities.Add(new City { Name = ipDetails.City }).Entity;
            cityId = city.Id;

            _dbContext.SaveChanges();
        }

        private void GetOrAddCountry(IPDetails ipDetails, out int countryId)
        {
            var country = _dbContext.Countries.FirstOrDefault(z => z.Name == ipDetails.Country);
            if (country != null)
            {
                countryId = country.Id;
                return;
            }

            country = _dbContext.Countries.Add(new Country { Name = ipDetails.Country }).Entity;
            countryId = country.Id;

            _dbContext.SaveChanges();
        }

        private void GetOrAddContinent(IPDetails ipDetails, out int continentId)
        {
            var continent = _dbContext.Continents.FirstOrDefault(z => z.Name == ipDetails.Continent);
            if (continent != null)
            {
                continentId = continent.Id;
                return;
            }

            continent = _dbContext.Continents.Add(new Continent { Name = ipDetails.Continent }).Entity;
            continentId = continent.Id;

            _dbContext.SaveChanges();
        }
    }
}