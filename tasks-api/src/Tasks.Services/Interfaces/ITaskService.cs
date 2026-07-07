using Tasks.Domain.Entities;
using Tasks.Services.DTOs;

namespace Tasks.Services.Interfaces;

/// <summary>
/// Business operations for tasks. The caller's user id is passed explicitly so the
/// service can enforce ownership (FR-7) independently of the transport layer.
/// </summary>
public interface ITaskService
{
    Task<IReadOnlyList<TaskDto>> GetMineAsync(Guid userId, CancellationToken ct = default);
    Task<TaskDto> GetByIdAsync(Guid userId, Guid id, CancellationToken ct = default);
    Task<TaskDto> CreateAsync(Guid userId, CreateTaskDto dto, CancellationToken ct = default);
    Task<TaskDto> UpdateAsync(Guid userId, Guid id, UpdateTaskDto dto, CancellationToken ct = default);
    Task<TaskDto> ChangeStatusAsync(Guid userId, Guid id, TaskState next, CancellationToken ct = default);
    Task DeleteAsync(Guid userId, Guid id, CancellationToken ct = default);
}
