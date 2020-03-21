using System;
using System.Threading;
using System.Threading.Tasks;
using Novibet.IpStack.Business.Models;

namespace Novibet.IpStack.Business.Services
{
    public interface IBackgroundTaskQueue
    {
        void QueueBackgroundWorkItem(Job workItem);

        Task<Job> DequeueAsync(CancellationToken cancellationToken);
    }
}
