namespace DigitalWallet.Api.Models
{
    public enum TransactionType
    {
        Deposito,
        BonificoInUscita,
        BonificoInEntrata
    }

    public class Transaction
    {
        public int Id { get; set; }
        public int WalletId { get; set; }
        public decimal Amount { get; set; }
        public TransactionType Type { get; set; }
        public DateTime TimeStamp { get; set; } = DateTime.UtcNow;
        public string Description { get; set; } = string.Empty;

        public Transaction(int walletId, decimal amount, TransactionType type, string description)
        {
            WalletId = walletId;
            Amount = amount;
            Type = type;
            TimeStamp = DateTime.UtcNow;
            Description = description;
        }

        public Transaction() { }
    }
}