using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Novibet.IpStack.Business.Models;
using Novibet.IpStack.Business.Repositories;

namespace Novibet.IpStack.Business.Services
{
    public class JobProcessingService : BackgroundService
    {
        private readonly ILogger<JobProcessingService> _logger;
        private readonly IJobRepository _jobRepository;
        private readonly IIpStackService _ipStackService;

        public JobProcessingService(
            IBackgroundTaskQueue taskQueue,
            ILogger<JobProcessingService> logger,
            IJobRepository jobRepository,
            IIpStackService ipStackService)
        {
            TaskQueue = taskQueue;
            _logger = logger;
            _jobRepository = jobRepository;
            _ipStackService = ipStackService;
        }

        public IBackgroundTaskQueue TaskQueue { get; }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation($"Queued Hosted Service is running.");

            // first try to recover any pending jobs from previous execution.
            await RecoverJobs(stoppingToken);

            await BackgroundProcessing(stoppingToken);
        }

        /// <summary>
        /// Deque task from queue for processing a job for update.
        /// </summary>
        /// <param name="stoppingToken"></param>
        /// <returns></returns>
        private async Task BackgroundProcessing(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var workItem = await TaskQueue.DequeueAsync(stoppingToken);

                try
                {
                    await workItem(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        "Error occurred executing {WorkItem}.", nameof(workItem));
                }
            }
        }


        public async Task RecoverJobs(CancellationToken cancellationToken)
        {
            var pendingJobs = await _jobRepository.GetByJobStatus(JobStatus.InProgress);

            if (pendingJobs == null || !pendingJobs.Any())
            {
                return;
            }

            foreach (var pending in pendingJobs)
            {
                TaskQueue.QueueBackgroundWorkItem(
                    (token) => _ipStackService.JobUpdateAsync(token, pending));
            }
        }

        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Queued Hosted Service is stopping.");

            await base.StopAsync(stoppingToken);
        }
    }

}
