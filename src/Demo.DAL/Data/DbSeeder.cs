using Demo.Domain.Entities;
using Demo.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Demo.DAL.Data;

/// <summary>Applies migrations and seeds demo data (admin user + sample products).</summary>
public static class DbSeeder
{
    public const string DemoAdminEmail = "admin@demo.com";
    public const string DemoAdminPassword = "Admin123!";

    public static async Task SeedAsync(AppDbContext db, IPasswordHasher hasher, CancellationToken ct = default)
    {
        // Relational providers (SQLite) apply migrations; non-relational (InMemory, used
        // in integration tests) just ensure the schema exists.
        if (db.Database.IsRelational())
            await db.Database.MigrateAsync(ct);
        else
            await db.Database.EnsureCreatedAsync(ct);

        if (!await db.Users.AnyAsync(ct))
        {
            var admin = User.Create(DemoAdminEmail, hasher.Hash(DemoAdminPassword), Roles.Admin);
            await db.Users.AddAsync(admin, ct);
        }

        if (!await db.Products.AnyAsync(ct))
        {
            var products = new[]
            {
                Product.Create("Wireless Mouse", "SKU-MOUSE-01", 19.99m, 120, "Peripherals"),
                Product.Create("Mechanical Keyboard", "SKU-KEYB-01", 79.50m, 45, "Peripherals"),
                Product.Create("27\" 4K Monitor", "SKU-MON-27", 329.00m, 18, "Monitors"),
                Product.Create("USB-C Hub", "SKU-HUB-7P", 42.00m, 60, "Accessories"),
                Product.Create("Laptop Stand", "SKU-STAND-AL", 34.90m, 75, "Accessories"),
            };
            await db.Products.AddRangeAsync(products, ct);
        }

        await db.SaveChangesAsync(ct);
    }
}
