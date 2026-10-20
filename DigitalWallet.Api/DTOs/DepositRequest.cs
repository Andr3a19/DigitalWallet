namespace DigitalWallet.Api.DTOs;

public record DepositRequest(decimal Amount, string? Description);