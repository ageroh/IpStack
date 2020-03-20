using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Novibet.IpStack.Business.Models;

namespace Novibet.IpStack.Business.Repositories
{
    public interface IJobRepository
    {
        Task<List<Job>> GetByJobStatus(JobStatus inProgress);
        Task<Job> GetAsync(Guid jobId);
        Task<Job> CreateJobAsync(string[] ipAddressess);
        Task<JobDetail> UpdateJobDetailAsync(string ipAddress, JobStatus jobStatus);
        Task UpdateJobAsync(Job job);
    }
}