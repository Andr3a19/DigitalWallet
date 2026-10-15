using DigitalWallet.Api.Models;
using Microsoft.AspNetCore.Identity;

namespace DigitalWallet.Api.Services
{
    // Cryptographic service utilizing ASP.NET Core Identity PBKDF2 with HMAC-SHA256 and per-user salting
    public class PasswordService : IPasswordService
    {
        private readonly PasswordHasher<User> _passwordHasher = new();

        // Generates a salted PBKDF2 hash from a clear-text password
        public string HashPassword(string password) => _passwordHasher.HashPassword(null!, password);

        // Validates a clear-text password against a stored hash, ensuring it does not evaluate to Failed
        public bool VerifyPassword(string password, string passwordHash)
        {
            var result = _passwordHasher.VerifyHashedPassword(null!, passwordHash, password);
            return result != PasswordVerificationResult.Failed;
        }
    }
}