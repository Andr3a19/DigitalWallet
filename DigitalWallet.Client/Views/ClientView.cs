using DigitalWallet.Client.Models;
using DigitalWallet.Client.Services;

namespace DigitalWallet.Client.Views
{
    internal class ClientView(WalletApiClient apiClient)
    {
        public async Task StartAsync()
        {
            bool isRunning = true;
            int choice;

            while (isRunning)
            {
                Console.WriteLine("\n1. Mostra tutti i conti");
                Console.WriteLine("2. Dettaglio singolo conto (per ID)");
                Console.WriteLine("3. Apri nuovo conto");
                Console.WriteLine("4. Deposita fondi");
                Console.WriteLine("5. Effettua bonifico P2P");
                Console.WriteLine("6. Estratto conto (storico movimenti)");
                Console.WriteLine("0. Esci");
                Console.Write("\nSeleziona un'opzione valida: ");
                choice = EnterInt(0, 6);

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

        // Retrieves and displays all registered wallets from the backend API
        private async Task ShowAllWalletsAsync()
        {
            var wallets = await apiClient.GetAllWalletsAsync();
            if (wallets.Count == 0)
            {
                Console.WriteLine("\nNessun conto registrato nel sistema");
                return;
            }

            Console.WriteLine("\nELENCO CONTI DISPONIBILI");
            foreach (var wallet in wallets)
                Console.WriteLine($"• [ID: {wallet.Id}] {wallet.OwnerName,-20} | Saldo: {wallet.Balance:C}");
        }

        // Fetches and displays detailed information for a specific wallet by its primary key
        private async Task ShowWalletDetailsAsync()
        {
            Console.Write("\nInserisci l'ID del conto: ");
            int id = EnterInt(1);

            var wallet = await apiClient.GetWalletByIdAsync(id);
            if (wallet == null)
            {
                Console.WriteLine($"\nNessun conto trovato con ID {id}");
                return;
            }

            Console.WriteLine($"[ID: {wallet.Id}] {wallet.OwnerName,-20} | Saldo: {wallet.Balance:C} | Creato: {wallet.CreatedAt.ToLocalTime()}");
        }

        // Prompts the user for owner details and requests a new wallet creation via HTTP POST
        private async Task CreateWalletAsync()
        {
            Console.Write("\nInserisci il nome e cognome del titolare: ");
            string ownerName = Console.ReadLine() ?? "";
            while (string.IsNullOrWhiteSpace(ownerName))
            {
                Console.WriteLine("Il nome e cognome inserito non è valido");
                Console.Write("Nuovo valore: ");
                ownerName = Console.ReadLine() ?? "";
            }

            var newWallet = await apiClient.CreateWalletAsync(ownerName);
            if (newWallet == null)
            {
                Console.WriteLine("\nErrore durante l'apertura del conto sul server");
                return;
            }

            Console.WriteLine($"\nConto aperto con successo, ID assegnato è : {newWallet.Id}");
        }

        // Prompts for wallet ID and amount, then dispatches a deposit request to the API
        private async Task DepositFundsAsync()
        {
            Console.Write("\nInserisci l'ID del conto su cui depositare: ");
            int id = EnterInt(1);

            Console.Write("Inserisci l'importo da depositare: ");
            decimal amount = EnterDecimal();

            Console.Write("Inserisci la causale: ");
            string description = Console.ReadLine() ?? "";

            var updatedWallet = await apiClient.DepositAsync(id, amount, description);
            if (updatedWallet == null)
            {
                Console.WriteLine($"\nImpossibile effettuare il deposito, il conto ID {id} non trovato");
                return;
            }

            Console.WriteLine($"\nDeposito eseguito con successo, il nuovo saldo è: {updatedWallet.Balance:C}");
        }

        // Coordinates a peer-to-peer transfer between two accounts and prints server response messages
        private async Task TransferFundsAsync()
        {
            Console.Write("\nInserisci l'ID del conto da cui prelevare: ");
            int sourceId = EnterInt(1);

            Console.Write("Inserisci l'ID del conto a cui inviare il denaro: ");
            int destId = EnterInt(1);

            Console.Write("Inserisci l'importo del bonifico: ");
            decimal amount = EnterDecimal();

            Console.Write("Inserisci la causale del bonifico: ");
            string description = Console.ReadLine() ?? "";

            var (success, message) = await apiClient.TransferAsync(sourceId, destId, amount, description);

            if (success)
                Console.WriteLine($"\n{message}");
            else
                Console.WriteLine($"\nErrore: {message}");
        }

        // Retrieves and formats the full chronological transaction history for a specific wallet
        private async Task ShowTransactionsAsync()
        {
            Console.Write("\nInserisci l'ID del conto di cui vuoi visualizzare l'estratto conto: ");
            int id = EnterInt(1);

            var transactions = await apiClient.GetTransactionsAsync(id);

            if (transactions.Count == 0)
            {
                Console.WriteLine($"\nNessuna transazione trovata per il conto ID {id}");
                return;
            }

            Console.WriteLine("\nESTRATTO CONTO");
            foreach (var transaction in transactions)
            {
                string sign = transaction.Type == TransactionType.BonificoInUscita ? "-" : "+";
                Console.WriteLine($"• {transaction.TimeStamp.ToLocalTime():dd/MM/yyyy HH:mm} | [{transaction.Type,-17}] | {sign}{transaction.Amount,8:C} | {transaction.Description}");
            }
        }

        // Helper method ensuring positive decimal input with support for comma and dot decimal separators
        private static decimal EnterDecimal()
        {
            decimal amount;
            // Handle numpad input by converting dots to commas for reliable decimal parsing
            string input = Console.ReadLine()?.Replace('.', ',') ?? "";
            while (!decimal.TryParse(input, out amount) || amount <= 0)
            {
                Console.WriteLine("Valore inserito non è valido");
                Console.Write("Nuovo valore: ");
                input = Console.ReadLine()?.Replace('.', ',') ?? "";
            }

            return amount;
        }

        // Helper method enforcing bounded integer input within a defined range
        private static int EnterInt(int min, int max = int.MaxValue)
        {
            int value;
            while (!int.TryParse(Console.ReadLine(), out value) || value < min || value > max)
            {
                Console.WriteLine("Valore inserito non è valido");
                Console.Write("Nuovo valore: ");
            }

            return value;
        }
    }
}