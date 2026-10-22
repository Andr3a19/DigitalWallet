using DigitalWallet.Api.DTOs;
using DigitalWallet.Api.Models;

namespace DigitalWallet.Api.Services;

public interface ICurrencyExchangeService
{
    ExchangeResult Convert(decimal amount, Currency sourceCurrency, Currency destinationCurrency);
}