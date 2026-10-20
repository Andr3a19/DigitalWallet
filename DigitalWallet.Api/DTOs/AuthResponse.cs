namespace DigitalWallet.Api.DTOs;

public record AuthResponse(string Token, int UserId, string Username, string FullName, string Role);