namespace DigitalWallet.Client.Models;

public record WalletResponse(
    int Id,
    int UserId,
    string OwnerName,
    string Iban,
    Currency Currency,
    decimal Balance,
    DateTime CreatedAt
);