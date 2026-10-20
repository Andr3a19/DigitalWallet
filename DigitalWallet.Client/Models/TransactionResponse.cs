namespace DigitalWallet.Client.Models;

public record TransactionResponse(
    int Id,
    int WalletId,
    decimal Amount,
    TransactionType Type,
    DateTime Timestamp,
    string Description
);