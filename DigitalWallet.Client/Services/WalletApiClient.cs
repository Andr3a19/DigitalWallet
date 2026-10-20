using DigitalWallet.Client.Models;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace DigitalWallet.Client.Services;

public class WalletApiClient
{
    private readonly HttpClient _httpClient;
    public string? CurrentUserFullName { get; private set; }
    public string? CurrentUsername { get; private set; }

    public WalletApiClient()
    {
        _httpClient = new HttpClient { BaseAddress = new Uri("https://localhost:7164") };
    }

    // Maps authentication credentials against the API and stores the returned JWT token in memory
    public async Task<(bool Success, string Message)> LoginAsync(string username, string password)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/auth/login", new { username, password });
            if (response.IsSuccessStatusCode)
            {
                var auth = await response.Content.ReadFromJsonAsync<AuthResponse>();
                if (auth is not null)
                {
                    SetAuthToken(auth.Token);
                    CurrentUserFullName = auth.FullName;
                    CurrentUsername = auth.Username;
                    return (true, "Autenticazione riuscita");
                }
            }

            var error = await response.Content.ReadAsStringAsync();
            return (false, string.IsNullOrWhiteSpace(error) ? "Credenziali non valide" : error);
        }
        catch (Exception ex)
        {
            return (false, $"Errore di connessione al server: {ex.Message}");
        }
    }

    public async Task<(bool Success, string Message)> RegisterAsync(string username, string fullName, string password)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/auth/register", new { username, fullName, password });
            if (response.IsSuccessStatusCode)
                return (true, "Registrazione completata con successo");

            var error = await response.Content.ReadAsStringAsync();
            return (false, string.IsNullOrWhiteSpace(error) ? "Errore durante la registrazione" : error);
        }
        catch (Exception ex)
        {
            return (false, $"Errore di connessione al server: {ex.Message}");
        }
    }

    // Attaches the cryptographically signed JWT token to the default Authorization header for all subsequent requests
    public void SetAuthToken(string token)
    {
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    public async Task<List<WalletResponse>> GetAllWalletsAsync()
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<List<WalletResponse>>("api/wallets") ?? [];
        }
        catch (HttpRequestException)
        {
            return [];
        }
    }

    public async Task<WalletResponse?> GetWalletByIdAsync(int id)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<WalletResponse>($"api/wallets/{id}");
        }
        catch (HttpRequestException)
        {
            return null;
        }
    }

    public async Task<(bool Success, WalletResponse? Wallet, string Message)> CreateWalletAsync(string ownerName, Currency currency)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/wallets", new { ownerName, currency });
            if (response.IsSuccessStatusCode)
            {
                var wallet = await response.Content.ReadFromJsonAsync<WalletResponse>();
                return (true, wallet, "Conto aperto con successo");
            }

            // Extracts detailed backend validation error message when the HTTP request fails
            var error = await response.Content.ReadAsStringAsync();
            return (false, null, error);
        }
        catch (Exception ex)
        {
            return (false, null, $"Errore di comunicazione: {ex.Message}");
        }
    }

    public async Task<(bool Success, WalletResponse? Wallet, string Message)> DepositAsync(int id, decimal amount, string? description)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync($"api/wallets/{id}/deposit", new { amount, description });
            if (response.IsSuccessStatusCode)
            {
                var wallet = await response.Content.ReadFromJsonAsync<WalletResponse>();
                return (true, wallet, "Deposito eseguito con successo");
            }

            var error = await response.Content.ReadAsStringAsync();
            return (false, null, error);
        }
        catch (Exception ex)
        {
            return (false, null, $"Errore di comunicazione: {ex.Message}");
        }
    }

    public async Task<(bool Success, string Message)> TransferAsync(int sourceId, int destId, decimal amount, string? description)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/wallets/transfer", new
            {
                SourceWalletId = sourceId,
                DestinationWalletId = destId,
                Amount = amount,
                Description = description
            });

            if (response.IsSuccessStatusCode)
                return (true, "Bonifico completato con successo");

            var message = await response.Content.ReadAsStringAsync();
            return (false, message);
        }
        catch (Exception ex)
        {
            return (false, $"Errore di comunicazione: {ex.Message}");
        }
    }

    public async Task<List<TransactionResponse>> GetTransactionsAsync(int id)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<List<TransactionResponse>>($"api/wallets/{id}/transactions") ?? [];
        }
        catch (HttpRequestException)
        {
            return [];
        }
    }
}