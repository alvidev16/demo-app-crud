using FluentAssertions;
using Tasks.Domain.Entities;
using Tasks.Domain.Exceptions;

namespace Tasks.Services.Tests;

/// <summary>Domain rules for the TaskItem entity (T-02, T-04).</summary>
public class TaskItemTests
{
    private static readonly DateOnly Today = new(2026, 1, 1);
    private static readonly DateOnly Tomorrow = Today.AddDays(1);

    // ---- Creation (AC-1, AC-2, AC-3) ----

    [Fact] // AC-1
    public void Create_WithValidData_StartsAsTodo()
    {
        var task = new TaskItemBuilder().WithToday(Today).DueOn(Tomorrow).Build();

        task.Status.Should().Be(TaskState.Todo);
        task.Id.Should().NotBeEmpty();
    }

    [Theory] // AC-2
    [InlineData("")]
    [InlineData("ab")]   // too short
    public void Create_WithInvalidTitle_ThrowsValidation(string title)
    {
        var act = () => new TaskItemBuilder().WithToday(Today).DueOn(Tomorrow).WithTitle(title).Build();

        act.Should().Throw<ValidationException>().Which.Errors.Should().ContainKey("Title");
    }

    [Fact] // AC-2: title over 120 chars
    public void Create_WithTooLongTitle_ThrowsValidation()
    {
        var act = () => new TaskItemBuilder().WithToday(Today).DueOn(Tomorrow)
            .WithTitle(new string('x', 121)).Build();

        act.Should().Throw<ValidationException>().Which.Errors.Should().ContainKey("Title");
    }

    [Fact] // AC-2
    public void Create_WithTooLongDescription_ThrowsValidation()
    {
        var act = () => new TaskItemBuilder().WithToday(Today).DueOn(Tomorrow)
            .WithDescription(new string('x', 1001)).Build();

        act.Should().Throw<ValidationException>().Which.Errors.Should().ContainKey("Description");
    }

    [Fact] // AC-3
    public void Create_WithPastDueDate_ThrowsValidation()
    {
        var act = () => new TaskItemBuilder().WithToday(Today).DueOn(Today.AddDays(-1)).Build();

        act.Should().Throw<ValidationException>().Which.Errors.Should().ContainKey("DueDate");
    }

    // ---- Status state machine (AC-4, AC-5, FR-4) ----

    [Fact] // AC-5
    public void MoveTo_TodoToInProgressToDone_Succeeds()
    {
        var task = new TaskItemBuilder().WithToday(Today).DueOn(Tomorrow).Build();

        task.MoveTo(TaskState.InProgress);
        task.MoveTo(TaskState.Done);

        task.Status.Should().Be(TaskState.Done);
    }

    [Fact] // AC-4
    public void MoveTo_TodoDirectlyToDone_ThrowsValidation()
    {
        var task = new TaskItemBuilder().WithToday(Today).DueOn(Tomorrow).Build();

        var act = () => task.MoveTo(TaskState.Done);

        act.Should().Throw<ValidationException>();
        task.Status.Should().Be(TaskState.Todo);
    }

    [Fact] // FR-4: reopen
    public void MoveTo_InProgressBackToTodo_Succeeds()
    {
        var task = new TaskItemBuilder().WithToday(Today).DueOn(Tomorrow).Build();
        task.MoveTo(TaskState.InProgress);

        task.MoveTo(TaskState.Todo);

        task.Status.Should().Be(TaskState.Todo);
    }

    [Fact] // FR-4: Done is terminal
    public void MoveTo_DoneToAnything_Throws()
    {
        var task = new TaskItemBuilder().WithToday(Today).DueOn(Tomorrow).Build();
        task.MoveTo(TaskState.InProgress);
        task.MoveTo(TaskState.Done);

        var act = () => task.MoveTo(TaskState.InProgress);

        act.Should().Throw<ValidationException>();
    }
}
