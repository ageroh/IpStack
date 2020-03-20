using System;
using System.ComponentModel.DataAnnotations;

namespace Novibet.IpStack.Business.Models
{
    public class JobDetail
    {
        public Guid Id { get; set; }

        public string IpAddress { get; set; }

        public JobStatus Status { get; set; }
    }
}
