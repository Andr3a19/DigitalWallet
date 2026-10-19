namespace DigitalWallet.Api.Models;

public class User
{
    public int Id { get; init; }
    public string Username { get; private set; } = string.Empty;
    public string FullName { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public Role Role { get; private set; } = Role.Customer;
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;

    private readonly List<Wallet> _wallets = [];
    public IReadOnlyCollection<Wallet> Wallets => _wallets.AsReadOnly();

    public User(string username, string fullName, string passwordHash, Role role = Role.Customer)
    {
        if (string.IsNullOrWhiteSpace(username))
            throw new ArgumentException("Lo username non può essere vuoto", nameof(username));

        if (string.IsNullOrWhiteSpace(fullName))
            throw new ArgumentException("Il nome completo non può essere vuoto", nameof(fullName));

        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new ArgumentException("L'hash della password non può essere vuoto", nameof(passwordHash));

        Username = username.Trim().ToLowerInvariant();
        FullName = fullName.Trim();
        PasswordHash = passwordHash;
        Role = role;
    }

    protected User() { }
}