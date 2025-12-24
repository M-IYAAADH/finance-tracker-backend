using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FinanceTracker.Api.Data;
using FinanceTracker.Api.Services;
using FinanceTracker.Api.DTOs;
using FinanceTracker.Api.Models;
using Microsoft.AspNetCore.RateLimiting;


namespace FinanceTracker.Api.Controllers
{
    [EnableRateLimiting("auth")]
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly PasswordService _passwordService;
        private readonly TokenService _tokenService;

        public AuthController(
            AppDbContext context,
            PasswordService passwordService,
            TokenService tokenService)
        {
            _context = context;
            _passwordService = passwordService;
            _tokenService = tokenService;
        }

        // =========================
        // REGISTER
        // =========================
        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterUserDto dto)
        {
            // 1. Check if email already exists
            var userExists = await _context.Users
                .AnyAsync(u => u.Email == dto.Email);

            if (userExists)
                return BadRequest("Email already registered");

            // 2. Hash password
            _passwordService.CreatePasswordHash(
                dto.Password,
                out string passwordHash,
                out string passwordSalt
            );

            // 3. Create user entity
            var user = new User
            {
                Email = dto.Email,
                FullName = dto.FullName,
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt
            };

            // 4. Save to database
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // 5. Return safe response
            return Ok(new UserResponseDto
            {
                Id = user.Id,
                Email = user.Email,
                FullName = user.FullName,
                CreatedAt = user.CreatedAt
            });
        }

        // =========================
        // LOGIN + JWT
        // =========================
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginUserDto dto)
        {
            // 1. Find user by email
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == dto.Email);

            if (user == null)
                return Unauthorized("Invalid email or password");

            // 2. Verify password
            var isPasswordValid = _passwordService.VerifyPasswordHash(
                dto.Password,
                user.PasswordHash,
                user.PasswordSalt
            );

            if (!isPasswordValid)
                return Unauthorized("Invalid email or password");

            // 3. Generate JWT token
            var token = _tokenService.CreateToken(user);

            // 4. Return token + user
            return Ok(new
            {
                token,
                user = new UserResponseDto
                {
                    Id = user.Id,
                    Email = user.Email,
                    FullName = user.FullName,
                    CreatedAt = user.CreatedAt
                }
            });
        }
    }
}
