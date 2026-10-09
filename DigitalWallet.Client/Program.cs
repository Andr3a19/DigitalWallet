using DigitalWallet.Client.Services;

namespace DigitalWallet.Client
{
    class Program
    {
        static async Task Main()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            Console.WriteLine("Digital wallet client");
            Console.WriteLine("Contattando il server backend via HTTP...\n");

            var apiClient = new WalletApiClient();
            var wallets = await apiClient.GetAllWalletsAsync();

            Console.WriteLine($"Risposta ricevuta dal Server! Conti trovati: {wallets.Count}\n");

            foreach (var wallet in wallets)
            {
                Console.WriteLine($"• [ID: {wallet.Id}] {wallet.OwnerName,-20} | Saldo: {wallet.Balance:C}");
            }

            Console.WriteLine("\nPremi un tasto per chiudere...");
            Console.ReadKey();
        }
    }
}