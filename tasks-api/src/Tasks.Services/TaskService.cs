using Tasks.Domain.Entities;
using Tasks.Domain.Exceptions;
using Tasks.Domain.Interfaces;
using Tasks.Services.DTOs;
using Tasks.Services.Interfaces;

namespace Tasks.Services;

/// <summary>
/// Task business logic: validation, status transitions and ownership enforcement,
/// over the repository abstraction. Depends on <see cref="TimeProvider"/> so the
/// "due date not in the past" rule (FR-5) is deterministically testable.
/// </summary>
public class TaskService : ITaskService
{
    private readonly ITaskRepository _repo;
    private readonly TimeProvider _clock;

    public TaskService(ITaskRepository repo, TimeProvider clock)
    {
        _repo = repo;
        _clock = clock;
    }

    private DateOnly Today => DateOnly.FromDateTime(_clock.GetUtcNow().UtcDateTime);

    public async Task<IReadOnlyList<TaskDto>> GetMineAsync(Guid userId, CancellationToken ct = default)
    {
        var tasks = await _repo.GetByOwnerAsync(userId, ct);
        return tasks.Select(TaskDto.FromEntity).ToList();
    }

    public async Task<TaskDto> GetByIdAsync(Guid userId, Guid id, CancellationToken ct = default)
        => TaskDto.FromEntity(await OwnedOrNotFound(userId, id, ct));

    public async Task<TaskDto> CreateAsync(Guid userId, CreateTaskDto dto, CancellationToken ct = default)
    {
        var task = TaskItem.Create(dto.Title, dto.Description, dto.DueDate, userId, Today);
        await _repo.AddAsync(task, ct);
        return TaskDto.FromEntity(task);
    }

    public async Task<TaskDto> UpdateAsync(Guid userId, Guid id, UpdateTaskDto dto, CancellationToken ct = default)
    {
        var task = await OwnedOrNotFound(userId, id, ct);
        task.UpdateDetails(dto.Title, dto.Description, dto.DueDate, Today);
        await _repo.UpdateAsync(task, ct);
        return TaskDto.FromEntity(task);
    }

    public async Task<TaskDto> ChangeStatusAsync(Guid userId, Guid id, TaskState next, CancellationToken ct = default)
    {
        var task = await OwnedOrNotFound(userId, id, ct);
        task.MoveTo(next); // throws ValidationException on an invalid transition (FR-4)
        await _repo.UpdateAsync(task, ct);
        return TaskDto.FromEntity(task);
    }

    public async Task DeleteAsync(Guid userId, Guid id, CancellationToken ct = default)
    {
        var task = await OwnedOrNotFound(userId, id, ct);
        await _repo.DeleteAsync(task, ct);
    }

    /// <summary>
    /// FR-7 / NFR-3: a task that doesn't exist OR isn't owned by the caller is reported
    /// as "not found" — the API never confirms the existence of another user's task.
    /// </summary>
    private async Task<TaskItem> OwnedOrNotFound(Guid userId, Guid id, CancellationToken ct)
    {
        var task = await _repo.GetByIdAsync(id, ct);
        if (task is null || task.OwnerUserId != userId)
            throw new NotFoundException(nameof(TaskItem), id);
        return task;
    }
}
