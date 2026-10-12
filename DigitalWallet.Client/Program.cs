using DigitalWallet.Client.Services;
using DigitalWallet.Client.Views;

namespace DigitalWallet.Client
{
    class Program
    {
        static async Task Main()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            var apiClient = new WalletApiClient();
            var view = new ClientView(apiClient);

            await view.StartAsync();
        }
    }
}