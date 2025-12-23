namespace FinanceTracker.Api.DTOs.Expenses
{
    public class ExpenseResponseDto
    {
        public Guid Id { get; set; }
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
        public string? Description { get; set; }
        public string Category { get; set; } = null!;
        public int Type { get; set; }
    }
}
