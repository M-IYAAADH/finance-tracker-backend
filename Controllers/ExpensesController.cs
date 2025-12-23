using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FinanceTracker.Api.Data;
using FinanceTracker.Api.Domain;
using FinanceTracker.Api.DTOs.Expenses;
using FinanceTracker.Api.DTOs.Common;

namespace FinanceTracker.Api.Controllers
{
    [ApiController]
    [Route("api/expenses")]
    [Authorize]
    public class ExpensesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ExpensesController(AppDbContext context)
        {
            _context = context;
        }

        private Guid GetUserId()
        {
            return Guid.Parse(
                User.FindFirst("sub")?.Value
                ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value
            );
        }

        [HttpGet]
        public async Task<IActionResult> GetExpenses(
            int page = 1,
            int pageSize = 20)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 20;
            if (pageSize > 50) pageSize = 50;

            var userId = GetUserId();

            var query = _context.Expenses
                .Where(e => e.UserId == userId)
                .Include(e => e.Category)
                .OrderByDescending(e => e.Date);

            var totalCount = await query.CountAsync();

            var expenses = await query
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

            return Ok(new
            {
                page,
                pageSize,
                totalCount,
                items = expenses
            });
        }


        [HttpPost]
        public async Task<IActionResult> CreateExpense(CreateExpenseDto dto)
        {
            var userId = GetUserId();

            // Validate amount
            if (dto.Amount <= 0)
                return BadRequest(new ErrorResponseDto
                {
                    Error = "ValidationError",
                    Message = "Amount must be greater than zero."
                });

            // Validate category belongs to user
            var categoryExists = await _context.Categories.AnyAsync(c =>
                c.Id == dto.CategoryId && c.UserId == userId);

            if (!categoryExists)
                return BadRequest(new ErrorResponseDto
                {
                    Error = "ValidationError",
                    Message = "Amount must be greater than zero."
                });

            if (!Enum.IsDefined(typeof(TransactionType), dto.Type))
                return BadRequest(new ErrorResponseDto
                {
                    Error = "ValidationError",
                    Message = "Amount must be greater than zero."
                });

            var expense = new Expense
            {
                Id = Guid.NewGuid(),
                Amount = dto.Amount,
                Date = dto.Date,
                Description = dto.Description,
                CategoryId = dto.CategoryId,
                UserId = userId,
                Type = (TransactionType)dto.Type,
                CreatedAt = DateTime.UtcNow
            };

            _context.Expenses.Add(expense);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetExpenses), new { id = expense.Id }, expense);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateExpense(Guid id, UpdateExpenseDto dto)
        {
            var userId = GetUserId();

            if (dto.Amount <= 0)
                return BadRequest("Amount must be greater than zero.");

            // Fetch expense owned by user
            var expense = await _context.Expenses
                .FirstOrDefaultAsync(e => e.Id == id && e.UserId == userId);

            if (expense == null)
                return NotFound("Expense not found.");

            if (!Enum.IsDefined(typeof(TransactionType), dto.Type))
                return BadRequest("Invalid transaction type.");

            expense.Type = (TransactionType)dto.Type;

            // Validate category ownership
            var categoryExists = await _context.Categories.AnyAsync(c =>
                c.Id == dto.CategoryId && c.UserId == userId);

            if (!categoryExists)
                return BadRequest("Invalid category.");

            expense.Amount = dto.Amount;
            expense.Date = dto.Date;
            expense.CategoryId = dto.CategoryId;
            expense.Description = dto.Description;
            expense.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(expense);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteExpense(Guid id)
        {
            var userId = GetUserId();

            var expense = await _context.Expenses
                .FirstOrDefaultAsync(e => e.Id == id && e.UserId == userId);

            if (expense == null)
                return NotFound("Expense not found.");

            _context.Expenses.Remove(expense);
            await _context.SaveChangesAsync();

            return NoContent();
        }
        [HttpGet("monthly")]
        public async Task<IActionResult> GetMonthlyExpenses(int year, int month)
        {
            var userId = GetUserId();

            var expenses = await _context.Expenses
                .Where(e =>
                    e.UserId == userId &&
                    e.Date.Year == year &&
                    e.Date.Month == month)
                .Include(e => e.Category)
                .OrderByDescending(e => e.Date)
                .ToListAsync();

            return Ok(expenses);
        }

        [HttpGet("monthly/total")]
        public async Task<IActionResult> GetMonthlyTotal(int year, int month)
        {
            var userId = GetUserId();

            var total = await _context.Expenses
                .Where(e =>
                    e.UserId == userId &&
                    e.Date.Year == year &&
                    e.Date.Month == month)
                .SumAsync(e => e.Amount);

            return Ok(new
            {
                year,
                month,
                total
            });
        }

        [HttpGet("monthly/income")]
        public async Task<IActionResult> GetMonthlyIncome(int year, int month)
        {
            var userId = GetUserId();

            var totalIncome = await _context.Expenses
                .Where(e =>
                    e.UserId == userId &&
                    e.Type == TransactionType.Income &&
                    e.Date.Year == year &&
                    e.Date.Month == month)
                .SumAsync(e => e.Amount);

            return Ok(new
            {
                year,
                month,
                income = totalIncome
            });
        }

        [HttpGet("monthly/expense")]
        public async Task<IActionResult> GetMonthlyExpense(int year, int month)
        {
            var userId = GetUserId();

            var totalExpense = await _context.Expenses
                .Where(e =>
                    e.UserId == userId &&
                    e.Type == TransactionType.Expense &&
                    e.Date.Year == year &&
                    e.Date.Month == month)
                .SumAsync(e => e.Amount);

            return Ok(new
            {
                year,
                month,
                expense = totalExpense
            });
        }

        [HttpGet("monthly/summary")]
        public async Task<IActionResult> GetMonthlySummary(int year, int month)
        {
            var userId = GetUserId();

            var income = await _context.Expenses
                .Where(e =>
                    e.UserId == userId &&
                    e.Type == TransactionType.Income &&
                    e.Date.Year == year &&
                    e.Date.Month == month)
                .SumAsync(e => e.Amount);

            var expense = await _context.Expenses
                .Where(e =>
                    e.UserId == userId &&
                    e.Type == TransactionType.Expense &&
                    e.Date.Year == year &&
                    e.Date.Month == month)
                .SumAsync(e => e.Amount);

            return Ok(new
            {
                year,
                month,
                income,
                expense,
                balance = income - expense
            });
        }

    }

}
