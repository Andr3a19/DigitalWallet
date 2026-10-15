using DigitalWallet.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace DigitalWallet.Api.Data
{
    // Entity Framework database context managing SQLite session, mapping Wallets, Transactions, and Users tables
    public class WalletDbContext(DbContextOptions<WalletDbContext> options) : DbContext(options)
    {
        public DbSet<Wallet> Wallets { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<User> Users { get; set; }
    }
}