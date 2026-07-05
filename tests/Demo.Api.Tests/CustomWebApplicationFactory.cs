using Demo.DAL.Data;
using Demo.Services.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Demo.Api.Tests;

/// <summary>
/// Spins up the real API in-process, swapping the SQLite database for an isolated
/// EF Core in-memory store and seeding the demo data.
/// </summary>
public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _dbName = $"tests-{Guid.NewGuid()}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            // Remove every descriptor tied to the SQLite-backed AppDbContext, including
            // the EF Core 9+ IDbContextOptionsConfiguration<AppDbContext> options config.
            var toRemove = services.Where(d =>
                d.ServiceType == typeof(AppDbContext) ||
                d.ServiceType == typeof(DbContextOptions) ||
                (d.ServiceType.FullName?.Contains(nameof(AppDbContext)) ?? false)).ToList();
            foreach (var descriptor in toRemove)
                services.Remove(descriptor);

            services.AddDbContext<AppDbContext>(options => options.UseInMemoryDatabase(_dbName));
        });
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        var host = base.CreateHost(builder);

        using var scope = host.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        DbSeeder.SeedAsync(db, hasher).GetAwaiter().GetResult();

        return host;
    }
}
