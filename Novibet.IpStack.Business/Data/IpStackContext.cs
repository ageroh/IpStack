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


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Country>().ToTable("Country");
            modelBuilder.Entity<Continent>().ToTable("Continent");
            modelBuilder.Entity<City>().ToTable("City");
            modelBuilder.Entity<Ip>().ToTable("Ip");

            modelBuilder.Entity<Ip>().HasIndex(z => z.IpAddress).IsUnique();

            base.OnModelCreating(modelBuilder);
        }
    }
}
