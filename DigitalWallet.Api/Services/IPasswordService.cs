namespace DigitalWallet.Api.Services
{
    // Contract for cryptographic password hashing and verification operations
    public interface IPasswordService
    {
        // Hashes a plain-text password using a cryptographically secure one-way algorithm with automatic salting
        string HashPassword(string password);

        // Verifies whether a provided plain-text password matches a stored cryptographic hash
        bool VerifyPassword(string password, string passwordHash);
    }
}