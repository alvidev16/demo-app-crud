using Microsoft.EntityFrameworkCore;
using Tasks.Domain.Entities;
using Tasks.Domain.Interfaces;

namespace Tasks.DAL;

/// <summary>EF Core implementation of <see cref="ITaskRepository"/>.</summary>
public class TaskRepository : ITaskRepository
{
    private readonly TasksDbContext _db;

    public TaskRepository(TasksDbContext db) => _db = db;

    public async Task<IReadOnlyList<TaskItem>> GetByOwnerAsync(Guid ownerUserId, CancellationToken ct = default) =>
        await _db.Tasks.AsNoTracking()
            .Where(t => t.OwnerUserId == ownerUserId)
            .OrderBy(t => t.DueDate)
            .ToListAsync(ct);

    public async Task<TaskItem?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await _db.Tasks.FirstOrDefaultAsync(t => t.Id == id, ct);

    public async Task AddAsync(TaskItem task, CancellationToken ct = default)
    {
        await _db.Tasks.AddAsync(task, ct);
        await _db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(TaskItem task, CancellationToken ct = default)
    {
        _db.Tasks.Update(task);
        await _db.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(TaskItem task, CancellationToken ct = default)
    {
        _db.Tasks.Remove(task);
        await _db.SaveChangesAsync(ct);
    }
}
