using FinanceTracker.Api.Data;
using FinanceTracker.Api.Domain;
using FinanceTracker.Api.DTOs.Common;
using FinanceTracker.Api.DTOs.Expenses;
using FinanceTracker.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace FinanceTracker.Api.Services
{
    public class ExpenseService : IExpenseService
    {
        private readonly AppDbContext _context;
        private readonly IMemoryCache _cache;

        public ExpenseService(
            AppDbContext context,
            IMemoryCache cache)
        {
            _context = context;
            _cache = cache;
        }

        // =========================
        // GET (CACHED)
        // =========================
        public async Task<PaginatedResult<ExpenseResponseDto>> GetExpensesAsync(
            Guid userId,
            int page,
            int pageSize)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 20;
            if (pageSize > 50) pageSize = 50;

            var cacheKey = GetExpensesCacheKey(userId, page, pageSize);

            if (_cache.TryGetValue(cacheKey, out PaginatedResult<ExpenseResponseDto>? cached))
            {
                return cached!;
            }

            var query = _context.Expenses
                .Where(e => e.UserId == userId)
                .Include(e => e.Category)
                .OrderByDescending(e => e.Date);

            var totalCount = await query.CountAsync();

            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(e => new ExpenseResponseDto
                {
                    Id = e.Id,
                    Amount = e.Amount,
                    Date = e.Date,
                    Description = e.Description,
                    Category = e.Category.Name,
                    Type = (int)e.Type
                })
                .ToListAsync();

            var result = new PaginatedResult<ExpenseResponseDto>
            {
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount,
                Items = items
            };

            _cache.Set(
                cacheKey,
                result,
                TimeSpan.FromMinutes(5));

            return result;
        }

        // =========================
        // CREATE
        // =========================
        public async Task<ExpenseResponseDto> CreateExpenseAsync(
            Guid userId,
            CreateExpenseDto dto)
        {
            var category = await _context.Categories
                .FirstOrDefaultAsync(c =>
                    c.Id == dto.CategoryId &&
                    c.UserId == userId);

            if (category == null)
                throw new InvalidOperationException("Invalid category");

            var expense = new Expense
            {
                Id = Guid.NewGuid(),
                Amount = dto.Amount,
                Date = dto.Date,
                Description = dto.Description,
                CategoryId = dto.CategoryId,
                UserId = userId,
                Type = (TransactionType)dto.Type
            };

            _context.Expenses.Add(expense);
            await _context.SaveChangesAsync();

            IncrementExpensesCacheVersion(userId);

            return new ExpenseResponseDto
            {
                Id = expense.Id,
                Amount = expense.Amount,
                Date = expense.Date,
                Description = expense.Description,
                Category = category.Name,
                Type = (int)expense.Type
            };
        }

        // =========================
        // UPDATE
        // =========================
        public async Task<ExpenseResponseDto> UpdateExpenseAsync(
            Guid userId,
            Guid expenseId,
            UpdateExpenseDto dto)
        {
            var expense = await _context.Expenses
                .Include(e => e.Category)
                .FirstOrDefaultAsync(e =>
                    e.Id == expenseId &&
                    e.UserId == userId);

            if (expense == null)
                throw new InvalidOperationException("Expense not found");

            var category = await _context.Categories
                .FirstOrDefaultAsync(c =>
                    c.Id == dto.CategoryId &&
                    c.UserId == userId);

            if (category == null)
                throw new InvalidOperationException("Invalid category");

            expense.Amount = dto.Amount;
            expense.Date = dto.Date;
            expense.Description = dto.Description;
            expense.CategoryId = dto.CategoryId;
            expense.Type = (TransactionType)dto.Type;

            await _context.SaveChangesAsync();

            IncrementExpensesCacheVersion(userId);

            return new ExpenseResponseDto
            {
                Id = expense.Id,
                Amount = expense.Amount,
                Date = expense.Date,
                Description = expense.Description,
                Category = category.Name,
                Type = (int)expense.Type
            };
        }

        // =========================
        // DELETE
        // =========================
        public async Task DeleteExpenseAsync(
            Guid userId,
            Guid expenseId)
        {
            var expense = await _context.Expenses
                .FirstOrDefaultAsync(e =>
                    e.Id == expenseId &&
                    e.UserId == userId);

            if (expense == null)
                throw new InvalidOperationException("Expense not found");

            expense.IsDeleted = true;
            expense.DeletedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            IncrementExpensesCacheVersion(userId);

        }

        // =========================
        // CACHE VERSIONING (SAFE)
        // =========================
        private string GetExpensesVersionKey(Guid userId)
        {
            return $"expenses:{userId}:version";
        }

        private int GetExpensesCacheVersion(Guid userId)
        {
            return _cache.GetOrCreate(
                GetExpensesVersionKey(userId),
                entry =>
                {
                    entry.SlidingExpiration = TimeSpan.FromHours(1);
                    return 1;
                });
        }

        private void IncrementExpensesCacheVersion(Guid userId)
        {
            var key = GetExpensesVersionKey(userId);
            var current = GetExpensesCacheVersion(userId);
            _cache.Set(key, current + 1);
        }

        private string GetExpensesCacheKey(
            Guid userId,
            int page,
            int pageSize)
        {
            var version = GetExpensesCacheVersion(userId);
            return $"expenses:{userId}:v:{version}:page:{page}:size:{pageSize}";
        }
    }
}
