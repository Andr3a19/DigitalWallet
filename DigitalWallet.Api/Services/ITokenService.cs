using DigitalWallet.Api.Models;

namespace DigitalWallet.Api.Services
{
    // Contract for generating cryptographically signed JSON Web Tokens (JWT)
    public interface ITokenService
    {
        string GenerateToken(User user);
    }
}