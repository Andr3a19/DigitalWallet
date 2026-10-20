using DigitalWallet.Api.Models;

namespace DigitalWallet.Api.DTOs;

public record CreateWalletRequest(string OwnerName, Currency Currency = Currency.EUR);