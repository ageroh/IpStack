using System.Threading.Tasks;

namespace Novibet.IpStack.Abstractions
{
    public interface IIPInfoProvider
    {
        IPDetails GetDetails(string ip);

        Task<IPDetails> GetDetailsAsync(string ip);
    }
}
