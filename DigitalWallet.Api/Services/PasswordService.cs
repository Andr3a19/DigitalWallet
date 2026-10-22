using DigitalWallet.Api.Models;
using Microsoft.AspNetCore.Identity;

namespace DigitalWallet.Api.Services;

public class PasswordService : IPasswordService
{
    private readonly PasswordHasher<User> _passwordHasher = new();

    // PasswordHasher<User> default implementation ignores the user instance
    public string HashPassword(string password) => _passwordHasher.HashPassword(null!, password);

    public bool VerifyPassword(string password, string passwordHash)
    {
        var result = _passwordHasher.VerifyHashedPassword(null!, passwordHash, password);
        return result != PasswordVerificationResult.Failed;
    }
}