using Microsoft.EntityFrameworkCore;
using MoneyTransfer.Api.Models;

namespace MoneyTransfer.Api.Data;

/// <summary>
/// EF Core database context for the money transfer domain.
/// Add new <see cref="DbSet{TEntity}"/> properties here as the domain grows.
/// </summary>
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<Transaction> Transactions => Set<Transaction>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Account>(entity =>
        {
            entity.HasKey(a => a.Id);
            entity.Property(a => a.AccountName).IsRequired();
        });

        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.HasKey(t => t.Id);

            // Stored as a string (e.g. "Success"/"Failed") for readability in the database.
            entity.Property(t => t.Status)
                .HasConversion<string>()
                .HasMaxLength(20);

            // Two FKs to the same Accounts table would create multiple cascade
            // paths in SQL Server, so cascading delete is disabled on both.
            // Accounts are not expected to be deleted while transactions reference them.
            entity.HasOne(t => t.FromAccount)
                .WithMany()
                .HasForeignKey(t => t.FromAccountId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(t => t.ToAccount)
                .WithMany()
                .HasForeignKey(t => t.ToAccountId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Seed a couple of accounts so the API is immediately testable in development.
        modelBuilder.Entity<Account>().HasData(
            new Account { Id = 1, AccountName = "Alice", Balance = 1000m },
            new Account { Id = 2, AccountName = "Bob", Balance = 500m }
        );
    }
}
