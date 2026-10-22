namespace DigitalWallet.Api.Models;

public class Wallet
{
    public int Id { get; init; }
    public int UserId { get; init; }
    public string OwnerName { get; private set; } = string.Empty;
    public string Iban { get; private set; } = string.Empty;
    public Currency Currency { get; private set; }
    public decimal Balance { get; private set; }
    public DateTime CreatedAt { get; init; }
    public Guid ConcurrencyToken { get; private set; } = Guid.NewGuid();

    public User? User { get; private set; }

    public Wallet(int userId, string ownerName, string iban, Currency currency = Currency.EUR)
    {
        if (userId <= 0)
            throw new ArgumentException("L'ID utente deve essere maggiore di zero", nameof(userId));

        if (string.IsNullOrWhiteSpace(ownerName))
            throw new ArgumentException("Il nome del titolare non può essere vuoto", nameof(ownerName));

        if (string.IsNullOrWhiteSpace(iban))
            throw new ArgumentException("L'IBAN non può essere vuoto", nameof(iban));

        UserId = userId;
        OwnerName = ownerName.Trim();
        Iban = iban.Trim().ToUpperInvariant();
        Currency = currency;
        Balance = 0.0m;
        CreatedAt = DateTime.UtcNow;
    }

    protected Wallet() { }

    public void Credit(decimal amount)
    {
        if (amount <= 0)
            throw new ArgumentOutOfRangeException(nameof(amount), "L'importo da accreditare deve essere maggiore di zero");

        Balance += amount;
        ConcurrencyToken = Guid.NewGuid();
    }

    public void Debit(decimal amount)
    {
        if (amount <= 0)
            throw new ArgumentOutOfRangeException(nameof(amount), "L'importo di addebito deve essere rigorosamente maggiore di zero");

        if (amount > Balance)
            throw new InvalidOperationException($"Fondi insufficienti: saldo disponibile {Balance:F2}, richiesto {amount:F2}");

        Balance -= amount;
        ConcurrencyToken = Guid.NewGuid();
    }
}