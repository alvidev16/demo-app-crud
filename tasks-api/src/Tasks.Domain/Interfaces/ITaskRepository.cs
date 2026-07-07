using Tasks.Domain.Entities;

namespace Tasks.Domain.Interfaces;

/// <summary>Data-access contract for tasks. Declared in the Domain (Clean dependency rule).</summary>
public interface ITaskRepository
{
    Task<IReadOnlyList<TaskItem>> GetByOwnerAsync(Guid ownerUserId, CancellationToken ct = default);
    Task<TaskItem?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(TaskItem task, CancellationToken ct = default);
    Task UpdateAsync(TaskItem task, CancellationToken ct = default);
    Task DeleteAsync(TaskItem task, CancellationToken ct = default);
}
