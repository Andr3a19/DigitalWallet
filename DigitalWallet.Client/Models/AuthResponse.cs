namespace DigitalWallet.Client.Models;

public record AuthResponse(string Token, int UserId, string Username, string FullName, string Role);