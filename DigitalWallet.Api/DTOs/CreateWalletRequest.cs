namespace DigitalWallet.Api.DTOs
{
    // Data Transfer Object for wallet creation, preventing over-posting attacks on sensitive properties like Balance
    public record CreateWalletRequest(string OwnerName);
}