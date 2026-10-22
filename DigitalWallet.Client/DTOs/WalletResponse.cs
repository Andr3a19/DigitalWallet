using DigitalWallet.Client.Models;

namespace DigitalWallet.Client.DTOs;

public record WalletResponse(
    int Id,
    int UserId,
    string OwnerName,
    string Iban,
    Currency Currency,
    decimal Balance,
    DateTime CreatedAt
);