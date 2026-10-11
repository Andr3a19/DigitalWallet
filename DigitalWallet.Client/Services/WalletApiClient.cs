using System.Net.Http.Json;
using DigitalWallet.Client.Models;

namespace DigitalWallet.Client.Services
{
    // HTTP client service handling network communication with the DigitalWallet.Api backend
    internal class WalletApiClient
    {
        private readonly HttpClient _httpClient;

        public WalletApiClient()
        {
            _httpClient = new HttpClient { BaseAddress = new Uri("https://localhost:7164") };
        }

        // Sends a GET request to retrieve all registered wallets
        public async Task<List<WalletResponse>> GetAllWalletsAsync()
        {
            return await _httpClient.GetFromJsonAsync<List<WalletResponse>>("api/wallets") ?? [];
        }

        // Sends a GET request to retrieve a single wallet by ID, safely returning null if not found
        public async Task<WalletResponse?> GetWalletByIdAsync(int id)
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<WalletResponse>($"api/wallets/{id}");
            } catch (HttpRequestException)
            {
                return null;
            }
        }

        // Sends a POST request to create a new wallet and returns the created resource
        public async Task<WalletResponse?> CreateWalletAsync(string ownerName)
        {
            var response = await _httpClient.PostAsJsonAsync("api/wallets", new { ownerName });

            if (response.IsSuccessStatusCode)
                return await response.Content.ReadFromJsonAsync<WalletResponse>();

            return null;
        }

        // Sends a POST request to deposit funds into a specific wallet
        public async Task<WalletResponse?> DepositAsync(int id, decimal amount, string? description)
        {
            var response = await _httpClient.PostAsJsonAsync($"api/wallets/{id}/deposit", new { amount, description });

            if (response.IsSuccessStatusCode)
                return await response.Content.ReadFromJsonAsync<WalletResponse>();

            return null;
        }

        // Sends a POST request to execute an atomic peer-to-peer transfer, returning success status and message
        public async Task<(bool Success, string Message)> TransferAsync(int sourceId, int destId, decimal amount, string? description)
        {
            var response = await _httpClient.PostAsJsonAsync(
                "api/wallets/transfer",
                new { SourceWalletId = sourceId, DestinationWalletId = destId, Amount = amount, Description = description }
            );

            if (response.IsSuccessStatusCode)
                return (true, "Bonifico completato con successo!");

            var message = await response.Content.ReadAsStringAsync();
            return (false, message);
        }

        // Sends a GET request to retrieve all transactions for a specific wallet ordered chronologically
        public async Task<List<TransactionResponse>> GetTransactionsAsync(int id)
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<List<TransactionResponse>>($"api/wallets/{id}/transactions") ?? [];
            } catch (HttpRequestException)
            {
                return [];
            }
        }
    }
}