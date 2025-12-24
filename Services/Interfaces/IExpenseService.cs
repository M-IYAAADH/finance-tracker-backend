using FinanceTracker.Api.DTOs.Common;
using FinanceTracker.Api.DTOs.Expenses;

namespace FinanceTracker.Api.Services.Interfaces
{
    public interface IExpenseService
    {
        Task<PaginatedResult<ExpenseResponseDto>> GetExpensesAsync(
            Guid userId,
            int page,
            int pageSize);
        
        Task<ExpenseResponseDto> CreateExpenseAsync(
            Guid userId,
            CreateExpenseDto dto);

        Task<ExpenseResponseDto> UpdateExpenseAsync(
            Guid userId,
            Guid expenseId,
            UpdateExpenseDto dto);

        Task DeleteExpenseAsync(
            Guid userId,
            Guid expenseId);

    }
}
