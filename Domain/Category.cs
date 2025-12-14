using FinanceTracker.Api.Models;

namespace FinanceTracker.Api.Domain
{
    public enum CategoryType
    {
        Income = 1,
        Expense = 2
    }

    public class Category
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = null!;

        public CategoryType Type { get; set; }

        public Guid UserId { get; set; }

        public User User { get; set; } = null!;
    }
}
