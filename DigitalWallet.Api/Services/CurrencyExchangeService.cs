using DigitalWallet.Api.DTOs;
using DigitalWallet.Api.Models;

namespace DigitalWallet.Api.Services;

public class CurrencyExchangeService : ICurrencyExchangeService
{
    public ExchangeResult Convert(decimal amount, Currency sourceCurrency, Currency destinationCurrency)
    {
        if (amount <= 0)
            throw new ArgumentOutOfRangeException(nameof(amount), "L'importo deve essere maggiore di zero");

        if (sourceCurrency == destinationCurrency)
            return new ExchangeResult(amount, 1.0m);

        return (sourceCurrency, destinationCurrency) switch
        {
            (Currency.EUR, Currency.CHF) => new ExchangeResult(decimal.Round(amount * 0.95m, 2, MidpointRounding.AwayFromZero), 0.95m),
            (Currency.CHF, Currency.EUR) => new ExchangeResult(decimal.Round(amount * 1.05m, 2, MidpointRounding.AwayFromZero), 1.05m),
            _ => throw new NotSupportedException("Conversione valutaria non supportata")
        };
    }
}