using System.Security.Cryptography;
using System.Text;

namespace FinanceTracker.Api.Services
{
    public class PasswordService
    {
        public void CreatePasswordHash(
            string password,
            out string passwordHash,
            out string passwordSalt)
        {
            using var hmac = new HMACSHA256();

            passwordSalt = Convert.ToBase64String(hmac.Key);
            passwordHash = Convert.ToBase64String(
                hmac.ComputeHash(Encoding.UTF8.GetBytes(password))
            );
        }

        public bool VerifyPasswordHash(
            string password, 
            string storedHash,
            string storedSalt)

        {
        var key = Convert.FromBase64String(storedSalt);
        using var hmac = new HMACSHA256(key);

        var computedHash = Convert.ToBase64String(
            hmac.ComputeHash(Encoding.UTF8.GetBytes(password))
        );
            
        return computedHash == storedHash;

        }
        
    }
}