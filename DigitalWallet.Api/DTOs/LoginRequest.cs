namespace DigitalWallet.Api.DTOs
{
    // DTO representing credentials submitted during user authentication
    public record LoginRequest(string Username, string Password);
}