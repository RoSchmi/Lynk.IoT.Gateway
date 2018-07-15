using Lynk.IoT.Gateway.Persistent.Models;
using Microsoft.EntityFrameworkCore;

namespace Lynk.IoT.Gateway.Persistent
{
    internal class AppDbContext : DbContext
    {
        public DbSet<Device> Devices { get; set; }
        public DbSet<Setting> Settings { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            optionsBuilder.UseSqlite("Data Source=app.sqlite");
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Device>(entity =>
            {
                entity.HasKey(x => x.Id);
                entity.Property(x => x.Id).ValueGeneratedOnAdd();
            });

            modelBuilder.Entity<Setting>(entity =>
            {
                entity.HasKey(x => x.Id);
                entity.Property(x => x.Id).ValueGeneratedOnAdd();
            });
        }
    }
}
