using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FinanceTracker.Api.Data;
using FinanceTracker.Api.Domain;
using FinanceTracker.Api.DTOs.Expenses;


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
        public async Task<IActionResult> GetExpenses()
        {
            var userId = GetUserId();

            var expenses = await _context.Expenses
                .Where(e => e.UserId == userId)
                .Include(e => e.Category)
                .OrderByDescending(e => e.Date)
                .ToListAsync();

            return Ok(expenses);
        }
        [HttpPost]
        public async Task<IActionResult> CreateExpense(CreateExpenseDto dto)
        {
            var userId = GetUserId();

            // Validate amount
            if (dto.Amount <= 0)
                return BadRequest("Amount must be greater than zero.");

            // Validate category belongs to user
            var categoryExists = await _context.Categories.AnyAsync(c =>
                c.Id == dto.CategoryId && c.UserId == userId);

            if (!categoryExists)
                return BadRequest("Invalid category.");

            var expense = new Expense
            {
                Id = Guid.NewGuid(),
                Amount = dto.Amount,
                Date = dto.Date,
                Description = dto.Description,
                CategoryId = dto.CategoryId,
                UserId = userId
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

            // Validate category ownership
            var categoryExists = await _context.Categories.AnyAsync(c =>
                c.Id == dto.CategoryId && c.UserId == userId);

            if (!categoryExists)
                return BadRequest("Invalid category.");

            expense.Amount = dto.Amount;
            expense.Date = dto.Date;
            expense.CategoryId = dto.CategoryId;
            expense.Description = dto.Description;

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

    }

}
