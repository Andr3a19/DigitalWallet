using DigitalWallet.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace DigitalWallet.Api.Data
{
    // Entity Framework database context managing the SQLite database session, mapping Wallets and Transactions tables
    public class WalletDbContext(DbContextOptions<WalletDbContext> options) : DbContext(options)
    {
        public DbSet<Wallet> Wallets { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
    }
}