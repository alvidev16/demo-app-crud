using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Tasks.Api.Auth;
using Tasks.DAL;

namespace Tasks.Api.Tests;

/// <summary>Runs the real API in-process over an isolated in-memory database.</summary>
public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _dbName = $"tests-{Guid.NewGuid()}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureServices(services =>
        {
            var toRemove = services.Where(d =>
                d.ServiceType == typeof(TasksDbContext) ||
                d.ServiceType == typeof(DbContextOptions) ||
                (d.ServiceType.FullName?.Contains(nameof(TasksDbContext)) ?? false)).ToList();
            foreach (var d in toRemove) services.Remove(d);

            services.AddDbContext<TasksDbContext>(o => o.UseInMemoryDatabase(_dbName));
        });
    }

    /// <summary>Mints a valid JWT for the given user, using the app's own token service.</summary>
    public string TokenFor(Guid userId)
        => Services.GetRequiredService<JwtTokenService>().CreateToken(userId);
}
