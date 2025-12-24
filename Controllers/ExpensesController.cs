using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FinanceTracker.Api.Data;
using FinanceTracker.Api.Domain;
using FinanceTracker.Api.DTOs.Expenses;
using FinanceTracker.Api.DTOs.Common;
using FinanceTracker.Api.Services.Interfaces;

namespace FinanceTracker.Api.Controllers
{
    [ApiController]
    [Route("api/expenses")]
    [Authorize]
    public class ExpensesController : ControllerBase
    {
        private readonly IExpenseService _expenseService;
        private readonly AppDbContext _context;

        public ExpensesController(
            AppDbContext context,
            IExpenseService expenseService)
        {
            _context = context;
            _expenseService = expenseService;
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
            var userId = GetUserId();

            var result = await _expenseService.GetExpensesAsync(
                userId,
                page,
                pageSize);

            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> CreateExpense(CreateExpenseDto dto)
        {
            var userId = GetUserId();

            var result = await _expenseService.CreateExpenseAsync(userId, dto);

            return Ok(result);
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateExpense(
            Guid id,
            UpdateExpenseDto dto)
        {
            var userId = GetUserId();

            var result = await _expenseService.UpdateExpenseAsync(
                userId,
                id,
                dto);

            return Ok(result);
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteExpense(Guid id)
        {
            var userId = GetUserId();

            await _expenseService.DeleteExpenseAsync(userId, id);

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
