namespace DigitalWallet.Client.Models
{
    public record WalletResponse(int Id, string OwnerName, decimal Balance, DateTime CreatedAt);
}