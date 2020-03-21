using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Novibet.IpStack.Abstractions;
using Novibet.IpStack.Business.Models;
using Novibet.IpStack.Business.Repositories;
using static Novibet.IpStack.Business.Constants;

namespace Novibet.IpStack.Business.Services
{
    /// <summary>
    /// Provide caching, validation, and all services for Ip Stack 
    /// </summary>
    public class IpStackService : IIpStackService
    {
        private readonly ILogger<IpStackService> _logger;
        private readonly IIPInfoProvider _ipInfoProvider;
        private readonly IMemoryCache _memoryCache;
        private readonly IIpRepository _ipRepository;
        private readonly IJobRepository _jobRepository;
        private readonly IBackgroundTaskQueue _backgroundTaskQueue;
        private static readonly SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1);
        private readonly int MaxBatchSize;

        public IpStackService(
            ILogger<IpStackService> logger,
            IIPInfoProvider iPInfoProvider,
            IMemoryCache memoryCache,
            IIpRepository ipRepository,
            IJobRepository jobRepository,
            IBackgroundTaskQueue backgroundTaskQueue,
            IConfiguration configuration)
        {
            _logger = logger;
            _ipInfoProvider = iPInfoProvider;
            _memoryCache = memoryCache;
            _ipRepository = ipRepository;
            _jobRepository = jobRepository;
            _backgroundTaskQueue = backgroundTaskQueue;
            int.TryParse(configuration.GetSection("MaxBatchSize").Value, out MaxBatchSize);
        }

        public async Task<Job> BatchUpdateAsync(string[] ipAddressess)
        {
            if (ipAddressess == null || !ipAddressess.Any())
            {
                throw new ArgumentNullException(nameof(ipAddressess));
            }

            var job = await _jobRepository.CreateJobAsync(ipAddressess);

            _backgroundTaskQueue.QueueBackgroundWorkItem(job);

            return job;
        }

        public async Task<Job> JobStatusAsync(Guid jobId)
        {
            if (jobId == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(jobId));
            }

            return await _jobRepository.GetAsync(jobId);
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

                ipDetail = await GetIpAsync(ipAddress);
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

        public async Task<(Guid, JobStatus)> AddOrUpdateIpAsync(JobDetail jobDetail)
        {
            try
            {
                var clientIpDetail = await _ipInfoProvider.GetDetailsAsync(jobDetail.IpAddress);

                var ipDetail = await _ipRepository.GetIpAsync(jobDetail.IpAddress);
                if (ipDetail != null)
                {
                    await _ipRepository.UpdateIpAsync(clientIpDetail, jobDetail.IpAddress);
                }
                else
                {
                    await _ipRepository.AddIpAsync(clientIpDetail, jobDetail.IpAddress);
                }

                return (jobDetail.JobId, JobStatus.Completed);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Could not complete add or update job detail for address:{ip}", jobDetail.IpAddress);
                return (jobDetail.JobId, JobStatus.Failed);
            }
        }



        public async Task<Ip> GetIpAsync(string ipAddress)
        {
            var ipDetail = await _ipRepository.GetIpAsync(ipAddress);
            if (ipDetail != null)
            {
                return ipDetail;
            }

            var clientIpDetail = await _ipInfoProvider.GetDetailsAsync(ipAddress);

            if (clientIpDetail != null)
            {
                return await _ipRepository.AddIpAsync(clientIpDetail, ipAddress);
            }

            return null;
        }

        public async Task<JobDetail> JobDetailUpdateAsync(CancellationToken token, JobDetail jobDetail)
        {
            // add or update ipAddress
            (var jobId, var jobStatus) = await AddOrUpdateIpAsync(jobDetail);

            return await _jobRepository.UpdateJobDetailAsync(jobDetail.IpAddress, jobStatus);
        }

        public async Task JobUpdateAsync(CancellationToken token, Job job)
        {
            if (job == null)
            {
                throw new ArgumentNullException(nameof(job));
            }

            var currentJob = await _jobRepository.GetAsync(job.Id);
            
            if (!currentJob.JobDetails.Any(z => z.Status == JobStatus.InProgress))
            {
                return;
            }

            var jobDetailsToProcess = currentJob.JobDetails.Where(z => z.Status == JobStatus.InProgress)?
                .ToList();

            await ProcessBatchJob(token, jobDetailsToProcess, currentJob);

        }

        private async Task ProcessBatchJob(CancellationToken token, List<JobDetail> jobsBuffer, Job job)
        {
            do
            {
                var jobsInBatch = 0;
                foreach (var jobToRun in jobsBuffer.Take(MaxBatchSize))
                {
                    await JobDetailUpdateAsync(token, jobToRun);
                    jobsInBatch++;
                }

                job.Completed += jobsInBatch;

                await _jobRepository.UpdateJobAsync(job);

                jobsBuffer = jobsBuffer?.Skip(MaxBatchSize)?.ToList();
            }
            while (jobsBuffer != null && jobsBuffer.Any());
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
                _backgroundTaskQueue.QueueBackgroundWorkItem(pending);
            }
        }
    }
}