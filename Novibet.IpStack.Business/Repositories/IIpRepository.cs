using System.Threading.Tasks;
using Novibet.IpStack.Abstractions;
using Novibet.IpStack.Business.Models;

namespace Novibet.IpStack.Business.Repositories
{
    public interface IIpRepository
    {
        Task<Ip> GetIpAsync(string ipAddress);
        Task<Ip> AddIpAsync(IPDetails clientIpDetail, string ipAddress);
        Task<City> GetOrAddCity(IPDetails ipDetails);
        Task<Country> GetOrAddCountry(IPDetails ipDetails);
        Task<Continent> GetOrAddContinent(IPDetails ipDetails);
        Task<Ip> UpdateIpAsync(IPDetails clientIpDetail, string ipAddress);
    }
}