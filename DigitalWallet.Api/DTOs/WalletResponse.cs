using DigitalWallet.Api.Models;

namespace DigitalWallet.Api.DTOs;

public record WalletResponse(int Id, int UserId, string OwnerName, string Iban, Currency Currency, decimal Balance, DateTime CreatedAt);