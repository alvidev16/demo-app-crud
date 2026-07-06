using Demo.Domain.Entities;
using Demo.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace Demo.DAL.Data;

/// <summary>EF Core context backing the SQLite database.</summary>
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Product> Products => Set<Product>();
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Name).IsRequired().HasMaxLength(100);
            // Value-object conversions: stored as their primitive column type.
            entity.Property(p => p.Sku)
                  .HasConversion(sku => sku.Value, value => Sku.Create(value))
                  .IsRequired().HasMaxLength(50);
            entity.Property(p => p.Price)
                  .HasConversion(money => money.Amount, amount => Money.Create(amount))
                  .HasColumnType("decimal(18,2)");
            entity.Property(p => p.Category).IsRequired().HasMaxLength(50);
            entity.HasIndex(p => p.Sku).IsUnique();
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(u => u.Id);
            entity.Property(u => u.Email)
                  .HasConversion(email => email.Value, value => Email.Create(value))
                  .IsRequired().HasMaxLength(256);
            entity.Property(u => u.PasswordHash).IsRequired();
            entity.Property(u => u.Role).IsRequired().HasMaxLength(20);
            entity.HasIndex(u => u.Email).IsUnique();
        });
    }
}
