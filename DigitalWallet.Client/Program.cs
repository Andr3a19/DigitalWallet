using DigitalWallet.Client.Services;
using DigitalWallet.Client.Views;
using System.Text;

namespace DigitalWallet.Client;

internal static class Program
{
    private static async Task Main()
    {
        Console.OutputEncoding = Encoding.UTF8;
        Console.InputEncoding = Encoding.UTF8;

        var apiClient = new WalletApiClient();
        var view = new ClientView(apiClient);

        await view.StartAsync();
    }
}