namespace DigitalWallet.Api.Models;

public class Transaction
{
    public int Id { get; init; }
    public int WalletId { get; init; }
    public decimal Amount { get; init; }
    public TransactionType Type { get; init; }
    public DateTime Timestamp { get; init; }
    public string Description { get; init; } = string.Empty;

    public Transaction(int walletId, decimal amount, TransactionType type, string description)
    {
        if (walletId <= 0)
            throw new ArgumentException("L'ID del conto deve essere maggiore di zero", nameof(walletId));

        if (amount <= 0)
            throw new ArgumentOutOfRangeException(nameof(amount), "L'importo deve essere maggiore di zero");

        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("La descrizione della transazione non può essere vuota", nameof(description));

        WalletId = walletId;
        Amount = amount;
        Type = type;
        Timestamp = DateTime.UtcNow;
        Description = description.Trim();
    }

    protected Transaction() { }
}