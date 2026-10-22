using DigitalWallet.Client.DTOs;
using DigitalWallet.Client.Models;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace DigitalWallet.Client.Services;

public class WalletApiClient
{
    private readonly HttpClient _httpClient;
    public string? CurrentUserFullName { get; private set; }
    public string? CurrentUsername { get; private set; }

    private const string DefaultBaseAddress = "https://localhost:7164";

    public WalletApiClient(string baseUrl)
    {
        _httpClient = new HttpClient { BaseAddress = new Uri(baseUrl) };
    }

    public WalletApiClient() : this(DefaultBaseAddress) { }

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

            return (false, await ExtractErrorAsync(response));
        }
        catch (HttpRequestException ex)
        {
            return (false, $"Server non raggiungibile: {ex.Message}");
        }
        catch (JsonException ex)
        {
            return (false, $"Risposta non valida dal server: {ex.Message}");
        }
    }

    public async Task<(bool Success, string Message)> RegisterAsync(string username, string fullName, string password)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/auth/register", new { username, fullName, password });
            if (response.IsSuccessStatusCode)
                return (true, "Registrazione completata con successo");

            return (false, await ExtractErrorAsync(response));
        }
        catch (HttpRequestException ex)
        {
            return (false, $"Server non raggiungibile: {ex.Message}");
        }
        catch (JsonException ex)
        {
            return (false, $"Risposta non valida dal server: {ex.Message}");
        }
    }

    // Attaches the cryptographically signed JWT token to the default Authorization header for all subsequent requests
    public void SetAuthToken(string token)
    {
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    public async Task<(List<WalletResponse>? Data, string? Error)> GetAllWalletsAsync()
        => await GetAsync<List<WalletResponse>>("api/wallets");

    public async Task<(WalletResponse? Data, string? Error)> GetWalletByIdAsync(int id)
        => await GetAsync<WalletResponse>($"api/wallets/{id}");

    public async Task<(List<TransactionResponse>? Data, string? Error)> GetTransactionsAsync(int id)
        => await GetAsync<List<TransactionResponse>>($"api/wallets/{id}/transactions");

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

            return (false, null, await ExtractErrorAsync(response));
        }
        catch (HttpRequestException ex)
        {
            return (false, null, $"Errore di comunicazione: {ex.Message}");
        }
        catch (JsonException ex)
        {
            return (false, null, $"Risposta non valida dal server: {ex.Message}");
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

            return (false, null, await ExtractErrorAsync(response));
        }
        catch (HttpRequestException ex)
        {
            return (false, null, $"Errore di comunicazione: {ex.Message}");
        }
        catch (JsonException ex)
        {
            return (false, null, $"Risposta non valida dal server: {ex.Message}");
        }
    }

    public async Task<(bool Success, TransferResponse? Data, string Message)> TransferAsync(int sourceId, int destId, decimal amount, string? description)
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

            if (!response.IsSuccessStatusCode)
                return (false, null, await ExtractErrorAsync(response));

            var result = await response.Content.ReadFromJsonAsync<TransferResponse>();
            return (true, result, "Bonifico completato con successo");
        }
        catch (HttpRequestException ex)
        {
            return (false, null, $"Errore di comunicazione: {ex.Message}");
        }
        catch (JsonException ex)
        {
            return (false, null, $"Risposta non valida dal server: {ex.Message}");
        }
    }

    private async Task<(T? Data, string? Error)> GetAsync<T>(string url)
    {
        try
        {
            var data = await _httpClient.GetFromJsonAsync<T>(url);
            return (data, null);
        }
        catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.Unauthorized)
        {
            return (default, "Sessione scaduta o non valida, effettua di nuovo il login");
        }
        catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.Forbidden)
        {
            return (default, "Non sei autorizzato ad accedere a questa risorsa");
        }
        catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            // Caller handles not found responses with a custom message
            return (default, null);
        }
        catch (HttpRequestException ex)
        {
            // StatusCode can be null when the server is unreachable or the connection fails
            var statusCode = ex.StatusCode?.ToString() ?? "rete non disponibile";
            return (default, $"Errore di rete ({statusCode}): {ex.Message}");
        }
        catch (JsonException ex)
        {
            return (default, $"Risposta non valida dal server: {ex.Message}");
        }
    }

    // Parses ASP.NET Core ProblemDetails to extract a user-friendly error message
    private static async Task<string> ExtractErrorAsync(HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync();
        if (string.IsNullOrWhiteSpace(content))
        {
            // Provides meaningful fallback messages for empty challenge responses
            if (response.StatusCode == HttpStatusCode.Forbidden)
                return "Non sei autorizzato ad accedere a questa risorsa";

            if (response.StatusCode == HttpStatusCode.Unauthorized)
                return "Sessione scaduta o non valida, effettua di nuovo il login";

            return $"Errore HTTP {(int)response.StatusCode}";
        }

        try
        {
            var problem = JsonSerializer.Deserialize<ProblemDetailsResponse>(content, JsonSerializerOptions.Web);

            if (problem?.Detail != null)
                return problem.Detail;

            if (problem?.Title != null)
                return problem.Title;
        }
        catch (JsonException)
        {
            // Not a ProblemDetails payload, fall through and return the raw content
        }

        return content;
    }

    private sealed record ProblemDetailsResponse(string? Title, string? Detail);
}