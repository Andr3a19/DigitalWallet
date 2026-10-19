using DigitalWallet.Api.Models;

namespace DigitalWallet.Api.DTOs;

public record RegisterRequest(string Username, string FullName, string Password, Role Role = Role.Customer);