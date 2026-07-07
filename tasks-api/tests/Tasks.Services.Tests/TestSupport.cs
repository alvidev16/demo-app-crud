using Tasks.Domain.Entities;

namespace Tasks.Services.Tests;

/// <summary>Deterministic clock for testing time-dependent rules (FR-5).</summary>
public sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
{
    public override DateTimeOffset GetUtcNow() => now;
}

/// <summary>Test Data Builder for <see cref="TaskItem"/> — start valid, override what matters.</summary>
public class TaskItemBuilder
{
    private string _title = "Write the report";
    private string? _description = "A sample task";
    private DateOnly _due = DateOnly.FromDateTime(DateTime.UtcNow.Date).AddDays(7);
    private Guid _owner = Guid.NewGuid();
    private DateOnly _today = DateOnly.FromDateTime(DateTime.UtcNow.Date);

    public TaskItemBuilder WithTitle(string title) { _title = title; return this; }
    public TaskItemBuilder WithDescription(string? d) { _description = d; return this; }
    public TaskItemBuilder DueOn(DateOnly due) { _due = due; return this; }
    public TaskItemBuilder OwnedBy(Guid owner) { _owner = owner; return this; }
    public TaskItemBuilder WithToday(DateOnly today) { _today = today; return this; }

    public TaskItem Build() => TaskItem.Create(_title, _description, _due, _owner, _today);
}
