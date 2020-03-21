using System;
using System.Threading;
using System.Threading.Tasks;
using Novibet.IpStack.Business.Models;

namespace Novibet.IpStack.Business.Services
{
    public interface IIpStackService
    {
        Task<Ip> GetIpCachedAsync(string ipAddress);
        Task<Job> BatchUpdateAsync(string[] ipAddressess);
        Task<Job> JobStatusAsync(Guid jobId);
        Task JobUpdateAsync(CancellationToken token, Job pending);
        Task RecoverJobs(CancellationToken cancellationToken);
    }
}
