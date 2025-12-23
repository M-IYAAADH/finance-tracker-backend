using FinanceTracker.Api.Models;

namespace FinanceTracker.Api.Domain
{
    public class Expense
    {
        public Guid Id { get; set; }

        public decimal Amount { get; set; }

        public DateTime Date { get; set; }

        public string? Description { get; set; }

        public Guid CategoryId { get; set; }
        public Category Category { get; set; } = null!;

        public Guid UserId { get; set; }
        public User User { get; set; } = null!;
    }
}
