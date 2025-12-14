using Microsoft.EntityFrameworkCore;
using FinanceTracker.Api.Models;
using FinanceTracker.Api.Domain;
using FinanceTracker.Api.Data.Configurations;

namespace FinanceTracker.Api.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        // Db Sets
        public DbSet<User> Users { get; set; }
        public DbSet<Category> Categories { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfiguration(new CategoryConfiguration());
        }
    }
}
