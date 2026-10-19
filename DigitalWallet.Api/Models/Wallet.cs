namespace DigitalWallet.Api.Models;

public class Wallet
{
    public int Id { get; init; }

    // Foreign key referencing User.Id
    public int UserId { get; init; }

    public string OwnerName { get; private set; } = string.Empty;
    public string Iban { get; private set; } = string.Empty;
    public Currency Currency { get; private set; }
    public decimal Balance { get; private set; }
    public DateTime CreatedAt { get; init; }
    public Guid ConcurrencyToken { get; private set; } = Guid.NewGuid();

    // Navigation property pointing to the account owner
    public User? User { get; init; }

    public Wallet(int userId, string ownerName, string iban, Currency currency = Currency.EUR)
    {
        if (userId <= 0)
            throw new ArgumentException("Lo userId non può essere negativo", nameof(userId));

        if (string.IsNullOrWhiteSpace(ownerName))
            throw new ArgumentException("Il nome del titolare non può essere vuoto", nameof(ownerName));

        if (string.IsNullOrWhiteSpace(iban))
            throw new ArgumentException("L'IBAN non può essere vuoto", nameof(iban));

        UserId = userId;
        OwnerName = ownerName;
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
            throw new InvalidOperationException($"Fondi insufficienti. Saldo disponibile: {Balance:F2}, richiesto: {amount:F2}");

        Balance -= amount;
        ConcurrencyToken = Guid.NewGuid();
    }
}