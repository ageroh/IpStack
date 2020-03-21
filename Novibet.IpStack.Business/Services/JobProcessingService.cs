using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Novibet.IpStack.Business.Services
{
    public class JobProcessingService : BackgroundService
    {
        private readonly ILogger<JobProcessingService> _logger;
        public IServiceProvider Services { get; }

        public JobProcessingService(
            IBackgroundTaskQueue taskQueue,
            ILogger<JobProcessingService> logger,
            IServiceProvider services)
        {
            TaskQueue = taskQueue;
            _logger = logger;
            Services = services;
        }

        public IBackgroundTaskQueue TaskQueue { get; }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation($"Queued Hosted Service is running.");

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
                    using (var scope = Services.CreateScope())
                    {
                        var ipStackService =
                            scope.ServiceProvider
                                .GetRequiredService<IIpStackService>();

                        await ipStackService.JobUpdateAsync(stoppingToken, workItem);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        "Error occurred executing {WorkItem}.", nameof(workItem));
                }
            }
        }

        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Queued Hosted Service is stopping.");

            await base.StopAsync(stoppingToken);
        }
    }
}
