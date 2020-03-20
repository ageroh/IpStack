using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Novibet.IpStack.Business.Models
{
    public class Job
    {
        [Key]
        public Guid Id { get; set; }

        public int Total { get; set; }

        public int Completed { get; set; }

        public IEnumerable<JobDetail> JobDetails { get; set; }

    }
}
