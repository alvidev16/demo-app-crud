using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Demo.DAL.Data;

/// <summary>
/// Enables EF Core CLI tooling (migrations) to instantiate the context at design
/// time without needing the web host to be running.
/// </summary>
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite("Data Source=demo.db")
            .Options;

        return new AppDbContext(options);
    }
}
