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

            _backgroundTaskQueue.QueueBackgroundWorkItem((token) => JobUpdateAsync(token, job));

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

                return (jobDetail.Id, JobStatus.Completed);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Could not complete add or update job detail for address:{ip}", jobDetail.IpAddress);
                return (jobDetail.Id, JobStatus.Failed);
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
            // process only jobdetails left in progress.
            if (job == null
                || !job.JobDetails.Any(z => z.Status == JobStatus.InProgress))
            {
                return;
            }

            var inProgressJobs = job.JobDetails.Where(z => z.Status == JobStatus.InProgress)?
                .ToList();

            var jobsBuffer = new List<Task>();
            for (int i = 0; i < inProgressJobs.Count; i++)
            {
                jobsBuffer.Add(JobDetailUpdateAsync(token, inProgressJobs[i]));
            }

            await ProcessBatchJob(jobsBuffer, job);

        }

        private async Task<List<Task>> ProcessBatchJob(List<Task> jobsBuffer, Job job)
        {
            IEnumerable<Task> jobsToRun;
            do
            {
                jobsToRun = jobsBuffer.Take(MaxBatchSize);
                foreach (var jobToRun in jobsToRun)
                {
                    jobToRun.Start();
                }

                await Task.WhenAll(jobsToRun);
                
                // those are completed or failed.
                job.Completed += jobsToRun.Count();

                await _jobRepository.UpdateJobAsync(job);

                jobsBuffer = jobsBuffer?.Skip(MaxBatchSize)?.ToList();
            }
            while (jobsToRun != null && jobsToRun.Any());
            return jobsBuffer;
        }
    }
}