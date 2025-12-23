namespace FinanceTracker.Api.DTOs.Common
{
    public class ErrorResponseDto
    {
        public string Error { get; set; } = null!;
        public string Message { get; set; } = null!;
    }
}
