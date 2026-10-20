using DigitalWallet.Api.Models;

namespace DigitalWallet.Api.Services;

public interface ITokenService
{
    string GenerateToken(User user);
}