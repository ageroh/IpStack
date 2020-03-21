using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Novibet.IpStack.Business.Data;
using Novibet.IpStack.Business.Models;

namespace Novibet.IpStack.Business.Repositories
{
    public class JobRepository : IJobRepository
    {
        private readonly ILogger<IpRepository> _logger;
        private readonly IpStackContext _dbContext;

        public JobRepository(ILogger<IpRepository> logger, IpStackContext dbContext)
        {
            _logger = logger;
            _dbContext = dbContext;
        }

        public async Task<List<Job>> GetByJobStatus(JobStatus inProgress)
        {
            return await
                _dbContext.Jobs.Where(z =>
                    z.JobDetails.Any(x => x.Status == JobStatus.InProgress))
                        .ToListAsync()
                        .ConfigureAwait(false);
        }


        public Task<Job> GetAsync(Guid jobId)
        {
            return _dbContext.Jobs
                    .Include(z => z.JobDetails)
                    .FirstOrDefaultAsync(z => z.Id == jobId);
        }

        public async Task<Job> CreateJobAsync(string[] ipAddressess)
        {
            var job = new Job()
            {
                Completed = 0,
                Id = Guid.NewGuid(),
                Total = ipAddressess.Length,
            };

            await _dbContext.Jobs.AddAsync(job);
            await _dbContext.SaveChangesAsync();

            await CreateJobDetails(ipAddressess, job.Id);

            return job;

        }


        public async Task<IEnumerable<JobDetail>> CreateJobDetails(string[] ipAddressess, Guid jobId)
        {
            var jobDetails = new List<JobDetail>();
            foreach (var ipAddress in ipAddressess)
            {
                jobDetails.Add(
                    new JobDetail
                    {
                        IpAddress = ipAddress,
                        JobId = jobId,
                        Status = JobStatus.InProgress
                    });
            }

            await _dbContext.AddRangeAsync(jobDetails);
            await _dbContext.SaveChangesAsync();

            return jobDetails;
        }

        public async Task<JobDetail> UpdateJobDetailAsync(string ipAddress, JobStatus jobStatus)
        {
            var job = await _dbContext.JobDetails.FirstOrDefaultAsync(x => x.IpAddress == ipAddress);
            if (job == null)
            {
                throw new ArgumentNullException(nameof(ipAddress), $"JobDetail Id for IP: {ipAddress} does not exists.");
            }

            job.Status = jobStatus;

            _dbContext.JobDetails.Update(job);
            await _dbContext.SaveChangesAsync();

            return job;
        }

        public async Task UpdateJobAsync(Job job)
        {
            _dbContext.Jobs.Update(job);

            await _dbContext.SaveChangesAsync();
        }
    }
}
