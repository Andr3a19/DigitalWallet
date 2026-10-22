using DigitalWallet.Client.Models;

namespace DigitalWallet.Client.DTOs;

public record TransactionResponse(
    int Id,
    int WalletId,
    decimal Amount,
    TransactionType Type,
    DateTime Timestamp,
    string Description
);