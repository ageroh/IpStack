using Microsoft.EntityFrameworkCore;
using Novibet.IpStack.Business.Models;

namespace Novibet.IpStack.Business.Data
{
    public class IpStackContext : DbContext 
    {
        public IpStackContext(DbContextOptions<IpStackContext> options) : base(options)
        {

        }

        public DbSet<Ip> IpAddressess { get; set; }
        public DbSet<Continent> Continents { get; set; }
        public DbSet<Country> Countries { get; set; }
        public DbSet<City> Cities { get; set; }
        public DbSet<Job> Jobs { get; set; }
        public DbSet<JobDetail> JobDetails { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Country>().ToTable("Country");
            modelBuilder.Entity<Continent>().ToTable("Continent");
            modelBuilder.Entity<City>().ToTable("City");
            modelBuilder.Entity<Ip>().ToTable("Ip");

            modelBuilder.Entity<Ip>().HasIndex(z => z.IpAddress).IsUnique();

            modelBuilder.Entity<Job>().ToTable("Job");

            modelBuilder.Entity<JobDetail>().ToTable("JobDetail");

            modelBuilder.Entity<Job>().HasIndex(z => z.Id).IsUnique();

            base.OnModelCreating(modelBuilder);
        }

#if DEBUG
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.EnableSensitiveDataLogging();
        }
#endif
    }
}
