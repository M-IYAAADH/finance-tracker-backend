using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FinanceTracker.Api.Data;
using FinanceTracker.Api.Domain;

namespace FinanceTracker.Api.Controllers
{
    [ApiController]
    [Route("api/reports")]
    [Authorize]
    public class ReportsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ReportsController(AppDbContext context)
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

        [HttpGet("monthly/category-breakdown")]
        public async Task<IActionResult> GetCategoryBreakdown(
            int year,
            int month,
            int type)
        {
            var userId = GetUserId();

            if (!Enum.IsDefined(typeof(TransactionType), type))
                return BadRequest("Invalid transaction type.");

            var data = await _context.Expenses
                .Where(e =>
                    e.UserId == userId &&
                    e.Type == (TransactionType)type &&
                    e.Date.Year == year &&
                    e.Date.Month == month)
                .GroupBy(e => e.Category.Name)
                .Select(g => new
                {
                    category = g.Key,
                    total = g.Sum(x => x.Amount)
                })
                .OrderByDescending(x => x.total)
                .ToListAsync();

            return Ok(data);
        }

        [HttpGet("monthly/daily-trend")]
        public async Task<IActionResult> GetDailyTrend(
            int year,
            int month,
            int type)
        {
            var userId = GetUserId();

            if (!Enum.IsDefined(typeof(TransactionType), type))
                return BadRequest("Invalid transaction type.");

            var data = await _context.Expenses
                .Where(e =>
                    e.UserId == userId &&
                    e.Type == (TransactionType)type &&
                    e.Date.Year == year &&
                    e.Date.Month == month)
                .GroupBy(e => e.Date.Date)
                .Select(g => new
                {
                    date = g.Key,
                    total = g.Sum(x => x.Amount)
                })
                .OrderBy(x => x.date)
                .ToListAsync();

            return Ok(data);
        }
        [HttpGet("monthly/summary-dashboard")]
        public async Task<IActionResult> GetMonthlyDashboardSummary(int year, int month)
        {
            var userId = GetUserId();

            // Total income
            var income = await _context.Expenses
                .Where(e =>
                    e.UserId == userId &&
                    e.Type == TransactionType.Income &&
                    e.Date.Year == year &&
                    e.Date.Month == month)
                .SumAsync(e => e.Amount);

            // Total expense
            var expense = await _context.Expenses
                .Where(e =>
                    e.UserId == userId &&
                    e.Type == TransactionType.Expense &&
                    e.Date.Year == year &&
                    e.Date.Month == month)
                .SumAsync(e => e.Amount);

            // Top 5 expense categories
            var topCategories = await _context.Expenses
                .Where(e =>
                    e.UserId == userId &&
                    e.Type == TransactionType.Expense &&
                    e.Date.Year == year &&
                    e.Date.Month == month)
                .GroupBy(e => e.Category.Name)
                .Select(g => new
                {
                    category = g.Key,
                    total = g.Sum(x => x.Amount)
                })
                .OrderByDescending(x => x.total)
                .Take(5)
                .ToListAsync();

            return Ok(new
            {
                year,
                month,
                income,
                expense,
                balance = income - expense,
                topCategories
            });
        }



    }
}
