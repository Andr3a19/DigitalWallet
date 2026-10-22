using DigitalWallet.Api.Models;

namespace DigitalWallet.Api.DTOs;

public record TransferResponse(
    string Message,
    decimal SourceBalance,
    decimal AmountSent,
    Currency SourceCurrency,
    decimal AmountReceived,
    Currency DestinationCurrency,
    decimal ExchangeRate
);