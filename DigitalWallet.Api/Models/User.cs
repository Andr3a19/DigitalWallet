namespace DigitalWallet.Api.Models
{
    // Roles defining permissions for Role-Based Access Control (RBAC)
    public enum UserRole
    {
        Customer,
        Admin
    }

    // Domain entity representing a registered system user
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public UserRole Role { get; set; } = UserRole.Customer;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property: one user can own multiple wallets (1-to-many relationship)
        public List<Wallet> Wallets { get; set; } = [];

        public User(string username, string fullName, string passwordHash, UserRole role = UserRole.Customer)
        {
            Username = username;
            FullName = fullName;
            PasswordHash = passwordHash;
            Role = role;
        }

        public User() { }
    }
}