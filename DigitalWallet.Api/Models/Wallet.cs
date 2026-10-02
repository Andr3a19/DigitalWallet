namespace DigitalWallet.Api.Models
{
    public class Wallet
    {
        public int Id { get; set; }
        public string OwnerName { get; set; } = string.Empty;
        public decimal Balance { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Wallet(string ownerName)
        {
            OwnerName = ownerName;
            Balance = 0.0m;
            CreatedAt = DateTime.UtcNow;
        }

        public Wallet() { }
    }
}