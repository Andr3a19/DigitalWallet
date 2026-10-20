namespace DigitalWallet.Api.DTOs;

public record TransferRequest(int SourceWalletId, int DestinationWalletId, decimal Amount, string? Description);