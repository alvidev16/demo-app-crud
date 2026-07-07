using Tasks.Domain.Exceptions;

namespace Tasks.Domain.Entities;

/// <summary>Workflow status of a task. Named TaskState to avoid colliding with System.Threading.Tasks.TaskStatus.</summary>
public enum TaskState
{
    Todo,
    InProgress,
    Done
}

/// <summary>
/// A task/work item. Named TaskItem to avoid colliding with System.Threading.Tasks.Task.
/// Invariants (title/description bounds, due date, status transitions) live here so a
/// task can never exist in an invalid state. (Satisfies FR-1..FR-5.)
/// </summary>
public class TaskItem
{
    public Guid Id { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public TaskState Status { get; private set; }
    public DateOnly DueDate { get; private set; }
    public Guid OwnerUserId { get; private set; }

    // Required by EF Core for materialization.
    private TaskItem() { }

    /// <summary>FR-2, FR-3, FR-5: validated creation; new tasks start as Todo.</summary>
    public static TaskItem Create(string title, string? description, DateOnly dueDate,
                                  Guid ownerUserId, DateOnly today)
    {
        Validate(title, description, dueDate, today);
        return new TaskItem
        {
            Id = Guid.NewGuid(),
            Title = title.Trim(),
            Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim(),
            Status = TaskState.Todo,
            DueDate = dueDate,
            OwnerUserId = ownerUserId
        };
    }

    /// <summary>Update editable details. Status is changed only via <see cref="MoveTo"/>.</summary>
    public void UpdateDetails(string title, string? description, DateOnly dueDate, DateOnly today)
    {
        // On update, allow a due date that has already passed only if it is unchanged;
        // a *new* due date must not be in the past.
        Validate(title, description, dueDate, dueDate == DueDate ? dueDate : today);
        Title = title.Trim();
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        DueDate = dueDate;
    }

    // FR-4: allowed status transitions. Anything not listed is rejected.
    private static readonly Dictionary<TaskState, TaskState[]> AllowedTransitions = new()
    {
        [TaskState.Todo] = [TaskState.InProgress],
        [TaskState.InProgress] = [TaskState.Done, TaskState.Todo],
        [TaskState.Done] = []
    };

    /// <summary>FR-4: move through the status state machine, rejecting invalid transitions.</summary>
    public void MoveTo(TaskState next)
    {
        if (next == Status)
            return;
        if (!AllowedTransitions[Status].Contains(next))
            throw new ValidationException(nameof(Status), $"Cannot move a task from {Status} to {next}.");
        Status = next;
    }

    private static void Validate(string title, string? description, DateOnly dueDate, DateOnly notBefore)
    {
        var errors = new Dictionary<string, string>();

        if (string.IsNullOrWhiteSpace(title) || title.Trim().Length is < 3 or > 120)
            errors[nameof(Title)] = "Title is required and must be between 3 and 120 characters.";

        if (description is not null && description.Length > 1000)
            errors[nameof(Description)] = "Description must be at most 1000 characters.";

        if (dueDate < notBefore)
            errors[nameof(DueDate)] = "Due date cannot be in the past.";

        if (errors.Count > 0)
            throw new ValidationException(errors);
    }
}
