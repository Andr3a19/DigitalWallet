namespace DigitalWallet.Client.Models;

public record TransferResponse(
    string Message,
    decimal SourceBalance,
    decimal AmountSent,
    Currency SourceCurrency,
    decimal AmountReceived,
    Currency DestinationCurrency,
    decimal ExchangeRate
);