using System.ComponentModel.DataAnnotations;

namespace FinanceTracker.Api.Dtos
{
    public class RegisterUserDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [MinLength(6)]
        public string Password { get; set; } = string.Empty;
    }
}
