using Microsoft.EntityFrameworkCore;

namespace FinanceTracker.Api.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // Db sets will go here later :))
    }
}