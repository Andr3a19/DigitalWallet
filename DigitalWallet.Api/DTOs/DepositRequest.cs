namespace DigitalWallet.Api.DTOs
{
    // DTO for depositing funds into a wallet
    public record DepositRequest(decimal Amount, string? Description);
}