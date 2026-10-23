using DigitalWallet.Client.DTOs;
using DigitalWallet.Client.Models;
using DigitalWallet.Client.Services;
using System.Globalization;

namespace DigitalWallet.Client.Views;

public class ClientView(WalletApiClient apiClient)
{
    public async Task StartAsync()
    {
        bool isAuthenticated = await ShowAuthMenuAsync();
        if (!isAuthenticated)
            return;

        bool isRunning = true;
        while (isRunning)
        {
            Console.WriteLine($"\nPORTAFOGLIO DIGITALE - Utente: {apiClient.CurrentUserFullName} ({apiClient.CurrentUsername})");
            Console.WriteLine("1. Mostra tutti i miei conti");
            Console.WriteLine("2. Dettaglio singolo conto");
            Console.WriteLine("3. Apri nuovo conto");
            Console.WriteLine("4. Deposita fondi");
            Console.WriteLine("5. Effettua bonifico P2P");
            Console.WriteLine("6. Estratto conto (storico movimenti)");
            Console.WriteLine("0. Esci");
            Console.Write("\nSeleziona un'opzione valida: ");

            var choice = EnterInt(0, 6);

            switch (choice)
            {
                case 0:
                    isRunning = false;
                    break;
                case 1:
                    await ShowAllWalletsAsync();
                    break;
                case 2:
                    await ShowWalletDetailsAsync();
                    break;
                case 3:
                    await CreateWalletAsync();
                    break;
                case 4:
                    await DepositFundsAsync();
                    break;
                case 5:
                    await TransferFundsAsync();
                    break;
                case 6:
                    await ShowTransactionsAsync();
                    break;
            }
        }
    }

    private async Task<bool> ShowAuthMenuAsync()
    {
        while (true)
        {
            Console.WriteLine("\nBENVENUTO NEL SISTEMA DIGITAL WALLET");
            Console.WriteLine("1. Accedi (Login)");
            Console.WriteLine("2. Registrati (Nuovo utente)");
            Console.WriteLine("0. Esci");
            Console.Write("\nSeleziona un'opzione valida: ");

            var choice = EnterInt(0, 2);
            switch (choice)
            {
                case 0:
                    return false;
                case 1:
                    if (await PerformLoginAsync())
                        return true;
                    break;
                case 2:
                    await PerformRegisterAsync();
                    break;
            }
        }
    }

    private async Task<bool> PerformLoginAsync()
    {
        Console.Write("\nInserisci lo username: ");
        var username = EnterString();

        Console.Write("Inserisci la password: ");
        var password = EnterPassword();

        var (success, message) = await apiClient.LoginAsync(username, password);
        if (success)
        {
            Console.WriteLine($"\n{message}");
            return true;
        }

        Console.WriteLine($"\nErrore di autenticazione: {message}");
        return false;
    }

    private async Task PerformRegisterAsync()
    {
        Console.Write("\nInserisci lo username desiderato: ");
        var username = EnterString();

        Console.Write("Inserisci il nome e cognome completo: ");
        var fullName = EnterString();

        Console.Write("Inserisci una password (minimo 8 caratteri): ");
        var password = EnterString();

        var (success, message) = await apiClient.RegisterAsync(username, fullName, password);
        if (success)
            Console.WriteLine($"\n{message}");
        else
            Console.WriteLine($"\nErrore di registrazione: {message}");
    }

    private async Task ShowAllWalletsAsync()
    {
        var (wallets, error) = await apiClient.GetAllWalletsAsync();
        if (error is not null)
        {
            Console.WriteLine($"\nErrore: {error}");
            return;
        }

        if (wallets is null || wallets.Count == 0)
        {
            Console.WriteLine("\nNessun conto associato al tuo profilo");
            return;
        }

        Console.WriteLine("\nELENCO CONTI DISPONIBILI");
        for (int i = 0; i < wallets.Count; i++)
        {
            var wallet = wallets[i];
            Console.WriteLine($"{i + 1}. {wallet.OwnerName,-20} | IBAN: {wallet.Iban} | Saldo: {wallet.Balance:F2} {wallet.Currency}");
        }
    }

    private async Task ShowWalletDetailsAsync()
    {
        var wallet = await SelectOwnWalletAsync();

        if (wallet is null)
        {
            Console.WriteLine("\nOperazione annullata: nessun conto selezionato");
            return;
        }

        Console.WriteLine("\nDETTAGLIO CONTO");
        Console.WriteLine($"Titolare: {wallet.OwnerName}");
        Console.WriteLine($"IBAN: {wallet.Iban}");
        Console.WriteLine($"Valuta: {wallet.Currency}");
        Console.WriteLine($"Saldo: {wallet.Balance:F2} {wallet.Currency}");
        Console.WriteLine($"Creato il: {wallet.CreatedAt.ToLocalTime():dd/MM/yyyy HH:mm}");
    }

    private async Task CreateWalletAsync()
    {
        Console.Write("\nInserisci il nome e cognome del titolare del conto: ");
        var ownerName = EnterString();

        Console.WriteLine("\nSeleziona la valuta del conto:");
        Console.WriteLine("1. EUR");
        Console.WriteLine("2. CHF");
        Console.Write("Scelta: ");
        var currencyChoice = EnterInt(1, 2);
        var currency = currencyChoice == 1 ? Currency.EUR : Currency.CHF;

        var (success, newWallet, message) = await apiClient.CreateWalletAsync(ownerName, currency);
        if (!success || newWallet is null)
        {
            Console.WriteLine($"\nErrore durante l'apertura del conto: {message}");
            return;
        }

        Console.WriteLine("\nConto aperto con successo");
        Console.WriteLine($"IBAN generato: {newWallet.Iban}");
        Console.WriteLine($"Valuta: {newWallet.Currency}");
    }

    private async Task DepositFundsAsync()
    {
        var wallet = await SelectOwnWalletAsync();

        if (wallet is null)
        {
            Console.WriteLine("\nOperazione annullata: nessun conto selezionato");
            return;
        }

        Console.Write("Inserisci l'importo da depositare: ");
        var amount = EnterDecimal();

        Console.Write("Inserisci la causale del deposito: ");
        var description = Console.ReadLine()?.Trim() ?? string.Empty;

        var (success, updatedWallet, message) = await apiClient.DepositAsync(wallet.Id, amount, description);
        if (!success || updatedWallet is null)
        {
            Console.WriteLine($"\nImpossibile effettuare il deposito: {message}");
            return;
        }

        Console.WriteLine($"\nDeposito eseguito con successo, il nuovo saldo è: {updatedWallet.Balance:F2} {updatedWallet.Currency}");
    }

    private async Task TransferFundsAsync()
    {
        var sourceWallet = await SelectOwnWalletAsync();
        if (sourceWallet is null)
        {
            Console.WriteLine("\nOperazione annullata: nessun conto selezionato");
            return;
        }
        var sourceId = sourceWallet.Id;

        Console.Write("Inserisci l'ID del conto a cui inviare il denaro: ");
        var destId = EnterInt(1);

        Console.Write("Inserisci l'importo del bonifico: ");
        var amount = EnterDecimal();

        Console.Write("Inserisci la causale del bonifico: ");
        var description = Console.ReadLine()?.Trim() ?? string.Empty;

        var (success, transferResult, message) = await apiClient.TransferAsync(sourceId, destId, amount, description);

        if (success)
        {
            Console.WriteLine($"\n{message}");

            if (transferResult is null)
                return;

            if (transferResult.SourceCurrency != transferResult.DestinationCurrency)
                Console.WriteLine($"Importo inviato: {transferResult.AmountSent:F2} {transferResult.SourceCurrency} (tasso: {transferResult.ExchangeRate:F4})");
            else
                Console.WriteLine($"Importo inviato: {transferResult.AmountSent:F2} {transferResult.SourceCurrency}");

            Console.WriteLine($"Importo ricevuto: {transferResult.AmountReceived:F2} {transferResult.DestinationCurrency}");
            Console.WriteLine($"Saldo residuo sul conto mittente: {transferResult.SourceBalance:F2} {transferResult.SourceCurrency}");
        }
        else
            Console.WriteLine($"\nErrore durante il bonifico: {message}");
    }

    private async Task ShowTransactionsAsync()
    {
        var wallet = await SelectOwnWalletAsync();

        if (wallet is null)
        {
            Console.WriteLine("\nOperazione annullata: nessun conto selezionato");
            return;
        }

        var (transactions, error) = await apiClient.GetTransactionsAsync(wallet.Id);
        if (error is not null)
        {
            Console.WriteLine($"\nErrore: {error}");
            return;
        }

        if (transactions is null || transactions.Count == 0)
        {
            Console.WriteLine("\nNessuna transazione trovata per il conto selezionato");
            return;
        }

        Console.WriteLine("\nESTRATTO CONTO");
        foreach (var transaction in transactions)
        {
            var sign = transaction.Type == TransactionType.BonificoInUscita ? "-" : "+";
            Console.WriteLine($"• {transaction.Timestamp.ToLocalTime():dd/MM/yyyy HH:mm} | [{transaction.Type,-17}] | {sign}{transaction.Amount,8:F2} | {transaction.Description}");
        }
    }

    private async Task<WalletResponse?> SelectOwnWalletAsync()
    {
        var (wallets, error) = await apiClient.GetAllWalletsAsync();
        if (error is not null)
        {
            Console.WriteLine($"\nErrore: {error}");
            return null;
        }

        if (wallets is null || wallets.Count == 0)
        {
            Console.WriteLine("\nNessun conto associato al tuo profilo");
            return null;
        }

        Console.WriteLine("\nSeleziona il conto:");
        for (int i = 0; i < wallets.Count; i++)
        {
            var w = wallets[i];
            Console.WriteLine($"{i + 1}. Conto {w.Currency} | Saldo: {w.Balance:F2} {w.Currency} | IBAN: {w.Iban}");
        }

        Console.Write("Scelta: ");
        var choice = EnterInt(1, wallets.Count);
        return wallets[choice - 1];
    }

    private static string EnterString()
    {
        var input = Console.ReadLine()?.Trim() ?? string.Empty;
        while (string.IsNullOrWhiteSpace(input))
        {
            Console.WriteLine("Il valore inserito non può essere vuoto");
            Console.Write("Nuovo valore: ");
            input = Console.ReadLine()?.Trim() ?? string.Empty;
        }

        return input;
    }

    private static string EnterPassword()
    {
        var input = Console.ReadLine() ?? string.Empty;
        while (string.IsNullOrWhiteSpace(input))
        {
            Console.WriteLine("Il valore inserito non può essere vuoto");
            Console.Write("Nuovo valore: ");
            input = Console.ReadLine() ?? string.Empty;
        }

        return input;
    }

    // Normalizes decimal input to guarantee reliable parsing regardless of operating system culture
    private static decimal EnterDecimal()
    {
        while (true)
        {
            var rawInput = Console.ReadLine()?.Trim() ?? string.Empty;
            var normalizedInput = rawInput.Replace(',', '.');

            if (decimal.TryParse(normalizedInput, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var parsed))
            {
                // Rounds to two decimals to represent currency cents accurately
                var amount = Math.Round(parsed, 2, MidpointRounding.AwayFromZero);
                if (amount > 0)
                    return amount;
            }

            Console.WriteLine("Valore inserito non valido");
            Console.Write("Nuovo valore: ");
        }
    }

    private static int EnterInt(int min, int max = int.MaxValue)
    {
        int value;
        while (!int.TryParse(Console.ReadLine(), out value) || value < min || value > max)
        {
            Console.WriteLine("Valore inserito non valido");
            Console.Write("Nuovo valore: ");
        }

        return value;
    }
}