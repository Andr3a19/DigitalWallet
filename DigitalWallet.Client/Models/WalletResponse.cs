namespace DigitalWallet.Client.Models
{
    // Response model representing a wallet entity received from the API
    public record WalletResponse(int Id, string OwnerName, decimal Balance, DateTime CreatedAt);
}