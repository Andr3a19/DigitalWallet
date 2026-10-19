using DigitalWallet.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace DigitalWallet.Api.Data;

public class WalletDbContext(DbContextOptions<WalletDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Wallet> Wallets => Set<Wallet>();
    public DbSet<Transaction> Transactions => Set<Transaction>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(u => u.Username).IsUnique();
            entity.Property(u => u.Username).HasMaxLength(50).IsRequired();
            entity.Property(u => u.FullName).HasMaxLength(100).IsRequired();
            entity.Property(u => u.PasswordHash).IsRequired();
        });

        modelBuilder.Entity<Wallet>(entity =>
        {
            entity.HasIndex(w => w.Iban).IsUnique();
            entity.Property(w => w.Iban).HasMaxLength(34).IsRequired();
            entity.Property(w => w.Balance).HasPrecision(18, 2);
            entity.Property(w => w.ConcurrencyToken).IsConcurrencyToken();

            entity.HasOne(w => w.User)
                  .WithMany(u => u.Wallets)
                  .HasForeignKey(w => w.UserId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.Property(t => t.Amount).HasPrecision(18, 2);
        });
    }
}