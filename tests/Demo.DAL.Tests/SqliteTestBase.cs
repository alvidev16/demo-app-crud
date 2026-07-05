using Demo.DAL.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Demo.DAL.Tests;

/// <summary>
/// Base class that spins up a real SQLite database in memory. Unlike the EF Core
/// InMemory provider, this exercises actual SQL translation, unique indexes and
/// relational constraints. The connection is kept open so the schema survives.
/// </summary>
public abstract class SqliteTestBase : IDisposable
{
    private readonly SqliteConnection _connection;
    protected readonly AppDbContext Db;

    protected SqliteTestBase()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_connection)
            .Options;

        Db = new AppDbContext(options);
        Db.Database.EnsureCreated();
    }

    /// <summary>Returns a fresh context over the same database to avoid identity-map caching.</summary>
    protected AppDbContext NewContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_connection)
            .Options;
        return new AppDbContext(options);
    }

    public void Dispose()
    {
        Db.Dispose();
        _connection.Dispose();
        GC.SuppressFinalize(this);
    }
}
