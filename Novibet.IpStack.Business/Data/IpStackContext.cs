using Microsoft.EntityFrameworkCore;
using Novibet.IpStack.Business.Models;

namespace Novibet.IpStack.Business.Data
{
    public class IpStackContext : DbContext 
    {
        public IpStackContext(DbContextOptions<IpStackContext> options) : base(options)
        {
        }

        public DbSet<Ip> Ip { get; set; }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Country>().ToTable("Country");
            modelBuilder.Entity<Continent>().ToTable("Continent");
            modelBuilder.Entity<City>().ToTable("City");
            modelBuilder.Entity<Ip>().ToTable("Ip");

            modelBuilder.Entity<Ip>()
               .HasOne(d => d.City)
               .WithMany()
               .HasForeignKey();

            modelBuilder.Entity<Ip>()
               .HasOne(d => d.Country)
               .WithMany()
               .HasForeignKey();

            modelBuilder.Entity<Ip>()
               .HasOne(d => d.Continent)
               .WithMany()
               .HasForeignKey();

            modelBuilder.Entity<Ip>().HasKey(z => z.IpAddress);
            modelBuilder.Entity<Ip>().HasIndex(z => z.IpAddress).IsUnique();
        }
    }
}
