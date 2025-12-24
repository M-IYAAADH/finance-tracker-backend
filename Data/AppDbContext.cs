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
        public DbSet<Expense> Expenses { get; set; }
        public DbSet<Category> Categories { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

                modelBuilder.Entity<Expense>()
                    .HasQueryFilter(e => !e.IsDeleted);

                modelBuilder.Entity<Category>()
                    .HasQueryFilter(c => !c.IsDeleted);

            modelBuilder.ApplyConfiguration(new CategoryConfiguration());
            modelBuilder.ApplyConfiguration(new ExpenseConfiguration());
        }
    }
}
