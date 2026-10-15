namespace DigitalWallet.Api.Models
{
    // Domain entity representing a digital bank account
    public class Wallet
    {
        public int Id { get; set; }

        // Foreign key referencing User.Id
        public int UserId { get; set; }

        public string OwnerName { get; set; } = string.Empty;
        public decimal Balance { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property pointing to the account owner
        public User? User { get; set; }

        public Wallet(int userId, string ownerName)
        {
            UserId = userId;
            OwnerName = ownerName;
            Balance = 0.0m;
            CreatedAt = DateTime.UtcNow;
        }

        public Wallet() { }
    }
}