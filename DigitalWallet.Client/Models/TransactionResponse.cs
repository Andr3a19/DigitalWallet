namespace DigitalWallet.Client.Models
{
    // Enumeration representing the transaction category
    public enum TransactionType
    {
        Deposito,
        BonificoInUscita,
        BonificoInEntrata
    }

    // Response model representing a recorded transaction received from the API
    public record TransactionResponse(
        int Id,
        int WalletId,
        decimal Amount,
        TransactionType Type,
        DateTime TimeStamp,
        string Description
    );
}