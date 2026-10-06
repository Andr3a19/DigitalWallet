namespace DigitalWallet.Api.DTOs
{
    // DTO representing a peer-to-peer transfer request between two wallets
    public record TransferRequest(int SourceWalletId, int DestinationWalletId, decimal Amount, string? Description);
}