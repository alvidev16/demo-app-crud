using Tasks.Domain.Entities;

namespace Tasks.Services.DTOs;

public record TaskDto(
    Guid Id,
    string Title,
    string? Description,
    TaskState Status,
    DateOnly DueDate,
    Guid OwnerUserId)
{
    public static TaskDto FromEntity(TaskItem t) =>
        new(t.Id, t.Title, t.Description, t.Status, t.DueDate, t.OwnerUserId);
}

public record CreateTaskDto(string Title, string? Description, DateOnly DueDate);

public record UpdateTaskDto(string Title, string? Description, DateOnly DueDate);

public record ChangeStatusDto(TaskState Status);
