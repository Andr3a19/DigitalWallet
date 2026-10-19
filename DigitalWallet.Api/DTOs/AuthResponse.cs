namespace DigitalWallet.Api.DTOs;
// DTO returned upon successful authentication containing the signed JWT and basic profile data
public record AuthResponse(string Token, int UserId, string Username, string FullName, string Role);