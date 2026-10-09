using System.Net.Http.Json;
using DigitalWallet.Client.Models;

namespace DigitalWallet.Client.Services
{
    internal class WalletApiClient
    {
        private readonly HttpClient _httpClient;

        public WalletApiClient()
        {
            _httpClient = new HttpClient { BaseAddress = new Uri("https://localhost:7164") };
        }

        public async Task<List<WalletResponse>> GetAllWalletsAsync()
        {
            return await _httpClient.GetFromJsonAsync<List<WalletResponse>>("api/wallets") ?? [];
        }
    }
}