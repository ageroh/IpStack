using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Novibet.IpStack.Business.Models;

namespace Novibet.IpStack.Business.Services
{
    public class BackgroundTaskQueue : IBackgroundTaskQueue
    {
        private static readonly ConcurrentQueue<Job> _workItems = new ConcurrentQueue<Job>();
        private SemaphoreSlim _signal = new SemaphoreSlim(0);

        public void QueueBackgroundWorkItem(Job job)
        {
            if (job == null)
            {
                throw new ArgumentNullException(nameof(job));
            }

            _workItems.Enqueue(job);
            _signal.Release();
        }

        public async Task<Job> DequeueAsync(CancellationToken cancellationToken)
        {
            await _signal.WaitAsync(cancellationToken);
            _workItems.TryDequeue(out var workItem);

            return workItem;
        }
    }
}
