using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Tasks.DAL;
using Tasks.Domain.Entities;

namespace Tasks.Api.Tests;

/// <summary>
/// Repository round-trip against a REAL SQLite database in memory (T-09 DoD) — exercises
/// actual SQL translation and the enum-to-string conversion, not the InMemory provider.
/// </summary>
public class TaskRepositoryTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly TasksDbContext _db;
    private static readonly DateOnly Today = new(2026, 1, 1);

    public TaskRepositoryTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
        var options = new DbContextOptionsBuilder<TasksDbContext>().UseSqlite(_connection).Options;
        _db = new TasksDbContext(options);
        _db.Database.EnsureCreated();
    }

    private TasksDbContext NewContext() =>
        new(new DbContextOptionsBuilder<TasksDbContext>().UseSqlite(_connection).Options);

    [Fact]
    public async Task Add_Then_GetByOwner_ReturnsOnlyThatOwnersTasks()
    {
        var alice = Guid.NewGuid();
        var bob = Guid.NewGuid();
        var repo = new TaskRepository(NewContext());
        await repo.AddAsync(TaskItem.Create("Alice one", null, Today.AddDays(3), alice, Today));
        await repo.AddAsync(TaskItem.Create("Bob one", null, Today.AddDays(1), bob, Today));

        var aliceTasks = await new TaskRepository(NewContext()).GetByOwnerAsync(alice);

        aliceTasks.Should().ContainSingle().Which.Title.Should().Be("Alice one");
    }

    [Fact]
    public async Task Update_PersistsStatusChange()
    {
        var repo = new TaskRepository(NewContext());
        var task = TaskItem.Create("Ship it", null, Today.AddDays(5), Guid.NewGuid(), Today);
        await repo.AddAsync(task);

        var loaded = await new TaskRepository(NewContext()).GetByIdAsync(task.Id);
        loaded!.MoveTo(TaskState.InProgress);
        await new TaskRepository(NewContext()).UpdateAsync(loaded);

        var reloaded = await new TaskRepository(NewContext()).GetByIdAsync(task.Id);
        reloaded!.Status.Should().Be(TaskState.InProgress);
    }

    public void Dispose()
    {
        _db.Dispose();
        _connection.Dispose();
        GC.SuppressFinalize(this);
    }
}
