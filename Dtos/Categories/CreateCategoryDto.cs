namespace FinanceTracker.Api.DTOs.Categories
{
    public class CreateCategoryDto
    {
        public string Name { get; set; } = null!;

        public int Type { get; set; }
    }
}
